using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.ViewManagement;

namespace Booru_Viewer.Types
{
	public static class ImageSaver
	{
		public static HttpClient client = new HttpClient();
		private static StorageFolder imageFolder;
		private static int currSaveCount = 0;
		public static StorageFolder ImageFolder
		{
			get
			{
				if (imageFolder == null)
				{
					GetFolder();

				}
				return imageFolder;
			}
			set => imageFolder = value;
		}

		static ImageSaver()
		{
		}


		private static void GetFolder()
		{

			var folderToken = ApplicationData.Current.LocalSettings.Values["SaveFolderToken"] as string;
			if (folderToken != null)
			{
				var folder = StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken).GetResults();
				ImageFolder = folder;
				return;

			}
			
			var item = KnownFolders.PicturesLibrary.TryGetItemAsync("Booru-Viewer").GetResults();
			if (item != null)
			{

				ImageFolder = KnownFolders.PicturesLibrary.GetFolderAsync("Booru-Viewer").GetResults();
			}
			else
			{
				ImageFolder = KnownFolders.PicturesLibrary.CreateFolderAsync("Booru-Viewer").GetResults();
			}
		}
		public static async Task GetFolderAsync()
		{
			var folderToken = ApplicationData.Current.LocalSettings.Values["SaveFolderToken"] as string;
			if (folderToken != null)
			{
				var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
				ImageFolder = folder;
				return;

			}
			var item = await KnownFolders.PicturesLibrary.TryGetItemAsync("Booru-Viewer");
			if (item != null)
			{

				ImageFolder = await KnownFolders.PicturesLibrary.GetFolderAsync("Booru-Viewer");
			}
			else
			{
				ImageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Booru-Viewer");
			}

		}
		public static async Task<string> SaveImage(string ImageURL)
		{
			while (currSaveCount >= 4)
			{
				Debug.WriteLine("Save Count is:" + currSaveCount);
			}
			currSaveCount++;
			var baseURLLength = (BooruAPI.BaseURL + "/data/").Length;
			if (ImageFolder == null)
			{
				try
				{
					await GetFolderAsync();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
					currSaveCount--;
					return "Could not open Save Folder";
					throw;
				}

			}
			var imageName = ImageURL.Substring(baseURLLength);
			imageName = imageName.Replace("/", "").Replace("__", "").Replace("_", " ");
			imageName = imageName.Remove(imageName.Length - 37, 32);
			var imageItem = await ImageFolder.TryGetItemAsync(imageName);
			if (imageItem != null)
			{
				currSaveCount--;
				return "Already saved image";
			}
			try
			{
				var response = await client.GetAsync(ImageURL);
				if (response.IsSuccessStatusCode)
				{
					StorageFile file = await ImageFolder.CreateFileAsync(imageName);
					var bytes = await response.Content.ReadAsByteArrayAsync();
					await FileIO.WriteBytesAsync(file, bytes);
				}
				currSaveCount--;
				return "Image Saved";
				
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				currSaveCount--;
				return "An error ocurred when saving, please try again";
			}
			

		}
	}
}
