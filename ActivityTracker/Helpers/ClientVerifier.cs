using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityTracker.Helpers
{
	public class ClientVerifier
	{
		public List<string> VerifyClient(string client, List<string> clientsActiveInDay) { 
			var ErrorMessages = new List<string>();
			var error = CheckForClientAlreadyInList(client, clientsActiveInDay);
			if (error != null) 
				ErrorMessages.Add(error);
			return ErrorMessages;
		}

		private string? CheckForClientAlreadyInList(string client, List<string> clientsActiveInDay) { 
			if (clientsActiveInDay.Contains(client)) {
				return "This client is already present in this day";
			}
			return null;
		}
	}
}
