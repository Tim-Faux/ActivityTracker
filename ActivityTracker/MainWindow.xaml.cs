using Microsoft.UI.Xaml;

namespace ActivityTracker
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

			ExtendsContentIntoTitleBar = true;
			SetTitleBar(AppTitleBar);
		}
	}
}
