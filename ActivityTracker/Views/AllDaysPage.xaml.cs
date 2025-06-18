using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ActivityTracker.Helpers;
using ActivityTracker.Messages;
using ActivityTracker.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Windows.ApplicationModel.DataTransfer;
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
			ImportStaff();
			ImportClients();
		}

		private void ImportStaff()
		{
			List<string> staffNames = new List<string>();
			Task.Run(async () => staffNames = await StaffNameImporter.ImportStaff()).Wait();

			int row = 0;
			foreach (var staffName in staffNames) {
				var rowDef = new RowDefinition();
				rowDef.Height = GridLength.Auto;
				StaffListGrid.RowDefinitions.Add(rowDef);

				var staffTextBox = CreateDragableTextbox(staffName, TextBoxType.Staff);
				Grid.SetRow(staffTextBox, row);
				StaffListGrid.Children.Add(staffTextBox);

				row++;
			}
		}
		private void ImportClients()
		{
			List<string> clientNames = new List<string>();
			Task.Run(async () => clientNames = await ClientNameImporter.ImportClients()).Wait();

			int row = 0;
			foreach (var clientName in clientNames) {
				var rowDef = new RowDefinition();
				rowDef.Height = GridLength.Auto;
				ClientListGrid.RowDefinitions.Add(rowDef);

				var clientTextBox = CreateDragableTextbox(clientName, TextBoxType.Client);
				Grid.SetRow(clientTextBox, row);
				ClientListGrid.Children.Add(clientTextBox);

				row++;
			}
		}

		public Grid CreateDragableTextbox(string text, TextBoxType textBoxType)
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.IsTabStop = false;
			textBlock.Margin = new Thickness(5);
			textBlock.TextWrapping = TextWrapping.Wrap;
			textBlock.IsTextSelectionEnabled = false;

			Grid grid = new Grid();
			grid.Children.Add(textBlock);
			var brush = new SolidColorBrush(Colors.LightGray);
			grid.BorderBrush = brush;
			grid.BorderThickness = new Thickness(1);
			var backgroundBrush = new SolidColorBrush(Colors.White);
			grid.Background = backgroundBrush;
			grid.CanDrag = true;
			if (textBoxType == TextBoxType.Client) {
				grid.DragStarting += Clients_DragStart;
				grid.DropCompleted += Clients_DropCompleted;
			}

			return grid;
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

			//TODO need to update this for the new dragable textboxes
			//ClientsInActivities.Blocks.Clear();
			//ClientsInActivities.Blocks.Add(paragraph);
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

			//ClientsInActivities.Blocks.Clear();
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

		private static string draggedClient = "";
		public void Clients_DragStart(object sender, DragStartingEventArgs e)
		{
			var grid = sender as Grid;
			if (grid != null && grid.Children.Count > 0) {
				var textBlock = grid.Children[0] as TextBlock;
				if (textBlock != null) {
					draggedClient = textBlock.Text;
				}
			}
		}

		public void Clients_DropCompleted(object sender, DropCompletedEventArgs e)
		{
			draggedClient = "";
		}

		public void SingleDay_Drop(object sender, DragEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(draggedClient)) {
				var grid = sender as Grid;
				if (grid != null && grid.Children.Count > 1) {
					var textbox = grid.Children[1] as TextBox;
					if(textbox != null)
						textbox.Text += draggedClient + "\r";
				}
			}
			draggedClient = "";
		}

		private void SingleDay_DragOver(object sender, DragEventArgs e)
		{
			e.AcceptedOperation = DataPackageOperation.Copy;
		}
	}
}
