using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Services.Common;

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

		public void ImportData(SingleDay data)
		{
			for (var staffIndex = 0; staffIndex < AllStaffPerDay.Count && staffIndex < data.AllStaffPerDay.Count; staffIndex++) {
				AllClients.UpdateData(AllStaffPerDay[staffIndex].ClientNames.Split("\r"), data.AllStaffPerDay[staffIndex].ClientNames.Split("\r"));
			}

			AllStaffPerDay.Clear();
			AllStaffPerDay.AddRange(data.AllStaffPerDay);
			Date = data.Date;
		}
	}
}
