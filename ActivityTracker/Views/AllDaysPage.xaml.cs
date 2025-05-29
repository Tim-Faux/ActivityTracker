using System.Collections.Generic;
using ActivityTracker.Messages;
using ActivityTracker.Models;
using CommunityToolkit.Mvvm.Messaging;
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
        private string AllClients = "";

		public AllDaysPage()
        {
			WeakReferenceMessenger.Default.Register<ActiveClientsListUpdated>(this, (r, m) =>
			{
				UpdateClientsInActivityList(m.Value.ActiveClients);
			});
			this.InitializeComponent();
		}

		public void UpdateClientsInActivityList(List<string> clientsInActivity)
		{
			AllClients = string.Empty;
			foreach (var client in clientsInActivity) {
				AllClients += client + "\r";
			}
			ClientsInActivities.Text = AllClients;
			
		}
	}
}
