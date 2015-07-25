using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Uri = Android.Net.Uri;
using System.IO;

namespace Uptred.Mobile
{
    [Activity(Label = "Uptred Mobile")]
    public class UploadFinishActivity : Activity
    {
        public static string Text = "";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UploadFinishedPanel);
            FindViewById<TextView>(Resource.Id.txtVideoUrl).Text = Text;
            StartActivity(new Intent(Intent.ActionView, Uri.Parse(Text)));

			NotificationHandler.UpdateNotification (this, "Upload Complete!");
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            StartActivity(new Intent(this, typeof(MainActivity)));
			NotificationHandler.CancelNotification (this);
        }
    }
}