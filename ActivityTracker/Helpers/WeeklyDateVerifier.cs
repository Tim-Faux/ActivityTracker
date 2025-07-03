using System;
using System.Collections.Generic;

namespace ActivityTracker.Helpers
{
	public static class WeeklyDateVerifier
	{
		public static List<string?> VerifyDate(DateTime selectedDate, DayOfWeek dayOfWeek)
		{
			var ErrorMessages = new List<string?>
			{
				SelectedDateIsAcceptedDayOfWeek(selectedDate, dayOfWeek)
			};
			ErrorMessages.RemoveAll(e => e == null);

			return ErrorMessages;
		}

		private static string? SelectedDateIsAcceptedDayOfWeek(DateTime selectedDate, DayOfWeek acceptedDayOfWeek)
		{
			if (selectedDate.DayOfWeek != acceptedDayOfWeek) {
				return "Date is not the correct day of the week";
			}
			return null;
		}
	}
}
