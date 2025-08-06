using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace ActivityTracker.Helpers
{
	public static class ClientNameSaver
	{
		private const string InputFilesFolder = "InputFiles";
		private const string fileName = "ClientNames.txt";
		public static async Task SaveClients(string clients)
		{
			StorageFolder storageFolder;
			try {
				storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(InputFilesFolder);
			}
			catch {
				storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(InputFilesFolder);
			}

			var clientFile = (StorageFile) await storageFolder.TryGetItemAsync(fileName);
			if (clientFile is null) {
				clientFile = await storageFolder.CreateFileAsync(fileName);
			}
			await FileIO.WriteTextAsync(clientFile, clients);
		}
	}
}
