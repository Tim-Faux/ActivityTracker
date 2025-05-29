using System;
using System.Collections.Generic;
using System.Linq;
using ActivityTracker.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace ActivityTracker.Models
{
    public class AllClients
    {
		private static List<string> allClients = [];
		public static void UpdateData(string[] oldClientsInActivityNames, string[] newClientInActivityNames)
		{
			var namesToRemove = oldClientsInActivityNames.Except(newClientInActivityNames);
			allClients.RemoveAll(c => namesToRemove.Contains(c));

			var clientsInActivityUpdated = false;
			foreach (var clientName in newClientInActivityNames) {
				if (!allClients.Contains(clientName)) {
					allClients.Add(clientName);
					clientsInActivityUpdated = true;
				}
			}

			if (clientsInActivityUpdated) {
				WeakReferenceMessenger.Default.Send(new ActiveClientsListUpdated(new ActiveClientsList { ActiveClients = allClients }));
			}
		}
	}
}
