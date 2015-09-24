using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using Uptred.Vimeo;
using Uptred.YouTube;
using System.Drawing;
using CoreGraphics;

namespace Uptred.Mobile
{
	partial class MainViewController : UIViewController
	{
		public MainViewController () : base ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.White;
			CGRect bounds;

			Title = "Uptred";

			//Image
			var imgLogo = new UIImageView(UIImage.FromFile("logotransparent.png"));
			bounds = new CGRect (0, 0, View.Bounds.Width * 0.5f, View.Bounds.Width * 0.5f);
			bounds.X = (View.Bounds.Width - bounds.Width) * 0.5f;
			bounds.Y = (View.Bounds.Height - bounds.Height) * 0.45f;
			imgLogo.Center = View.Center;
			View.AddSubview (imgLogo);

			//BtnAuthVimeo
			var btnAuthVimeo = new UIButton (UIButtonType.System);
			btnAuthVimeo.SetTitle ("Authorize Vimeo Account", UIControlState.Normal);
			btnAuthVimeo.BackgroundColor = UIColor.DarkGray;
			btnAuthVimeo.SetTitleColor (UIColor.White, UIControlState.Normal);
			bounds = new CGRect (0,0,400,40);
			bounds.X = (View.Bounds.Width - bounds.Width) * 0.5f;
			bounds.Y = View.Bounds.Height - bounds.Height * 3.5f;
			btnAuthVimeo.Frame = bounds;
			btnAuthVimeo.TouchUpInside += BtnAuthVimeo_TouchUpInside;
			View.AddSubview (btnAuthVimeo);

			//BtnAuthYouTube
			var btnAuthYouTube = new UIButton(UIButtonType.System);
			btnAuthYouTube.SetTitle ("Authorize YouTube Account", UIControlState.Normal);
			btnAuthYouTube.BackgroundColor = UIColor.DarkGray;
			btnAuthYouTube.SetTitleColor (UIColor.White, UIControlState.Normal);
			bounds = new CGRect(0,0,400,40);
			bounds.X = (View.Bounds.Width - bounds.Width) * 0.5f;
			bounds.Y = View.Bounds.Height - bounds.Height * 2;
			btnAuthYouTube.Frame = bounds;
			btnAuthYouTube.TouchUpInside += BtnAuthYouTube_TouchUpInside;
			View.AddSubview (btnAuthYouTube);

		}

		void BtnAuthVimeo_TouchUpInside (object sender, EventArgs e)
		{
			switchToAuth("Vimeo", VimeoHook.GetLoginURL(
				clientId: Constants.VimeoAPIKey, 
				redirect: Constants.VimeoRedirectURL));
		}

		void BtnAuthYouTube_TouchUpInside (object sender, EventArgs e)
		{
			switchToAuth("YouTube", YouTubeHook.GetLoginURL(
				clientId: Constants.YouTubeAPIKey,
				redirect: Constants.YouTubeRedirectURL));
		}

		void switchToAuth(string provider, string url)
		{
			var webScreen = new AuthViewController ();
			webScreen.Provider = provider;
			webScreen.NavigateUrl = url;
			NavigationController.PushViewController(webScreen, true);
		}
	}
}
