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
		public static StorageFolder imageFolder;
		static ImageSaver()
		{


		}

		private static async Task GetFolder()
		{
			var item = await KnownFolders.PicturesLibrary.TryGetItemAsync("Booru-Viewer");
			if (item != null)
			{

				imageFolder = await KnownFolders.PicturesLibrary.GetFolderAsync("Booru-Viewer");
			}
			else
			{
				imageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Booru-Viewer");
			}

		}
		public static async Task<string> SaveImage(string ImageURL)
		{
			var baseURLLength = (BooruAPI.BaseURL + "/data/").Length;
			if (imageFolder == null)
			{
				try
				{
					await GetFolder();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
					return "Could not open Booru Viewer Folder";
					throw;
				}
				
			}
			var imageName = ImageURL.Substring(baseURLLength) + ".png";
			imageName = imageName.Replace("/", "");
			var imageItem = await imageFolder.TryGetItemAsync(imageName);
			if (imageItem != null)
			{
				return "Already saved image";
			}
			try
			{
				var response = await client.GetAsync(ImageURL);
				if (response.IsSuccessStatusCode)
				{
					StorageFile file = await imageFolder.CreateFileAsync(imageName);
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
