using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Uri = Android.Net.Uri;
using System.IO;
using Uptred.YouTube;

namespace Uptred.Mobile
{
    [Activity(Label = "Uptred Mobile")]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "http", DataHost = "youtube.auth.uptred.com", DataPathPattern =".*")]
    public class YouTubePanelActivity : Activity
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
                        var info = new YouTubeUploadTask();
                        info.Path = path;
                        info.Url = Settings.YouTubeHook.GetUploadSessionUrl(info.Path);
                        Settings.YouTubeInfo = info;
                        Settings.SaveInfos();
                        StartActivity(new Intent(this, typeof(YouTubeUploadActivity)));
                    }
                    catch (Exception e)
                    {
                        //Getting ticket failed, possibly because YouTubeHook is null or
                        //its access is denied. Switch to main screen.
                        Console.WriteLine(e.Message);
                        StartActivity(new Intent(this, typeof(MainActivity)));
                    }
                };

                PickFileActivity.OnCancelAction = () =>
                    StartActivity(new Intent(this, typeof(YouTubePanelActivity)));

                StartActivity(new Intent(this, typeof(PickFileActivity)));
            };

            FindViewById<Button>(Resource.Id.btnUploadResume).Visibility =
                (Settings.YouTubeInfo != null && !Settings.YouTubeInfo.Done) ? ViewStates.Visible : ViewStates.Gone;

            FindViewById<Button>(Resource.Id.btnUploadResume).Click += delegate
            {
                //Resume Upload: Open upload activity
                var info = Settings.YouTubeInfo;
                if (File.Exists(info.Path))
                {
                    StartActivity(new Intent(this, typeof(YouTubeUploadActivity)));
                }
            };
        }

        protected override void OnStart()
        {
            base.OnStart();
			NotificationHandler.CancelNotification (this);
            YouTubeHook.VerboseCallback = (o) => Console.WriteLine(o);
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
                Settings.YouTubeHook = null;
            }
            if (code != string.Empty)
            {
                try
                {
                    Settings.YouTubeHook = YouTubeHook.Authorize(
                        authCode: code,
                        clientId: ApiKeys.YouTubeClientId,
                        secret: ApiKeys.YouTubeClientSecret,
                        redirect: ApiKeys.YouTubeRedirectURL);
                    Console.WriteLine("Logged in as " + Settings.YouTubeHook.DisplayName);
                    Settings.YouTubeRefreshToken = Settings.YouTubeHook.RefreshToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Settings.YouTubeHook = null;
                }
            }
            else
            {
                try
                {
                    Settings.YouTubeHook = YouTubeHook.ReAuthorize(
                        refreshToken: Settings.YouTubeRefreshToken,
                        clientId: ApiKeys.YouTubeClientId,
                        secret: ApiKeys.YouTubeClientSecret,
                        redirect: ApiKeys.YouTubeRedirectURL);
                    Console.WriteLine("Logged in as " + Settings.YouTubeHook.DisplayName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Settings.YouTubeHook = null;
                }
            }

            if (Settings.YouTubeHook == null)
            {
                StartActivity(new Intent(Intent.ActionView, Uri.Parse(
                    YouTubeHook.GetLoginURL(clientId: ApiKeys.YouTubeClientId, redirect: ApiKeys.YouTubeRedirectURL))));
            }
            else
            {
                FindViewById<TextView>(Resource.Id.lblVerifier).Text = Settings.YouTubeHook.DisplayName;
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            StartActivity(new Intent(this, typeof(MainActivity)));
        }
    }
}