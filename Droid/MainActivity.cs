using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Firebase.Iid;
using Android.Util;

namespace XamarinFCMSample.Droid
{
	[Activity(Label = "XamarinFCMSample.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			LoadApplication(new App());

			try
			{
				var userId = Intent.GetStringExtra("NotificationUserID");
				var userName = Intent.GetStringExtra("NotificationUserName");
				var message = Intent.GetStringExtra("NotificationMessage");

				if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(message))
				{
					var currentApp = (Xamarin.Forms.Application.Current as App);
					if (currentApp != null)
					{
						var notification = new NotificationInfo();
						notification.SenderUserCode = userId;
						notification.SenderUserName = userName;
						notification.Message = message;

						currentApp.NotificationReceived(notification);
					}
				}
			}
			catch (Exception) { }

			RegisterForNotificationFCM();
		}

		private void RegisterForNotificationFCM()
		{
			if (FirebaseInstanceId.Instance != null)
			{
				var token = FirebaseInstanceId.Instance.Token;
				if (!string.IsNullOrEmpty(token))
				{
					// send fcm token to forms or server directly...
					Log.Debug("MainActivity", "FCM token :{0}", token);
				}
			}
		}

		public static bool InBackground = false;

		protected override void OnPause()
		{
			base.OnPause();
			InBackground = true;
		}

		protected override void OnResume()
		{
			base.OnResume();
			InBackground = false;
		}

		protected override void OnStop()
		{
			base.OnStop();
			InBackground = true;
		}
	}
}
