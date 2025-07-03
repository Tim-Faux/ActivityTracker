using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ActivityTracker.Models
{
    public class SingleDay
    {
		public const int numberOfStaff = 8;
		public ObservableCollection<Staff> AllStaffPerDay { get; set; } = new ObservableCollection<Staff>();
		public DateTimeOffset? Date { get; set; }
		public SingleDay()
		{
			CreateAllStaffPerDayList();
		}

		public void CreateAllStaffPerDayList()
		{
			for (var i = 0; i < numberOfStaff; i++) {
				AllStaffPerDay.Add(new Staff());
			}
		}

		public List<string> GetAllClientsInDay()
		{
			var clientList = new List<string>();
			foreach (var staff in AllStaffPerDay) {
				clientList.AddRange(staff.ClientNames.Split('\r'));
			}
			return clientList;
		}

		public List<string> GetAllStaffInDay()
		{
			var staffList = new List<string>();
			foreach (var staff in AllStaffPerDay) {
				staffList.Add(staff.StaffName);
			}
			return staffList;
		}
	}
}
