using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

		public static async Task GetFolder()
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
		public static async void SaveImage(string ImageURL)
		{
			if (imageFolder == null)
			{
				await GetFolder();
			}
			var response = await client.GetAsync(ImageURL);
			if (response.IsSuccessStatusCode)
			{

				StorageFile file = await imageFolder.CreateFileAsync("Image " + (await imageFolder.GetFilesAsync()).Count + ".png");
				var bytes = await response.Content.ReadAsByteArrayAsync();
				await FileIO.WriteBytesAsync(file, bytes);
			}
		}
	}
}
