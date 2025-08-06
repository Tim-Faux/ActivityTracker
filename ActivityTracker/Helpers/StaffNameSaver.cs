using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace ActivityTracker.Helpers
{
	public class StaffNameSaver
	{
		private const string InputFilesFolder = "InputFiles";
		private const string fileName = "StaffNames.txt";
		public static async Task SaveStaff(string staff)
		{
			StorageFolder storageFolder;
			try {
				storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(InputFilesFolder);
			}
			catch {
				storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(InputFilesFolder);
			}

			var staffFile = (StorageFile) await storageFolder.TryGetItemAsync(fileName);
			if (staffFile is null) {
				staffFile = await storageFolder.CreateFileAsync(fileName);
			}
			await FileIO.WriteTextAsync(staffFile, staff);
		}
	}
}
