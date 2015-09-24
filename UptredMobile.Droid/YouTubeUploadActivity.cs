using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Uri = Android.Net.Uri;

namespace Uptred.Mobile
{
    [Activity(Label = "Uptred Mobile")]
	public class YouTubeUploadActivity : UploadActivityBase
    {
        protected override bool IsDone()
        {
            return Settings.YouTubeInfo.Done;
        }

        protected override void SetDone()
        {
            Settings.YouTubeInfo.Done = true;
        }

        protected override void OnUploadResume ()
		{
			base.OnUploadResume ();
			if (!Settings.YouTubeInfo.Done)
				Upload ();
		}

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            FindViewById<EditText>(Resource.Id.txtTitle).Text = Settings.YouTubeInfo.Meta.Title;
            FindViewById<EditText>(Resource.Id.txtDesc).Text = Settings.YouTubeInfo.Meta.Description;
            FindViewById<CheckBox>(Resource.Id.chkPublic).Checked = Settings.YouTubeInfo.Meta.PrivacyStatus == "public";

            var fi = new System.IO.FileInfo(Settings.YouTubeInfo.Path);
			updatePercentage (Settings.YouTubeInfo.LastByte, fi.Length);
        }
        
		protected override void onTitleChange(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
			base.onTitleChange(sender, e);
            Settings.YouTubeInfo.Meta.Title = FindViewById<EditText>(Resource.Id.txtTitle).Text;
            Settings.SaveInfos();
        }

        protected override void onDescChange(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
			base.onDescChange(sender, e);
            Settings.YouTubeInfo.Meta.Description = FindViewById<EditText>(Resource.Id.txtDesc).Text;
            Settings.SaveInfos();
        }

		protected override void onPublicChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
			base.onPublicChange(sender, e);
            Settings.YouTubeInfo.Meta.PrivacyStatus = e.IsChecked ? "public" : "private";
            Settings.SaveInfos();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Settings.YouTubeHook.UploadCallback = UploadCallback;
            if (!Settings.YouTubeInfo.Done && !paused) Upload();
        }
        
        void UploadCallback(Uptred.VerifyFeedback feedback)
        {
            this.RunOnUiThread(() =>
            {
                //Crashes if is not on main thread
                updatePercentage (feedback.LastByte, feedback.ContentSize);
                if (Settings.YouTubeInfo.LastByte >= feedback.LastByte)
                {
                    Console.WriteLine(string.Format("No bytes uploaded. Retries: {0}", _retries));
                    _retries++;
                    if (_retries > 3)
                    {
                        try
                        {
                            var url = Settings.YouTubeHook.GetUploadSessionUrl(Settings.YouTubeInfo.Path);
                            if (!Core.IsNullOrWhiteSpace(url))
                            {
                                Settings.YouTubeInfo.Url = url;
                                Settings.YouTubeInfo.LastByte = 0;
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
                Settings.YouTubeInfo.LastByte = feedback.LastByte;
                if (feedback.LastByte >= feedback.ContentSize)
                {
                    //Upload Completed.
                    Console.WriteLine(string.Format("Upload completed. Video ID: {0}. Applying Metadata...", videoId));
                    FindViewById<TextView>(Resource.Id.txtProgress).Text = "Applying Metadata...";
                }

                _fraction = 0;
                Settings.SaveInfos();
            });
        }

        async void Upload()
        {
			Console.WriteLine ("Upload Module Starting");
            uploading = true;
            try
            {
                while (!Settings.YouTubeInfo.Done)
                {
                    if (paused) 
					{
						uploading = false;
						RunOnUiThread(updatePercentage);
						return;
					}
                    Console.WriteLine(string.Format("Uploading {0}", Settings.YouTubeInfo.Path));
                    var fi = new System.IO.FileInfo(Settings.YouTubeInfo.Path);
                    var contentSize = fi.Length;
                    await Task.Run(() =>
                    {
							Uptred.Core.UpstreamCallback = (lastByte, totalSize) =>
							{
								_fraction=lastByte;
								RunOnUiThread(updatePercentage);
							};
                        	videoId =
                            Settings.YouTubeHook.Upload(
                                Settings.YouTubeInfo.Path,
                                url: Settings.YouTubeInfo.Url,
                                chunkSize: 720 * 1024, //720kB
                                startByte: Settings.YouTubeInfo.LastByte,
                                step: true);
							Uptred.Core.UpstreamCallback = null;
                    });
                    if (videoId == "")
                    {
                        //Step
                        //return;
                    }
                    else if (videoId == null)
                    {
                        paused = true;
                        Console.WriteLine("Error in uploading.");
                        return;
                    }
                    else
                    {
                        Settings.YouTubeInfo.Done = true;
                        break;
                    }
                }

                if (Settings.YouTubeInfo.Done)
                {
                    Settings.YouTubeHook.ApplyVideoMetadata(videoId, Settings.YouTubeInfo.Meta);
                    UploadFinishActivity.Text = string.Format("http://youtube.com/{0}", videoId);
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
				Console.WriteLine (e.Message);
                return;
            }
        }
    }
}