using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using UIKit;
using UserNotifications;

namespace XamarinFCMSample.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
	{

		public event EventHandler<UserInfoEventArgs> NotificationReceived;

		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();

			LoadApplication(new App());


			RegisterForNotificationFCM();

			return base.FinishedLaunching(app, options);
		}

		private void RegisterForNotificationFCM()
		{
			try
			{
				InstanceId.Notifications.ObserveTokenRefresh(TokenRefreshNotification);

				// Register your app for remote notifications.
				if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
				{
					// iOS 10 or later
					var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
					UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
					{
						Console.WriteLine(granted);
					});

					// For iOS 10 display notification (sent via APNS)
					UNUserNotificationCenter.Current.Delegate = this;

					// For iOS 10 data message (sent via FCM)
					Messaging.SharedInstance.RemoteMessageDelegate = this;
				}
				else
				{
					// iOS 9 or before
					var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
					var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
					UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
				}

				UIApplication.SharedApplication.RegisterForRemoteNotifications();

				if (Firebase.Analytics.App.DefaultInstance == null)
					Firebase.Analytics.App.Configure();
			}
			catch (Exception) { }
		}

		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
			Messaging.SharedInstance.Disconnect();
			Console.WriteLine("Disconnected from FCM");
		}

		private void HandleNotification(NSDictionary userInfo)
		{
			try
			{
				var currentApp = (Xamarin.Forms.Application.Current as App);
				if (currentApp != null)
				{
					var notification = new NotificationInfo();

					var apsDictionary = userInfo["aps"] as NSDictionary;

					string body;
					if (apsDictionary["alert"] is NSDictionary)
					{
						var alertDictionary = apsDictionary["alert"] as NSDictionary;

						if (alertDictionary.ContainsKey(new NSString("title")))
							notification.Title = alertDictionary["title"].ToString();

						body = alertDictionary["body"].ToString();
					}
					else
					{
						body = apsDictionary["alert"].ToString();
					}

					notification.Message = body;

					var userKey = userInfo["USERKEY"];
					if (userKey != null)
						notification.SenderUserCode = userKey.ToString();

					var userNameKey = userInfo["TITLEKEY"];
					if (userNameKey != null)
						notification.SenderUserName = userNameKey.ToString();

					currentApp.NotificationReceived(notification);
				}
			}
			catch (Exception) { }
		}

		// To receive notifications in foregroung on iOS 9 and below.
		// To receive notifications in background in any iOS version
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			HandleNotification(userInfo);
			return;
			//new UIAlertView("DidReceiveRemoteNotification", userInfo.ToString(), null, "OK", null).Show();
			// If you are receiving a notification message while your app is in the background,
			// this callback will not be fired 'till the user taps on the notification launching the application.

			// If you disable method swizzling, you'll need to call this method. 
			// This lets FCM track message delivery and analytics, which is performed
			// automatically with method swizzling enabled.
			//Messaging.GetInstance ().AppDidReceiveMessage (userInfo);

			if (NotificationReceived == null)
				return;

			var e = new UserInfoEventArgs { UserInfo = userInfo };
			NotificationReceived(this, e);
		}

		// You'll need this method if you set "FirebaseAppDelegateProxyEnabled": NO in GoogleService-Info.plist
		//public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		//{
		//	InstanceId.SharedInstance.SetApnsToken(deviceToken, ApnsTokenType.Prod);
		//}

		// To receive notifications in foreground on iOS 10 devices.
		[Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
		public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
		{
			//new UIAlertView("WillPresentNotification", notification.Request.Content.Body, null, "OK", null).Show();

			HandleNotification(notification.Request.Content.UserInfo);
			return;

			if (NotificationReceived == null)
			{

				completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound);
				return;
			}

			var e = new UserInfoEventArgs { UserInfo = notification.Request.Content.UserInfo };
			NotificationReceived(this, e);

		}

		// Receive data message on iOS 10 devices.
		public void ApplicationReceivedRemoteMessage(RemoteMessage remoteMessage)
		{
			Console.WriteLine(remoteMessage.AppData);

		}

		//////////////////
		////////////////// WORKAROUND
		//////////////////

		#region Workaround for handling notifications in background for iOS 10

		[Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
		public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
		{
			try
			{
				Console.WriteLine(response.Notification.Request.Content.UserInfo);
				HandleNotification(response.Notification.Request.Content.UserInfo);
				return;
				if (NotificationReceived == null)
				{
					completionHandler();
					return;
				}

				var e = new UserInfoEventArgs { UserInfo = response.Notification.Request.Content.UserInfo };
				NotificationReceived(this, e);
			}
			catch (Exception) { }
		}

		#endregion

		//////////////////
		////////////////// END OF WORKAROUND
		//////////////////
		/// 
		void TokenRefreshNotification(object sender, NSNotificationEventArgs e)
		{
			// This method will be fired everytime a new token is generated, including the first
			// time. So if you need to retrieve the token as soon as it is available this is where that
			// should be done.
			var token = InstanceId.SharedInstance.Token;

			if (!string.IsNullOrEmpty(token))
			{
				ConnectToFCM();

				// send fcm token to forms or server directly...
				Console.WriteLine("FCM token :{0}", token);
			}
		}

		public void ConnectToFCM()
		{
			Messaging.SharedInstance.Connect(error =>
			{
				if (error != null)
				{
					Console.WriteLine("Unable to connect to FCM" + error);
				}
				else
				{
					Console.WriteLine("Success - Connected to FCM");
					Console.WriteLine($"FCM Token: {InstanceId.SharedInstance.Token}");
					Console.WriteLine($"APN Token: {apnToken}");
				}
			});
		}

		private string apnToken = null;
	}

	public class UserInfoEventArgs : EventArgs
	{
		public NSDictionary UserInfo { get; set; }
	}
}
