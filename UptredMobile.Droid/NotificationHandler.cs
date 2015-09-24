using System;
using Android.App;
using Android.Content;

namespace Uptred.Mobile
{
	public static class NotificationHandler
	{
		static Notification.Builder builder = null;
		const int notificationId = 0;

		public static void UpdateNotification(Activity activity, string text, string title = "Uptred Mobile")
		{
			if (builder == null) {
				builder = new Notification.Builder (activity)
				.SetSmallIcon (Resource.Drawable.logotransparent);
			}
			builder.SetContentTitle (title).SetContentText (text);

			// Build the notification:
			Notification notification = builder.Build();

			// Get the notification manager:
			NotificationManager notificationManager =
				activity.GetSystemService (Context.NotificationService) as NotificationManager;

			// Publish the notification:
			notificationManager.Notify (notificationId, notification);
		}

		public static void CancelNotification(Activity activity)
		{
			try
			{
				NotificationManager notificationManager =
					activity.GetSystemService (Context.NotificationService) as NotificationManager;
				notificationManager.Cancel(notificationId);
				builder = null;
			}
			catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}
	}
}

