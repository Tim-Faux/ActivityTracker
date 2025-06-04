using System.ComponentModel;

namespace ActivityTracker.Models
{
    public partial class Staff : INotifyPropertyChanged
	{
		public string StaffNames { get; set; } = "";

		private string _clientNames = "";
		public string ClientNames { 
			get { 
				return _clientNames;
			}
			set {
				var oldClientNames = _clientNames;
				_clientNames = value;
				ClientsInActivityChanged("ClientNames", oldClientNames, value);
			} 
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void ClientsInActivityChanged(string propertyChangedName, string oldText, string newText)
		{
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyChangedName));

				var oldClientNames = oldText.Split("\r");
				var newClientNames = newText.Split("\r");
				AllClients allClients = new();

				AllClients.UpdateData(oldClientNames, newClientNames);
			}
		}
	}
}
