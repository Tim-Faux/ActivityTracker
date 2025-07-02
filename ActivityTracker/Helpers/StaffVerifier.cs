using System.Collections.Generic;

namespace ActivityTracker.Helpers
{
	internal class StaffVerifier
	{
		public List<string> VerifyStaff(string staff, List<string> staffActiveInDay)
		{
			var ErrorMessages = new List<string>();
			var error = CheckForStaffAlreadyInList(staff, staffActiveInDay);
			if (error != null)
				ErrorMessages.Add(error);
			return ErrorMessages;
		}

		private string? CheckForStaffAlreadyInList(string staff, List<string> staffActiveInDay)
		{
			if (staffActiveInDay.Contains(staff)) {
				return "This staff is already present in this day";
			}
			return null;
		}
	}
}
