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

namespace ActivityTracker.Helpers
{
	public static class WordDocumentImporter
	{
		private static DaysOfTheWeek[] daysOfTheWeek = [DaysOfTheWeek.Monday, DaysOfTheWeek.Tuesday, DaysOfTheWeek.Wednesday, DaysOfTheWeek.Thursday, DaysOfTheWeek.Friday];
		public static async Task<Dictionary<DaysOfTheWeek, SingleDay>?> ImportWordDocument()
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
				var parsedDocumentData = await ParseWordDoc(file);
				return parsedDocumentData;
			}
			else {
				return null;
			}
		}

		private static async Task<Dictionary<DaysOfTheWeek, SingleDay>?> ParseWordDoc(StorageFile file)
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
			var lineOneSplit = paragraphs[0].ChildEntities;
			var activity = string.Empty;
			var staff = string.Empty;
			if (lineOneSplit.Count == 1) {
				var splitTextRangeOne = lineOneSplit[0] as WTextRange;

				if (splitTextRangeOne != null) {
					if (splitTextRangeOne.Text.EndsWith(" "))
						activity = splitTextRangeOne.Text.Trim();
					else
						staff = splitTextRangeOne.Text.Trim();
				}
				paragraphs.RemoveAt(0);
			}
			else if (lineOneSplit.Count == 2) {
				var splitTextRangeOne = lineOneSplit[0] as WTextRange;
				activity = splitTextRangeOne != null ? splitTextRangeOne.Text.Trim() : string.Empty;
				var splitTextRangeTwo = lineOneSplit[1] as WTextRange;
				staff = splitTextRangeTwo != null ? splitTextRangeTwo.Text.Trim() : string.Empty;

				paragraphs.RemoveAt(0);
			}

			return new ActivityAndStaff { Activity = activity, Staff = staff };
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
	}
}
