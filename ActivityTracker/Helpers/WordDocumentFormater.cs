using ActivityTracker.Models;
using Syncfusion.DocIO.DLS;
using Syncfusion.Drawing;

namespace ActivityTracker.Helpers
{
    public static class WordDocumentFormater
    {
		private const int columnWidth = 100;
		private const int headerCellHeight = 30;
		private const int contentCellHeight = 100;

		public static WordDocument FormatWeekScheduleDoc(SingleDay[] daysInWeek)
		{
			WordDocument document = new WordDocument();

			IWSection section = document.AddSection();
			section.PageSetup.Orientation = PageOrientation.Landscape;
			IWParagraph paragraph = section.AddParagraph();
			paragraph.ParagraphFormat.FirstLineIndent = 36;
			paragraph.BreakCharacterFormat.FontSize = 12f;

			CreateTable(section, daysInWeek);
			IWSection activitySuggestionSection = document.AddSection();
			AddActivitySuggestionDaysOfTheWeek(activitySuggestionSection);

			return document;
		}

		private static void CreateTable(IWSection section, SingleDay[] daysInWeek)
		{
			IWTable table = section.AddTable();
			table.ResetCells(SingleDay.numberOfStaff + 1, 5);

			table = SetTableHeader(table);
			table = SetTableContent(table, daysInWeek);
		}

		private static IWTable SetTableHeader(IWTable table)
		{
			table.Rows[0].Height = headerCellHeight;

			var mondayCell = table.Rows[0].Cells[0];
			var paragraph = mondayCell.AddParagraph();
			IWTextRange textRange = paragraph.AppendText("Monday");
			mondayCell.Width = columnWidth;
			textRange.CharacterFormat.Bold = true;
			textRange.CharacterFormat.FontName = "Aptos";

			var tuesdayCell = table.Rows[0].Cells[1];
			paragraph = tuesdayCell.AddParagraph();
			textRange = paragraph.AppendText("Tuesday");
			tuesdayCell.Width = columnWidth;
			textRange.CharacterFormat.Bold = true;
			textRange.CharacterFormat.FontName = "Aptos";

			var wednesdayCell = table.Rows[0].Cells[2];
			paragraph = wednesdayCell.AddParagraph();
			textRange = paragraph.AppendText("Wednesday");
			wednesdayCell.Width = columnWidth;
			textRange.CharacterFormat.Bold = true;
			textRange.CharacterFormat.FontName = "Aptos";

			var thursdayCell = table.Rows[0].Cells[3];
			paragraph = thursdayCell.AddParagraph();
			textRange = paragraph.AppendText("Thurday");
			thursdayCell.Width = columnWidth;
			textRange.CharacterFormat.Bold = true;
			textRange.CharacterFormat.FontName = "Aptos";

			var fridayCell = table.Rows[0].Cells[4];
			paragraph = fridayCell.AddParagraph();
			textRange = paragraph.AppendText("Friday");
			fridayCell.Width = columnWidth;
			textRange.CharacterFormat.Bold = true;
			textRange.CharacterFormat.FontName = "Aptos";

			return table;
		}

		private static IWTable SetTableContent(IWTable table, SingleDay[] daysInWeek)
		{
			var currentColumn = 0;
			foreach (var day in daysInWeek) {
				var currentRow = 1;
				foreach (var singleDayInformation in day.AllStaffPerDay) {
					var currentCell = table.Rows[currentRow].Cells[currentColumn];
					var paragraph = currentCell.AddParagraph();

					IWTextRange textRange = paragraph.AppendText($"{singleDayInformation.Activity} ");
					textRange.CharacterFormat.TextColor = Color.Red;
					textRange.CharacterFormat.FontName = "Aptos";

					textRange = paragraph.AppendText($"{singleDayInformation.StaffNames}\r");
					textRange.CharacterFormat.TextColor = Color.Blue;
					textRange.CharacterFormat.FontName = "Aptos";

					textRange = paragraph.AppendText(singleDayInformation.ClientNames);
					textRange.CharacterFormat.TextColor = Color.Black;
					textRange.CharacterFormat.FontName = "Aptos";
					currentCell.Width = columnWidth;

					table.Rows[currentRow].Height = contentCellHeight;
					currentRow++;
				}
				currentColumn++;
			}

			return table;
		}

		private static void AddActivitySuggestionDaysOfTheWeek(IWSection section)
		{
			var paragraph = section.AddParagraph();
			paragraph.ParagraphFormat.AfterSpacing = 8f;
			IWTextRange daysOfTheWeek = paragraph.AppendText("Week schedule\r" +
				"Monday: \r" +
				"Tuesday: \r" +
				"Wednesday: \r" +
				"Thursday: \r" +
				"Friday: \r");
			daysOfTheWeek.CharacterFormat.FontName = "Aptos";
		}
	}
}
