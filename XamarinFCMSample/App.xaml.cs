using System;
using Xamarin.Forms;

namespace XamarinFCMSample
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = new XamarinFCMSamplePage();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

		public void NotificationReceived(NotificationInfo notification)
		{
			try
			{
				if (IsNavigatable(MainPage))
				{
					MainPage.DisplayAlert(notification.SenderUserName, notification.Message, "OK");
					/*  Navigate to notification detail page with notification info parameter... */
					/*
					Device.BeginInvokeOnMainThread(async () =>
						{
							await MainPage.Navigation.PushModalAsync(
								new NotificationDetailPage(notification), true);
						});
						*/
				}
			}
			catch (Exception)
			{
				// exception logging
			}
		}

		bool IsNavigatable(Page page)
		{
			return true;
		}
	}
}
