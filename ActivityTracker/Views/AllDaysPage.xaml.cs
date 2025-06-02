using System.Collections.Generic;
using ActivityTracker.Messages;
using ActivityTracker.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

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
			WeakReferenceMessenger.Default.Register<ActiveClientsListUpdated>(this, (r, m) =>
			{
				UpdateClientsInActivityList(m.Value.ActiveClientsCount.GetClientDictionary());
			});
			this.InitializeComponent();
		}

		public void UpdateClientsInActivityList(Dictionary<string,int> clientsInActivity)
		{
			var clients = clientsInActivity.Keys;
			Paragraph paragraph = new Paragraph();
			foreach (var client in clients) {
				Run run = new Run();
				if (clientsInActivity[client] > 3) {
					run.Text += $"{client} X{clientsInActivity[client]}\r";
					run.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 237, 67, 55));
				}
				else if (clientsInActivity[client] > 1) {
					run.Text += $"{client} X{clientsInActivity[client]}\r";
					run.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255,193,7));
				}
				else {
					run.Text += client + "\r";
				}
				paragraph.Inlines.Add(run);
			}

			ClientsInActivities.Blocks.Clear();
			ClientsInActivities.Blocks.Add(paragraph);
			
		}
	}
}
