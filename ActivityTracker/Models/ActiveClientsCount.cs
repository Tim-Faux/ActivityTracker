using System.Collections.Generic;

namespace ActivityTracker.Models
{
    public class ActiveClientsCount
    {
		private Dictionary<string, int> CountOfTimesActivePerClient { get; set; }

		public ActiveClientsCount() { 
			CountOfTimesActivePerClient = new Dictionary<string, int>();
		}

		public void AddClient(string clientName)
		{
			if (!string.IsNullOrWhiteSpace(clientName)) {
				if (CountOfTimesActivePerClient.ContainsKey(clientName)) {
					CountOfTimesActivePerClient[clientName] += 1;
				}
				else {
					CountOfTimesActivePerClient.Add(clientName, 1);
				}
			}
		}

		public void RemoveClient(string clientName)
		{
			if (CountOfTimesActivePerClient.ContainsKey(clientName) && CountOfTimesActivePerClient[clientName] > 0) {
				CountOfTimesActivePerClient[clientName] -= 1;
			}
		}

		public void RemoveAllClients(List<string> clientsToRemove)
		{
			foreach (string clientName in clientsToRemove) {
				RemoveClient(clientName);
			}
		}

		public Dictionary<string, int> GetClientDictionary()
		{
			return CountOfTimesActivePerClient;
		}
	}
}
