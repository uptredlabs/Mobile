using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace Uptred.Mobile
{
    [Activity(Label = "Uptred Mobile")]
    public class VimeoUploadActivity : UploadActivityBase
    {
        protected override bool IsDone()
        {
            return Settings.VimeoInfo.Done;
        }

        protected override void SetDone()
        {
            Settings.VimeoInfo.Done = true;
        }

        protected override void OnUploadResume()
        {
            if (!Settings.VimeoInfo.Done) Upload();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            FindViewById<EditText>(Resource.Id.txtTitle).Text = Settings.VimeoInfo.Meta.Title;
            FindViewById<EditText>(Resource.Id.txtDesc).Text = Settings.VimeoInfo.Meta.Description;
            FindViewById<CheckBox>(Resource.Id.chkPublic).Checked = Settings.VimeoInfo.Meta.PrivacyView == "anybody";

            var fi = new System.IO.FileInfo(Settings.VimeoInfo.Path);
            updatePercentage(Settings.VimeoInfo.LastByte, fi.Length);
        }

        protected override void onTitleChange(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            base.onTitleChange(sender, e);
            Settings.VimeoInfo.Meta.Title = FindViewById<EditText>(Resource.Id.txtTitle).Text;
            Settings.SaveInfos();
        }

        protected override void onDescChange(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            base.onDescChange(sender, e);
            Settings.VimeoInfo.Meta.Description = FindViewById<EditText>(Resource.Id.txtDesc).Text;
            Settings.SaveInfos();
        }

        protected override void onPublicChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            base.onPublicChange(sender, e);
            Settings.VimeoInfo.Meta.PrivacyView = e.IsChecked ? "anybody" : "nobody";
            Settings.SaveInfos();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Settings.VimeoHook.UploadCallback = UploadCallback;
            if (!Settings.VimeoInfo.Done && !paused) Upload();
        }

        void UploadCallback(Uptred.VerifyFeedback feedback)
        {
            this.RunOnUiThread(() =>
            {
                updatePercentage(feedback.LastByte, feedback.ContentSize);
                if (Settings.VimeoInfo.LastByte >= feedback.LastByte)
                {
                    Console.WriteLine(string.Format("No bytes uploaded. Retries: {0}", _retries));
                    _retries++;
                    if (_retries > 3)
                    {
                        try
                        {
                            var ticket = Settings.VimeoHook.GetTicket();
                            if (ticket != null)
                            {
                                Settings.VimeoInfo.Ticket = ticket;
                                Settings.VimeoInfo.LastByte = 0;
                                _retries = 0;
                                Settings.SaveInfos();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Terminating because of too many retries.");
                            Console.WriteLine(e.Message);
                            paused = true;
                            _retries = 0;
                            return;
                        }
                    }
                }
                Settings.VimeoInfo.LastByte = feedback.LastByte;
                if (Settings.VimeoInfo.VideoId != null && Settings.VimeoInfo.VideoId != "")
                {
                    //Upload Completed.
                    Console.WriteLine(string.Format("Upload completed. Video ID: {0}. Applying Metadata...", Settings.VimeoInfo.VideoId));
                    FindViewById<TextView>(Resource.Id.txtProgress).Text = "Applying Metadata...";
                }

                //_fraction = 0;
                Settings.SaveInfos();
            });
        }

        async void Upload()
        {
            Console.WriteLine("Upload Module Starting");
            uploading = true;
            try
            {
                while (!Settings.VimeoInfo.Done)
                {
                    if (paused)
                    {
                        uploading = false;
                        RunOnUiThread(updatePercentage);
                        return;
                    }
                    Console.WriteLine(string.Format("Uploading {0}", Settings.VimeoInfo.Path));
                    var fi = new System.IO.FileInfo(Settings.VimeoInfo.Path);
                    var contentSize = fi.Length;
                    await Task.Run(() =>
                    {
                        Uptred.Core.UpstreamCallback = (lastByte, totalSize) =>
                        {
                            _fraction = lastByte;
                            RunOnUiThread(updatePercentage);
                        };

                        Settings.VimeoInfo.VideoId =
                        Settings.VimeoHook.Upload(
                            Settings.VimeoInfo.Path,
                            ticket: Settings.VimeoInfo.Ticket,
                            chunkSize: 720 * 1024, //720kB
                            startByte: Settings.VimeoInfo.LastByte,
                            step: true);
                        Uptred.Core.UpstreamCallback = null;
                    });
                    if (Settings.VimeoInfo.VideoId == "")
                    {
                        //Step
                        //return;
                    }
                    else if (Settings.VimeoInfo.VideoId == null)
                    {
                        paused = true;
                        Console.WriteLine("Error in uploading.");
                        return;
                    }
                    else
                    {
                        Settings.VimeoInfo.Done = true;
                        Settings.SaveInfos();
                        break;
                    }
                }

                if (Settings.VimeoInfo.Done)
                {
                    Settings.VimeoInfo.Meta.SetMetadata(Settings.VimeoInfo.VideoId, Settings.VimeoHook);
                    var sp = Settings.VimeoInfo.VideoId.Split('/');
                    UploadFinishActivity.Text = string.Format("http://vimeo.com/{0}", sp[sp.Length - 1]);
                    RunOnUiThread(() => StartActivity(new Intent(this, typeof(UploadFinishActivity))));
                    return;
                }
                uploading = false;
                RunOnUiThread(updatePercentage);
            }
            catch (Exception e)
            {
                paused = true;
                Console.WriteLine("Error in uploading.");
                Console.WriteLine(e.Message);
                return;
            }
        }
    }
}