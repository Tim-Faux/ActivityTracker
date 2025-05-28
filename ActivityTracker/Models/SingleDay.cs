using System.Collections.ObjectModel;

namespace ActivityTracker.Models
{
    public class SingleDay
    {
		const int numberOfStaff = 5;
		public ObservableCollection<Staff> AllStaffPerDay { get; set; } = new ObservableCollection<Staff>();
		public SingleDay()
		{
			for (var i = 0; i < numberOfStaff; i++) {
				AllStaffPerDay.Add(new Staff() { ClientNames = $"{i}", StaffNames = $"{i + numberOfStaff}" });
			}
		}
	}
}
