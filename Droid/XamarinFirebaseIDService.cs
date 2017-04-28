using System;
using Android.App;
using Android.Util;
using Firebase.Iid;

namespace XamarinFCMSample.Droid
{
	[Service]
	[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
	public class XamarinFirebaseIDService : FirebaseInstanceIdService
	{
		const string TAG = "XamarinFirebaseIDService";
		public override void OnTokenRefresh()
		{
			var refreshedToken = FirebaseInstanceId.Instance.Token;
			Log.Debug(TAG, "Refreshed token: " + refreshedToken);
			SendRegistrationToServer(refreshedToken);
		}

		private void SendRegistrationToServer(string token)
		{

		}
	}
}
