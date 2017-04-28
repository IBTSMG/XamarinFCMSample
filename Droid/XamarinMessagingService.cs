using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Firebase.Messaging;
using Java.Lang;

namespace XamarinFCMSample.Droid
{
	[Service]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class XamarinMessagingService : FirebaseMessagingService
	{
		int notificationId = -1;
		const string TAG = "XamarinMessagingService";
		public override void OnMessageReceived(RemoteMessage message)
		{
			try
			{
				HandleNotification(message);
			}
			catch (System.Exception ex)
			{
				Log.Debug(TAG, "OnMessageReceivedFailed : {0}", ex);
			}
		}

		void SendNotification(RemoteMessage message)
		{
			HandleNotification(message);
		}

		private void HandleNotification(RemoteMessage message)
		{
			try
			{
				if (message.Data != null)
				{
					string userCode = string.Empty;
					message.Data.TryGetValue("USERKEY", out userCode);

					string username = string.Empty;
					message.Data.TryGetValue("USERNAME", out username);

					string body = string.Empty;
					message.Data.TryGetValue("body", out body);

					if (MainActivity.InBackground || Xamarin.Forms.Application.Current == null)
					{
						Notify(userCode, username, body);
					}
					else
					{
						var currentApp = (Xamarin.Forms.Application.Current as App);
						if (currentApp != null)
						{
							var notification = new NotificationInfo();

							notification.SenderUserCode = userCode;
							notification.SenderUserName = username;
							notification.Message = body;

							currentApp.NotificationReceived(notification);
						}
					}
				}
			}
			catch (System.Exception) { }
		}

		void Notify(string userCode, string username, string body)
		{
			var intent = new Intent(this, typeof(MainActivity));
			//intent.AddFlags(ActivityFlags.BroughtToFront);

			intent.PutExtra("NotificationUserID", userCode);
			intent.PutExtra("NotificationUserName", username);
			intent.PutExtra("NotificationMessage", body);


			var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);

			var notificationBuilder = new Notification.Builder(this)
				.SetSmallIcon(Resource.Drawable.icon)
				//.SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon))
				.SetContentTitle(username)
				.SetContentText(body)
				.SetAutoCancel(true)
				.SetContentIntent(pendingIntent);

			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				notificationBuilder.SetColor(Color.ParseColor("#62be1b"));
				//notificationBuilder.SetSmallIcon(Resource.Drawable.icon_alpha);
				//notificationBuilder.SetLargeIcon(BitmapFactory.decodeResource(context.getResources(), R.mipmap.ic_launcher));
			}

			notificationBuilder.SetPriority((int)NotificationPriority.High);
			//notificationBuilder.SetStyle(new Notification.BigTextStyle());
			notificationBuilder.SetWhen(JavaSystem.CurrentTimeMillis());
			notificationBuilder.SetDefaults(NotificationDefaults.All);
			//notificationBuilder.SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon));
			if (Build.VERSION.SdkInt.GetHashCode() >= 21)
				notificationBuilder.SetVibrate(new long[0]);

			var notificationManager = NotificationManager.FromContext(this);
			notificationManager.Notify(++notificationId, notificationBuilder.Build());
		}
	}
}
