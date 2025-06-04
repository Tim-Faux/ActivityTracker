using ActivityTracker.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ActivityTracker.Messages
{
    public class ActiveClientsListUpdated : ValueChangedMessage<ActiveClientsList>
	{
		public ActiveClientsListUpdated(ActiveClientsList activeClientsList) : base(activeClientsList)
		{
		}
	}
}
