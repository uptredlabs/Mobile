using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace Uptred.Mobile
{
    [Activity(Label = "Uptred Mobile")]
    public class UploadActivityBase : Activity
    {
        protected bool uploading = false;
        protected bool shown = true;
        protected bool created = false;
        protected bool _paused = false;
        protected bool paused
        {
            get
            {
                return _paused;
            }
            set
            {
                if (_paused && !value)
                {
                    //Was paused, and resumed. Start uploading.
                    _paused = value;
                    RunOnUiThread(OnUploadResume);
                }
                _paused = value;
                RunOnUiThread(() =>
                {
                    if (_paused)
                        Uptred.Core.Halt();
                    FindViewById<Button>(Resource.Id.btnPause).Text = _paused ? "Resume" : "Pause";
                });
            }
        }

        protected string videoId = null;
        protected long _last, _total, _fraction = 0;
        protected int _retries = 0;

        protected virtual void OnUploadResume()
        {
        }

        protected virtual bool IsDone()
        {
            return true;
        }

        protected virtual void SetDone()
        {

        }

        protected void updatePercentage()
        {
            if (!created) return;
            if (IsDone())
            {
                if (shown)
                {
                    FindViewById<ProgressBar>(Resource.Id.progressBar).Progress = 100;
                    FindViewById<TextView>(Resource.Id.txtProgress).Text = "Upload Completed!";
                }
            }
            else if (paused)
            {
                var text = uploading ? "Pausing..." : "Upload Paused.";
                if (shown)
                    FindViewById<TextView>(Resource.Id.txtProgress).Text = text;
                NotificationHandler.UpdateNotification(this, text);
            }
            else if (!uploading)
            {
                //This should never happen.
                if (shown)
                    FindViewById<TextView>(Resource.Id.txtProgress).Text = "Uploader is Idle.";
                NotificationHandler.CancelNotification(this);
            }
            else
            {
                decimal percent = Math.Min(100, 100 * (decimal)(_last + _fraction) / (decimal)(_total + 1));
                if (percent >= 100) percent = 100;

                var text = string.Format("{0}% Uploaded", (percent).ToString("N2"));
                if (shown)
                {
                    FindViewById<ProgressBar>(Resource.Id.progressBar).Progress = (int)percent;
                    FindViewById<TextView>(Resource.Id.txtProgress).Text = text;
                }
                NotificationHandler.UpdateNotification(this, text);
            }
        }

        protected void updatePercentage(long last, long total)
        {
            _last = last;
            _total = total;
            updatePercentage();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Settings.LoadInfos();
            SetContentView(Resource.Layout.UploaderPanel);

            FindViewById<EditText>(Resource.Id.txtTitle).AfterTextChanged += onTitleChange;
            FindViewById<EditText>(Resource.Id.txtDesc).AfterTextChanged += onDescChange;
            FindViewById<CheckBox>(Resource.Id.chkPublic).CheckedChange += onPublicChange;
            FindViewById<Button>(Resource.Id.btnPause).Click += onPauseClick;
            created = true;
        }

        protected virtual void onPauseClick(object sender, EventArgs e)
        {
            paused = !paused;
            updatePercentage();
        }

        protected virtual void onTitleChange(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
        }

        protected virtual void onDescChange(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
        }

        protected virtual void onPublicChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
        }

        protected override void OnPause()
        {
            shown = false;
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            shown = true;
            updatePercentage();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            paused = true;
            StartActivity(new Intent(this, typeof(MainActivity)));
        }
    }
}

