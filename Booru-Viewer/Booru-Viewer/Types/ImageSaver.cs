using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Booru_Viewer.Types
{
	public static class ImageSaver
	{
		public static HttpClient client = new HttpClient();
		private static StorageFolder imageFolder;
		public static bool IsSavingImageList { get; private set; }
		public static int TotalImageSaveCount { get; private set; }
		public static int CurrentImageSaveIndex { get; private set; }

		public delegate void ImageFinishedSavingEventHandler(int currentIndex, int totalCount, bool isLastImage);

		public static event ImageFinishedSavingEventHandler ImageFinishedSavingEvent;

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
					return "Could not open Save Folder";
				}

			}
			var imageName = ImageURL.Substring(baseURLLength);
			var splitName = imageName.Split('/');
			imageName = splitName[splitName.Length - 1];
			imageName = imageName.Replace("_", "").Replace("sample", "").Replace("-", "");
			var imageItem = await ImageFolder.TryGetItemAsync(imageName);
			if (imageItem != null && (await imageItem.GetBasicPropertiesAsync()).Size > 0)
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
				return "An error ocurred when saving, please try again";
			}


		}

		public static async void SaveImagesFromList(List<string> urls)
		{
			IsSavingImageList = true;
			CurrentImageSaveIndex = 0;
			TotalImageSaveCount = urls.Count;
			foreach (var url in urls)
			{
				await SaveImage(url);
				ImageFinishedSavingEvent?.Invoke(CurrentImageSaveIndex++, TotalImageSaveCount,
					CurrentImageSaveIndex == TotalImageSaveCount - 1);
			}
			IsSavingImageList = false;
		}
	}
}
