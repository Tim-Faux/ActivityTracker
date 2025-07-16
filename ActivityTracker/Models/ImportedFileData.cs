using System;
using System.Collections.Generic;
using ActivityTracker.Helpers;
using Syncfusion.DocIO.DLS;

namespace ActivityTracker.Models
{
	public class ImportedFileData
	{
		public Dictionary<DaysOfTheWeek, SingleDay>? WeekScheduleTableData { get; set; }
		public DateTime? MondayDate { get; set; }
		public IWParagraphCollection? TextAfterWeekSchedule { get; set; }
	}
}
