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
using Syncfusion.UI.Xaml.Calendar;
using Syncfusion.UI.Xaml.Editors;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ActivityTracker.Views
{
	public sealed partial class AllDaysPage : Page
	{
		private const int SuccessBarDisplayTimeSeconds = 3;
		private const int WarningBarDisplayTimeSeconds = 3;
		private const int ErrorBarDisplayTimeSeconds = 3;

		private SingleDay Monday = new();
		private SingleDay Tuesday = new();
		private SingleDay Wednesday = new();
		private SingleDay Thursday = new();
		private SingleDay Friday = new();
		private StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
		private DispatcherTimer timer;

		public AllDaysPage()
        {
			WeakReferenceMessenger.Default.Register<ActiveClientsListUpdated>(this, (r, m) =>
			{
				UpdateClientsInActivityList(m.Value.ActiveClientsCount.GetClientDictionary());
			});
			this.InitializeComponent();
			ImportStaff();
			ImportClients();
			timer = new DispatcherTimer();
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
			else if (textBoxType == TextBoxType.Staff) {
				grid.DragStarting += Staff_DragStart;
				grid.DropCompleted += Staff_DropCompleted;
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
						else {
							clientTextBlock.Foreground = new SolidColorBrush(Colors.Black);
							clientTextBlock.Text = clientName;
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

			AllClients.ClearAllClientsCount();
			var clientTextBlockGrid = ClientListGrid.Children[0] as Grid;
			if (clientTextBlockGrid != null ) 
				clientTextBlockGrid.Children.Clear();
			ImportClients();
		}

		public void SaveToDocument(object sender, RoutedEventArgs e)
		{
			string titleDate = Monday.Date.HasValue ? Monday.Date.Value.ToString("M.d") : DateTime.Now.ToString("M.d");
			string fileName = $"week schedule {titleDate}.docx";
			Assembly assembly = typeof(App).GetTypeInfo().Assembly;
			SingleDay[] daysInWeek = { Monday, Tuesday, Wednesday, Thursday, Friday };
			WordDocument document = WordDocumentFormater.FormatWeekScheduleDoc(daysInWeek);

			if (!Monday.Date.HasValue || !Tuesday.Date.HasValue || !Wednesday.Date.HasValue || !Thursday.Date.HasValue || !Friday.Date.HasValue) {
				DisplayWarning(new List<string> { "The document has been saved but there were dates missing. Please review and save again" });
			}
			else {
				DisplaySuccess(new List<string> { "The document has been successful saved" });
			}

			document.Save(storageFolder.Path + "\\" + fileName, FormatType.Docx);
			document.Close();
		}

		public async void ImportDataFromFile(object sender, RoutedEventArgs e)
		{
			var importedDays = await WordDocumentImporter.ImportWordDocument();
			if (importedDays != null) {
				var importMismatch = false;
				if(importedDays.ContainsKey(DaysOfTheWeek.Monday))
					Monday.ImportData(importedDays[DaysOfTheWeek.Monday]);
				else
					importMismatch = true; 

				if (importedDays.ContainsKey(DaysOfTheWeek.Tuesday))
					Tuesday.ImportData(importedDays[DaysOfTheWeek.Tuesday]);
				else
					importMismatch = true; 

				if (importedDays.ContainsKey(DaysOfTheWeek.Wednesday))
					Wednesday.ImportData(importedDays[DaysOfTheWeek.Wednesday]);
				else
					importMismatch = true; 

				if (importedDays.ContainsKey(DaysOfTheWeek.Thursday))
					Thursday.ImportData(importedDays[DaysOfTheWeek.Thursday]);
				else
					importMismatch = true;

				if (importedDays.ContainsKey(DaysOfTheWeek.Friday))
					Friday.ImportData(importedDays[DaysOfTheWeek.Friday]);
				else
					importMismatch = true;

				if(importMismatch)
					DisplayWarning(new List<string> { "File successfully imported but did not match expected formatting. Please review the data to ensure nothing was lost" });
				else
					DisplaySuccess(new List<string> { "File successfully imported" });
			}
			else {
				DisplayError(new List<string> { "File could not be imported. Please ensure the file's formatting is correct" });
			}
		}

		private void SingleDay_DragOver(object sender, DragEventArgs e)
		{
			e.AcceptedOperation = DataPackageOperation.Copy;
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
			dayOfTheWeekDraggedOver = null;
		}

		private static string draggedStaff = "";
		public void Staff_DragStart(object sender, DragStartingEventArgs e)
		{
			var grid = sender as Grid;
			if (grid != null && grid.Children.Count > 0) {
				var textBlock = grid.Children[0] as TextBlock;
				if (textBlock != null) {
					var staffName = textBlock.Text.Trim();
					draggedStaff = textBlock.Text.Trim();
				}
			}
		}

		public void Staff_DropCompleted(object sender, DropCompletedEventArgs e)
		{
			draggedStaff = "";
			dayOfTheWeekDraggedOver = null;
		}

		private static string? dayOfTheWeekDraggedOver = null;
		public void ItemViews_Drag(object sender, DragEventArgs e)
		{
			var itemView = sender as ItemsView;
			if (itemView != null) {
				dayOfTheWeekDraggedOver = itemView.Name;
			}
		}

		public void SingleDay_Drop(object sender, DragEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(draggedClient)) {
				var grid = sender as Grid;
				if (grid != null && grid.Children.Count > 1) {
					var textbox = grid.Children[1] as TextBox;
					if (textbox != null) {
						verifyClientCanBeDropped(textbox);
					}
				}
			}
			draggedClient = "";

			if (!string.IsNullOrWhiteSpace(draggedStaff)) {
				var grid = sender as Grid;
				if (grid != null && grid.Children.Count > 0) {
					var staffGrid = grid.Children[0] as Grid;
					if (staffGrid != null && staffGrid.Children.Count > 1) {
						var textbox = staffGrid.Children[1] as TextBox;
						if (textbox != null) {
							verifyStaffCanBeDropped(textbox);
						}
					}
				}
			}
			draggedStaff = "";
			dayOfTheWeekDraggedOver = null;
		}

		private void verifyClientCanBeDropped(TextBox textbox)
		{
			ClientVerifier clientVerifier = new ClientVerifier();
			var allClientsActiveInDay = GetClientsInDroppedClientColumn();
			var clientCanBePlaced = clientVerifier.VerifyClient(draggedClient, allClientsActiveInDay);
			if (clientCanBePlaced.Count == 0) {
				textbox.Text += string.IsNullOrWhiteSpace(textbox.Text) || textbox.Text.EndsWith("\r") ? string.Empty : Environment.NewLine;
				textbox.Text += draggedClient + Environment.NewLine;
			}
			else {
				DisplayError(clientCanBePlaced);
			}
		}

		private List<string> GetClientsInDroppedClientColumn()
		{
			var clients = new List<string>();
			switch (dayOfTheWeekDraggedOver) {
				case "monday":
					clients = Monday.GetAllClientsInDay();
					break;
				case "tuesday":
					clients = Tuesday.GetAllClientsInDay();
					break;
				case "wednesday":
					clients = Wednesday.GetAllClientsInDay();
					break;
				case "thursday":
					clients = Thursday.GetAllClientsInDay();
					break;
				case "friday":
					clients = Friday.GetAllClientsInDay();
					break;
			}
			return clients;
		}

		private void verifyStaffCanBeDropped(TextBox textbox)
		{
			var staffVerifier = new StaffVerifier();
			var allStaffActiveInDay = GetStaffInDroppedColumn();
			var staffCanBePlacedErrors = staffVerifier.VerifyStaff(draggedStaff, allStaffActiveInDay);
			if (staffCanBePlacedErrors.Count == 0) {
				textbox.Text = draggedStaff + Environment.NewLine;
			}
			else {
				DisplayError(staffCanBePlacedErrors);
			}
		}

		private List<string> GetStaffInDroppedColumn()
		{
			var staff = new List<string>();
			switch (dayOfTheWeekDraggedOver) {
				case "monday":
					staff = Monday.GetAllStaffInDay();
					break;
				case "tuesday":
					staff = Tuesday.GetAllStaffInDay();
					break;
				case "wednesday":
					staff = Wednesday.GetAllStaffInDay();
					break;
				case "thursday":
					staff = Thursday.GetAllStaffInDay();
					break;
				case "friday":
					staff = Friday.GetAllStaffInDay();
					break;
			}
			return staff;
		}

		private void DisplaySuccess(List<string> SuccessMessages)
		{
			ValidationError.Visibility = Visibility.Visible;
			ValidationError.Severity = InfoBarSeverity.Success;
			ValidationError.Title = "Success";
			ValidationError.Message = string.Join(Environment.NewLine, SuccessMessages);

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(SuccessBarDisplayTimeSeconds);
			timer.Tick += Timer_HideError;
			timer.Start();
		}

		private void DisplayWarning(List<string> WarningMessages)
		{
			ValidationError.Visibility = Visibility.Visible;
			ValidationError.Severity = InfoBarSeverity.Warning;
			ValidationError.Title = "Warning";
			ValidationError.Message = string.Join(Environment.NewLine, WarningMessages);

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(WarningBarDisplayTimeSeconds);
			timer.Tick += Timer_HideError;
			timer.Start();
		}

		private void DisplayError(List<string> ErrorMessages)
		{
			ValidationError.Visibility = Visibility.Visible;
			ValidationError.Severity = InfoBarSeverity.Error;
			ValidationError.Title = "Error";
			ValidationError.Message = string.Join(Environment.NewLine, ErrorMessages);

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(ErrorBarDisplayTimeSeconds);
			timer.Tick += Timer_HideError;
			timer.Start();
		}

		private void Timer_HideError(object? sender, object e)
		{
			ValidationError.Visibility = Visibility.Collapsed;
			timer.Stop();
		}

		#region DisableCalendarDays Handlers
		private void DisableAllCalendarDaysExceptMonday(object sender, CalendarItemPreparedEventArgs e)
		{
			DisableAllCalendarDaysExceptSpecificDay(sender, e, DayOfWeek.Monday);
		}

		private void DisableAllCalendarDaysExceptTuesday(object sender, CalendarItemPreparedEventArgs e)
		{
			DisableAllCalendarDaysExceptSpecificDay(sender, e, DayOfWeek.Tuesday);
		}

		private void DisableAllCalendarDaysExceptWednesday(object sender, CalendarItemPreparedEventArgs e)
		{
			DisableAllCalendarDaysExceptSpecificDay(sender, e, DayOfWeek.Wednesday);
		}

		private void DisableAllCalendarDaysExceptThursday(object sender, CalendarItemPreparedEventArgs e)
		{
			DisableAllCalendarDaysExceptSpecificDay(sender, e, DayOfWeek.Thursday);
		}

		private void DisableAllCalendarDaysExceptFriday(object sender, CalendarItemPreparedEventArgs e)
		{
			DisableAllCalendarDaysExceptSpecificDay(sender, e, DayOfWeek.Friday);
		}

		private void DisableAllCalendarDaysExceptSpecificDay(object sender, CalendarItemPreparedEventArgs e, DayOfWeek acceptedDay)
		{
			var calender = sender as SfCalendar;
			if (calender != null && e.ItemInfo.ItemType == CalendarItemType.Day && e.ItemInfo.Date.DayOfWeek != acceptedDay) {
				e.ItemInfo.IsBlackout = true;
			}
		}
		#endregion

		#region DateChanging handlers
		private void SelectedMondayDateChanging(object sender, DateChangingEventArgs e)
		{
			VerifyDayOfWeek(e, DayOfWeek.Monday);
		}

		private void SelectedTuesdayDateChanging(object sender, DateChangingEventArgs e)
		{
			VerifyDayOfWeek(e, DayOfWeek.Tuesday);
		}

		private void SelectedWednesdayDateChanging(object sender, DateChangingEventArgs e)
		{
			VerifyDayOfWeek(e, DayOfWeek.Wednesday);
		}

		private void SelectedThursdayDateChanging(object sender, DateChangingEventArgs e)
		{
			VerifyDayOfWeek(e, DayOfWeek.Thursday);
		}

		private void SelectedFridayDateChanging(object sender, DateChangingEventArgs e)
		{
			VerifyDayOfWeek(e, DayOfWeek.Friday);
		}

		private void VerifyDayOfWeek(DateChangingEventArgs e, DayOfWeek dayOfWeek)
		{
			if (e.NewDate.HasValue) {
				var date = e.NewDate.Value.Date;
				var errors = WeeklyDateVerifier.VerifyDate(date, dayOfWeek);
				if (errors != null && errors.Count > 0) {
					DisplayError(errors);
					e.Cancel = true;
				}
				else {
					switch (dayOfWeek) {
						case DayOfWeek.Monday:
							SetNewDates(date);
							break;
						case DayOfWeek.Tuesday:
							SetNewDates(date.AddDays(-1));
							break;
						case DayOfWeek.Wednesday:
							SetNewDates(date.AddDays(-2));
							break;
						case DayOfWeek.Thursday:
							SetNewDates(date.AddDays(-3));
							break;
						case DayOfWeek.Friday:
							SetNewDates(date.AddDays(-4));
							break;
					}
				}
			}
		}

		private void SetNewDates(DateTime mondayDate)
		{
			mondayCalendar.SelectedDate = mondayDate;
			tuesdayCalendar.SelectedDate = mondayDate.AddDays(1);
			wednesdayCalendar.SelectedDate = mondayDate.AddDays(2);
			thursdayCalendar.SelectedDate = mondayDate.AddDays(3);
			fridayCalendar.SelectedDate = mondayDate.AddDays(4);
		}
		#endregion
	}
}
