using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace Uptred.Mobile
{
    [Activity (Label = "Uptred Mobile", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
            
			FindViewById<Button> (Resource.Id.btnAuthVimeo).Click += delegate {
                StartActivity(new Intent(this, typeof(VimeoPanelActivity)));
            };

            FindViewById<Button>(Resource.Id.btnAuthYouTube).Click += delegate {
                StartActivity(new Intent(this, typeof(YouTubePanelActivity)));
            };
        }

		protected override void OnStart ()
		{
			base.OnStart ();
			NotificationHandler.CancelNotification (this);
		}

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            MoveTaskToBack(true);
        }
    }
}