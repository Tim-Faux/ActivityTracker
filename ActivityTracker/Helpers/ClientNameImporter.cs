using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace ActivityTracker.Helpers
{
	public class ClientNameImporter
	{
		private const string InputFilesFolder = "InputFiles";
		private const string fileName = "ClientNames.txt";

		public async static Task<List<string>> ImportClients()
		{
			StorageFolder storageFolder;
			try {
				storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(InputFilesFolder);
			}
			catch {
				storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(InputFilesFolder);
			}

			var noteFile = (StorageFile) await storageFolder.TryGetItemAsync(fileName);
			if (noteFile is not null) {
				return (await FileIO.ReadLinesAsync(noteFile)).ToList();
			}
			else {
				await storageFolder.CreateFileAsync(fileName);
			}
			return new List<string>();
		}
	}
}
