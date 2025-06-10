using System.Threading.Tasks;
using ActivityTracker.Helpers;
using Microsoft.UI.Xaml;
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
			bool isValid = SyncfusionLicenseProvider.ValidateLicense(Platform.WinUI);
			if (!isValid) {
				SyncfusionLicenseError.Visibility = Visibility.Visible;
			}
		}
	}
}
