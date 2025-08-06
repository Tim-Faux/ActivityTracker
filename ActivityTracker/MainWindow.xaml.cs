using System;
using System.Threading.Tasks;
using ActivityTracker.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Syncfusion.Licensing;

namespace ActivityTracker
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
		{
			this.InitializeComponent();

			ExtendsContentIntoTitleBar = true;
			SetTitleBar(AppTitleBar);

			Task.Run(() => SyncfusionLicenseRegister.RegisterSyncfusionLicense()).Wait();
			var isValid = SyncfusionLicenseProvider.ValidateLicense(Platform.WinUI);
			if (!isValid) {
				SyncfusionLicenseError.Visibility = Visibility.Visible;
			}
		}

		private async Task ValidateSyncfusionLicense()
		{
			var isValid = false;
			var numberOfAttempts = 0;
			do {
				isValid = SyncfusionLicenseProvider.ValidateLicense(Platform.WinUI);
				if (!isValid) {
					SyncfusionLicenseError.Visibility = Visibility.Visible;
					await Task.Delay(500);
				}
				else {
					SyncfusionLicenseError.Visibility = Visibility.Collapsed;
				}
			} while (!isValid && numberOfAttempts < 10);
		}

		private async void SyncfusionLicenseLink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
		{
			var text = new TextBlock
			{
				Text = "Enter your Syncfusion license and hit enter:",
				Margin = new Thickness(10)
			};
			var syncfusionLicenceTextbox = new TextBox
			{
				PlaceholderText = "Enter your license",
				Width = 1000,
				Margin = new Thickness(10),
				AcceptsReturn = false
			};

			var content = new StackPanel();
			content.Children.Insert(0, text);
			content.Children.Insert(1, syncfusionLicenceTextbox);

			ContentDialog LicenseErrorDialog = new ContentDialog
			{
				XamlRoot = this.Content.XamlRoot,
				Title = "Syncfusion License Error",
				Content = content,
				PrimaryButtonText = "Enter",
				CloseButtonText = "Cancel",
				DefaultButton = ContentDialogButton.Primary
			};

			ContentDialogResult result = await LicenseErrorDialog.ShowAsync();
			if (result == ContentDialogResult.Primary) {
				await SyncfusionLicenseRegister.SaveSyncfusionLicense(syncfusionLicenceTextbox.Text.Trim());
			}

			await ValidateSyncfusionLicense();
		}
	}
}
