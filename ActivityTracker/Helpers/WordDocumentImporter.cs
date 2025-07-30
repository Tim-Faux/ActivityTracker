using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.IO;
using System.Collections.Generic;
using ActivityTracker.Models;
using Syncfusion.Drawing;

namespace ActivityTracker.Helpers
{
	public static class WordDocumentImporter
	{
		private static DaysOfTheWeek[] daysOfTheWeek = [DaysOfTheWeek.Monday, DaysOfTheWeek.Tuesday, DaysOfTheWeek.Wednesday, DaysOfTheWeek.Thursday, DaysOfTheWeek.Friday];
		public static async Task<ImportedFileData?> ImportWordDocument()
		{
			FileOpenPicker fileOpenPicker = new()
			{
				ViewMode = PickerViewMode.Thumbnail,
				FileTypeFilter = { ".docx" },
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};

			nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
			InitializeWithWindow.Initialize(fileOpenPicker, windowHandle);

			StorageFile file = await fileOpenPicker.PickSingleFileAsync();

			if (file != null && file.FileType == ".docx") {
				var importedFileMondayDate = ParseDateFromTitle(file);
				var weekScheduleTableData = await ParseWordDoc(file, importedFileMondayDate);

				IWParagraphCollection? textAfterWeekSchedule = null;
				if (weekScheduleTableData != null) {
					textAfterWeekSchedule = await ParseTextAfterWeekSchedule(file);
				}

				var parsedDocumentData = new ImportedFileData {
					WeekScheduleTableData = weekScheduleTableData,
					MondayDate = importedFileMondayDate,
					TextAfterWeekSchedule = textAfterWeekSchedule
				};
				return parsedDocumentData;
			}
			else {
				return null;
			}
		}

		private static async Task<Dictionary<DaysOfTheWeek, SingleDay>?> ParseWordDoc(StorageFile file, DateTime? mondayDate)
		{
			var allDays = new Dictionary<DaysOfTheWeek, SingleDay>();
			using (var wordDocStream = await file.OpenStreamForReadAsync()) {
				WordDocument document = new WordDocument(wordDocStream, FormatType.Automatic);
				if (document.Sections.Count < 1)
					return null;
				var tableSection = document.Sections[0];

				if(tableSection.Tables.Count < 1)
					return null;
				var table = tableSection.Tables[0];

				if(table.Rows.Count < 2)
					return null;

				for (int row = 1; row < table.Rows.Count; row++) {
					WTableRow currentRow = table.Rows[row];
					for (int column = 0; column < currentRow.Cells.Count && daysOfTheWeek.Length > column; column++) {
						var singleDay = allDays.ContainsKey(daysOfTheWeek[column]) ? allDays[daysOfTheWeek[column]] : new SingleDay();
						var paragraphs = currentRow.Cells[column].Paragraphs;

						if (paragraphs.Count > 0) {
							var activityAndStaff = GetActivityAndStaff(paragraphs);
							var clients = GetClients(paragraphs);

							singleDay.AllStaffPerDay[row - 1].StaffName = activityAndStaff.Staff;
							singleDay.AllStaffPerDay[row - 1].Activity = activityAndStaff.Activity;
							singleDay.AllStaffPerDay[row - 1].ClientNames = clients;
							if(mondayDate.HasValue)
								singleDay.Date = mondayDate.Value.AddDays(column);

							allDays.Remove(daysOfTheWeek[column]);
							allDays.Add(daysOfTheWeek[column], singleDay);
						}
					}
				}
			}
			return allDays;
		}

		private static ActivityAndStaff GetActivityAndStaff(IWParagraphCollection paragraphs)
		{
			var activityAndStaff = new ActivityAndStaff();
			var linesToRemove = new Stack<int>();
			for (var i = 0; i < paragraphs.Count; i++) {
				var paragraphEntities = paragraphs[i].ChildEntities;
				var activityOrStaffInLine = false;
				foreach (var paragraphEntity in paragraphEntities) {
					var textRange = paragraphEntity as WTextRange;
					if (textRange != null) {
						var activityAndStaffInLine = ParseActivityAndStaff(textRange);
						if (!string.IsNullOrWhiteSpace(activityAndStaffInLine.Activity) &&
							string.IsNullOrWhiteSpace(activityAndStaff.Activity)) {
							activityAndStaff.Activity = activityAndStaffInLine.Activity;
							activityOrStaffInLine = true;
						}
						if (!string.IsNullOrWhiteSpace(activityAndStaffInLine.Staff)) {
							if(!string.IsNullOrWhiteSpace(activityAndStaff.Staff) && !activityAndStaff.Staff.EndsWith('\r'))
								activityAndStaff.Staff += "\r";
							activityAndStaff.Staff += activityAndStaffInLine.Staff;
							activityOrStaffInLine = true;
						}
					}
				}
				if (activityOrStaffInLine) {
					linesToRemove.Push(i);
				}
			}

			while (linesToRemove.Count > 0) {
				paragraphs.RemoveAt(linesToRemove.Pop());
			}

			return activityAndStaff;
		}

		private static ActivityAndStaff ParseActivityAndStaff(WTextRange textRange)
		{
			var activity = string.Empty;
			var staff = string.Empty;
			var textColor = textRange.CharacterFormat.TextColor;
			if (IsRed(textColor)) {
				activity += textRange.Text.Trim();
			}
			else if (IsBlue(textColor)) {
				foreach (var staffName in textRange.Text.Split(',')) {
					if (!string.IsNullOrWhiteSpace(staff) && !staff.EndsWith('\r'))
						staff += ",\r";
					staff += staffName.Trim();
				}
			}
			return new ActivityAndStaff { Activity = activity, Staff = staff };
		}

		private static bool IsRed(Color color)
		{
			if ((color.IsNamedColor && color.Name == Color.Red.Name) || color.ToArgb() == Color.Red.ToArgb())
				return true;
			if (color.GetSaturation() > 0.1 && color.GetBrightness() > 0.1 && color.GetBrightness() < 0.95)
				if (color.GetHue() < 30 || color.GetHue() > 325)
					return true;
			return false;
		}

		private static bool IsBlue(Color color)
		{
			if ((color.IsNamedColor && color.Name == Color.Blue.Name) || color.ToArgb() == Color.Blue.ToArgb())
				return true;
			if (color.GetSaturation() > 0.1 && color.GetBrightness() > 0.1 && color.GetBrightness() < 0.95)
				if (color.GetHue() > 150 && color.GetHue() < 280) 
					return true;
			return false;
		}

		private static string GetClients(IWParagraphCollection paragraph)
		{
			var clientsInDay = string.Empty;
			foreach (IWParagraph client in paragraph) {
				if (!string.IsNullOrWhiteSpace(client.Text)) {
					clientsInDay += client.Text;
					if (!client.Text.EndsWith('\r'))
						clientsInDay += '\r';
				}
			}

			return clientsInDay;
		}

		private class ActivityAndStaff
		{
			public string Activity { get; set; } = "";
			public string Staff { get; set; } = "";
		}

		private static DateTime? ParseDateFromTitle(StorageFile file) 
		{
			var fileName = file.DisplayName.ToLower();
			var numCharBeforeDate = fileName.LastIndexOf("schedule") + "schedule".Length;
			var dateString = fileName.Substring(numCharBeforeDate);

			if (DateTime.TryParse(dateString, out var fileNameDate))
				return fileNameDate;
			else 
				return null;
		}

		private async static Task<IWParagraphCollection?> ParseTextAfterWeekSchedule(StorageFile file)
		{
			using (var wordDocStream = await file.OpenStreamForReadAsync()) {
				WordDocument document = new WordDocument(wordDocStream, FormatType.Automatic);
				if (document.Sections.Count < 1)
					return null;
				var paragraphsAfterWeekSchedule = document.Sections[0].Paragraphs;
				return paragraphsAfterWeekSchedule;
			}
		}
	}
}
