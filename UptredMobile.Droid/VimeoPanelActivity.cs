using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Uptred.Vimeo;
using Uri = Android.Net.Uri;
using System.IO;

namespace Uptred.Mobile
{
    [Activity(Label = "Uptred Mobile")]
    [IntentFilter(new[] { Intent.ActionView }, 
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "uptred", DataHost = "vimeo")]
    public class VimeoPanelActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoggedInPanel);
            Settings.LoadInfos();

            FindViewById<Button>(Resource.Id.btnUploadNew).Click += delegate
            {
                PickFileActivity.OnFinishAction = (path) =>
                {
                    try
                    {
                        //New Upload: Get ticket, do meta, open upload activity
                        var info = new VimeoUploadInformation();
                        info.Path = path;
                        info.Ticket = Settings.VimeoHook.GetTicket();
                        Settings.VimeoInfo = info;
                        Settings.SaveInfos();
                        StartActivity(new Intent(this, typeof(VimeoUploadActivity)));
                    }
                    catch (Exception e)
                    {
                        //Getting ticket failed, possibly because VimeoHook is null or
                        //its access is denied. Switch to main screen.
                        Console.WriteLine(e.Message);
                        StartActivity(new Intent(this, typeof(MainActivity)));
                    }
                };

                PickFileActivity.OnCancelAction = () => 
                    StartActivity(new Intent(this, typeof(VimeoPanelActivity)));

                StartActivity(new Intent(this, typeof(PickFileActivity)));
            };

            FindViewById<Button>(Resource.Id.btnUploadResume).Visibility =
                (Settings.VimeoInfo != null && !Settings.VimeoInfo.Done) ? ViewStates.Visible : ViewStates.Gone;

            FindViewById<Button>(Resource.Id.btnUploadResume).Click += delegate
            {
                //Resume Upload: Open upload activity
                VimeoUploadInformation info = Settings.VimeoInfo;
                if (File.Exists(info.Path))
                {
                    StartActivity(new Intent(this, typeof(VimeoUploadActivity)));
                }
            };
        }

        protected override void OnStart()
        {
            base.OnStart();
			NotificationHandler.CancelNotification (this);
            VimeoHook.VerboseCallback = (o) => Console.WriteLine(o);
            Settings.LoadInfos();

            var code = string.Empty;
            try
            {
                var urlparams = Uptred.Core.QueryParametersFromUrl(Intent.Data.ToString());
                code = urlparams["code"];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Settings.VimeoHook = null;
            }
            if (code != string.Empty)
            {
                try
                {
                    Settings.VimeoHook = VimeoHook.Authorize(
                        authCode: code, 
                        clientId: ApiKeys.VimeoClientId, 
                        secret: ApiKeys.VimeoClientSecret, 
                        redirect: ApiKeys.VimeoRedirectURL);
                    Console.WriteLine("Logged in as " + Settings.VimeoHook.User["name"].ToString());
                    Settings.VimeoAccessToken = Settings.VimeoHook.AccessToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Settings.VimeoHook = null;
                }
            }
            else
            {
                try
                {
                    Settings.VimeoHook = VimeoHook.ReAuthorize(
                        accessToken: Settings.VimeoAccessToken,
                        clientId: ApiKeys.VimeoClientId,
                        secret: ApiKeys.VimeoClientSecret,
                        redirect: ApiKeys.VimeoRedirectURL);
                    Console.WriteLine("Logged in as " + Settings.VimeoHook.User["name"].ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Settings.VimeoHook = null;
                }
            }

            if (Settings.VimeoHook == null)
            {
                StartActivity(new Intent(Intent.ActionView, Uri.Parse(
                    VimeoHook.GetLoginURL(clientId: ApiKeys.VimeoClientId, redirect: ApiKeys.VimeoRedirectURL))));
            }
            else
            {
                FindViewById<TextView>(Resource.Id.lblVerifier).Text = Settings.VimeoHook.User["name"].ToString();
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            StartActivity(new Intent(this, typeof(MainActivity)));
        }
    }
}