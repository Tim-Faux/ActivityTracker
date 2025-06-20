using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ActivityTracker.Helpers;
using ActivityTracker.Messages;
using ActivityTracker.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
				
				var staffTextBox = CreateDragableTextbox(staffName.Trim(), TextBoxType.Staff);
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

				var clientTextBox = CreateDragableTextbox(clientName.Trim(), TextBoxType.Client);
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
			foreach (var child in ClientListGrid.Children) {
				var clientTextBlockGrid = child as Grid;
				if (clientTextBlockGrid != null && clientTextBlockGrid.Children.Count > 0) {
					var clientTextBlock = clientTextBlockGrid.Children[0] as TextBlock;
					if (clientTextBlock != null) {
						var clientText = clientTextBlock.Text;
						var clientName = clientText.Any(char.IsDigit) && clientText.Contains("X") ?
							clientText.Substring(0, clientText.LastIndexOf("X")).Trim() :
							clientText.Trim();

						if (clientsInActivity.ContainsKey(clientName)) {
							if (clientsInActivity[clientName] > 3) {
								clientTextBlock.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 237, 67, 55));
							}
							else if (clientsInActivity[clientName] > 1) {
								clientTextBlock.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 193, 7));
							}
							else {
								clientTextBlock.Foreground = new SolidColorBrush(Colors.Black);
							}
							clientTextBlock.Text = clientsInActivity[clientName] > 1 ?
								$"{clientName} X{clientsInActivity[clientName]}" :
								clientName;
						}
					}
				}
			}
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
					var clientName = textBlock.Text;
					clientName = clientName.Any(char.IsDigit) && clientName.Contains("X") ? clientName.Substring(0, clientName.LastIndexOf("X")).Trim() : clientName.Trim();
					draggedClient = clientName;
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
					if (textbox != null) {
						textbox.Text += textbox.Text.EndsWith("\r") ? string.Empty : Environment.NewLine;
						textbox.Text += draggedClient + Environment.NewLine;
					}
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
