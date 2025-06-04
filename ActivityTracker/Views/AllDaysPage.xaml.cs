using System.Collections.Generic;
using System.Reflection;
using ActivityTracker.Helpers;
using ActivityTracker.Messages;
using ActivityTracker.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Windows.Storage;

namespace ActivityTracker.Views
{
	public sealed partial class AllDaysPage : Page
    {
		private SingleDay Monday = new();
		private SingleDay Tuesday = new();
		private SingleDay Wednesday = new();
		private SingleDay Thursday = new();
		private SingleDay Friday = new();
		private StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

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

		public void ClearData(object sender, RoutedEventArgs e)
		{
			Monday.AllStaffPerDay.Clear();
			Monday.CreateAllStaffPerDayList();

			Tuesday.AllStaffPerDay.Clear();
			Tuesday.CreateAllStaffPerDayList();

			Wednesday.AllStaffPerDay.Clear();
			Wednesday.CreateAllStaffPerDayList();

			Thursday.AllStaffPerDay.Clear();
			Thursday.CreateAllStaffPerDayList();

			Friday.AllStaffPerDay.Clear();
			Friday.CreateAllStaffPerDayList();

			ClientsInActivities.Blocks.Clear();
			AllClients.ClearAllClientsCount();
		}

		public void SaveToDocument(object sender, RoutedEventArgs e)
		{
			string fileName = "note.docx";
			Assembly assembly = typeof(App).GetTypeInfo().Assembly;
			SingleDay[] daysInWeek = { Monday, Tuesday, Wednesday, Thursday, Friday };
			WordDocument document = WordDocumentFormater.FormatWeekScheduleDoc(daysInWeek);

			document.Save(storageFolder.Path + "\\" + fileName, FormatType.Docx);
			document.Close();

		}
	}
}
