using ActivityTracker.Models;
using Microsoft.UI.Xaml.Controls;

namespace ActivityTracker.Views
{
    public sealed partial class AllDaysPage : Page
    {
        private SingleDay Monday = new();
		private SingleDay Tuesday = new();
		private SingleDay Wednesday = new();
		private SingleDay Thursday = new();
		private SingleDay Friday = new();

		public AllDaysPage()
        {
            this.InitializeComponent();
        }
    }
}
