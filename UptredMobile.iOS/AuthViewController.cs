using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Diagnostics;
using Uptred.Vimeo;
using Uptred.YouTube;
using SimpleJSON;
using System.Threading.Tasks;

namespace Uptred.Mobile
{
	partial class AuthViewController : UIViewController
	{
		public string NavigateUrl;
		public string Provider;
		UIWebView webView = null;

		public AuthViewController () : base ()
		{
			
		}
			
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.White;
			Title = "Login";

			webView = new UIWebView (View.Bounds);
			View.AddSubview(webView);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			webView.LoadRequest (NSUrlRequest.FromUrl (NSUrl.FromString (NavigateUrl)));
			webView.ScalesPageToFit = true;
			webView.LoadFinished += WebView_LoadFinished;
		}

		async void WebView_LoadFinished (object sender, EventArgs e)
		{
			var url = webView.EvaluateJavascript("window.location.href");
			var match = Provider == "Vimeo" ? Constants.VimeoRedirectURL : Constants.YouTubeRedirectURL;
			if (!url.StartsWith (match))
				return;
			var split = url.Split ('=');
			if (split.Length == 0)
				return;
			var code = split [split.Length - 1];
			foreach (var view in View.Subviews)
				view.RemoveFromSuperview ();

			var text = new UILabel (new CoreGraphics.CGRect(0, 0, 400, 90));
			text.Text = "Loading...";
			text.TextAlignment = UITextAlignment.Center;
			text.Center = View.Center;
			text.TextColor = UIColor.Black;
			View.Add (text);

			InvokeInBackground (delegate {
				if (Provider == "Vimeo") {
					var hook = VimeoHook.Authorize (
						authCode: code,
						clientId: Constants.VimeoAPIKey,
						secret: Constants.VimeoAPISecret,
						redirect: Constants.VimeoRedirectURL);
					InvokeOnMainThread( delegate {
						text.Text = string.Format ("Logged in as {0}!", hook.User ["name"].Value);
					});
				}
				else if (Provider == "YouTube") {
					var hook = YouTubeHook.Authorize(
						authCode: code,
						clientId: Constants.YouTubeAPIKey,
						secret: Constants.YouTubeAPISecret,
						redirect: Constants.YouTubeRedirectURL);
					InvokeOnMainThread( delegate {
						text.Text = string.Format ("Logged in as {0}!", hook.DisplayName);
					});
				}
			});
		}
	}
}
