using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

namespace Booru_Viewer.Types
{
	public static class ImageSaver
	{
		public static HttpClient client = new HttpClient();
		private static StorageFolder imageFolder;

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


		public static void GetFolder()
		{
			var savedFolderPath = ApplicationData.Current.RoamingSettings.Values["SaveFolderPath"] as string;

			if (!string.IsNullOrEmpty(savedFolderPath))
			{
				ImageFolder = StorageFolder.GetFolderFromPathAsync(savedFolderPath).GetResults();
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
			var savedFolderPath = ApplicationData.Current.RoamingSettings.Values["SaveFolderPath"] as string;

			if (savedFolderPath != null)
			{
				ImageFolder = await StorageFolder.GetFolderFromPathAsync(savedFolderPath);
				return;
			}
			var item = await KnownFolders.PicturesLibrary.TryGetItemAsync("Booru-Viewer");
			if (item != null)
			{

				ImageFolder = await KnownFolders.PicturesLibrary.GetFolderAsync(savedFolderPath ?? "Booru-Viewer");
			}
			else
			{
				ImageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Booru-Viewer");
			}

		}
		public static async Task<string> SaveImage(string ImageURL)
		{
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
					return "Could not open Booru Viewer Folder";
					throw;
				}

			}
			var imageName = ImageURL.Substring(baseURLLength);
			imageName = imageName.Replace("/", "");
			var imageItem = await ImageFolder.TryGetItemAsync(imageName);
			if (imageItem != null)
			{
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
				return "Image Saved";
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				throw;
			}


		}
	}
}
