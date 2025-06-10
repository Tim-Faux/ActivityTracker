using System.Collections.ObjectModel;

namespace ActivityTracker.Models
{
    public class SingleDay
    {
		const int numberOfStaff = 8;
		public ObservableCollection<Staff> AllStaffPerDay { get; set; } = new ObservableCollection<Staff>();
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
	}
}
