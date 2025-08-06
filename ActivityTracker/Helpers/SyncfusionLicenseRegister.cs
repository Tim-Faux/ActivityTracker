using System;
using System.Threading.Tasks;
using Syncfusion.Licensing;
using Windows.Storage;

namespace ActivityTracker.Helpers
{
	public class SyncfusionLicenseRegister
	{
		private const string InputFilesFolder = "InputFiles";
		private const string fileName = "SyncfusionLicense.txt";

		public static async Task RegisterSyncfusionLicense() 
		{
			StorageFolder storageFolder;
			try { 
				storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(InputFilesFolder);
			} catch {
				storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(InputFilesFolder);
			}

			var noteFile = (StorageFile) await storageFolder.TryGetItemAsync(fileName);
			if (noteFile is not null) {
				var license = await FileIO.ReadTextAsync(noteFile);
				SyncfusionLicenseProvider.RegisterLicense(license.Trim());
			}
			else {
				await storageFolder.CreateFileAsync(fileName);
			}
		}

		public static async Task SaveSyncfusionLicense(string license)
		{
			StorageFolder storageFolder;
			try {
				storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(InputFilesFolder);
			}
			catch {
				storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(InputFilesFolder);
			}

			var noteFile = (StorageFile) await storageFolder.TryGetItemAsync(fileName);
			if (noteFile is null) {
				noteFile = await storageFolder.CreateFileAsync(fileName);	
			}

			SyncfusionLicenseProvider.RegisterLicense(license.Trim());
			await FileIO.WriteTextAsync(noteFile, license);
		}
	}
}
