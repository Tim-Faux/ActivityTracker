using System.Linq;
using ActivityTracker.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace ActivityTracker.Models
{
    public class AllClients
    {
		private static ActiveClientsCount allClientsCount = new ActiveClientsCount();
		public static void UpdateData(string[] oldClientsInActivityNames, string[] newClientInActivityNames)
		{
			var namesToRemove = oldClientsInActivityNames.Except(newClientInActivityNames);
			allClientsCount.RemoveAllClients(namesToRemove.ToList());

			var clientsToAdd = newClientInActivityNames.Except(oldClientsInActivityNames);

			foreach (var clientName in clientsToAdd) {
				allClientsCount.AddClient(clientName);
			}

			WeakReferenceMessenger.Default.Send(new ActiveClientsListUpdated(new ActiveClientsList { ActiveClientsCount = allClientsCount }));
		}
	}
}
