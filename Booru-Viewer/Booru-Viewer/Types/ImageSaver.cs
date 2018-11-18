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
		public static HttpClient Client = new HttpClient();
		private static StorageFolder _imageFolder;
		public static bool IsSavingImageList { get; private set; }
		public static int TotalImageSaveCount { get; private set; }
		public static int CurrentImageSaveIndex { get; private set; }

		public delegate void ImageFinishedSavingEventHandler(int currentIndex, int totalCount, bool isLastImage, int duplicateCount);

		public static event ImageFinishedSavingEventHandler ImageFinishedSavingEvent;

		public delegate void ImageFolderLoadedEvent();

		public static event ImageFolderLoadedEvent ImageFolderLoadedEventHandler;
		public static StorageFolder ImageFolder
		{
			get
			{
				if (_imageFolder == null)
				{
					GetFolder();

				}
				return _imageFolder;
			}
			set => _imageFolder = value;
		}

		static ImageSaver()
		{
		}


		private static async void GetFolder()
		{
			if (ApplicationData.Current.LocalSettings.Values["SaveFolderToken"] is string folderToken)
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
			ImageFolderLoadedEventHandler?.Invoke();
		}
		public static async Task GetFolderAsync()
		{
			if (ApplicationData.Current.LocalSettings.Values["SaveFolderToken"] is string folderToken)
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
			ImageFolderLoadedEventHandler?.Invoke();
		}
		public static async Task<Tuple<bool, string>> SaveImage(string imageUrl)
		{
			
			
			var baseUrlLength = (BooruApi.BaseUrl + "/data/").Length;
			if (ImageFolder == null)
			{
				try
				{
					await GetFolderAsync();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
					return new Tuple<bool, string>(false, "Could not open folder");
				}

			}
			var imageName = imageUrl.Substring(baseUrlLength);
			var splitName = imageName.Split('/');
			imageName = splitName[splitName.Length - 1];
			imageName = imageName.Replace("_", "").Replace("sample", "").Replace("-", "");
			var imageItem = await ImageFolder.TryGetItemAsync(imageName);
			if (imageItem != null && (await imageItem.GetBasicPropertiesAsync()).Size > 0)
			{
				return new Tuple<bool, string>(true, "Already saved image");
			}
			try
			{
				var response = await Client.GetAsync(imageUrl);
				if (response.IsSuccessStatusCode)
				{
					StorageFile file = await ImageFolder.CreateFileAsync(imageName);
					var bytes = await response.Content.ReadAsByteArrayAsync();
					await FileIO.WriteBytesAsync(file, bytes);
				}
				return new Tuple<bool, string>(true, "Image Saved");

			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				return new Tuple<bool, string>(false, "Failed to save image, " + e.Message);
			}


		}

		public static async Task SaveImagesFromList(List<string> urls)
		{
			IsSavingImageList = true;
			CurrentImageSaveIndex = 0;
			TotalImageSaveCount = urls.Count;
			int dupeCount = 0;
			foreach (var url in urls)
			{
				var result = await SaveImage(url);
				if (result.Item1 && result.Item2.Contains("Already"))
				{
					dupeCount++;
				}

				ImageFinishedSavingEvent?.Invoke(CurrentImageSaveIndex++, TotalImageSaveCount,
					CurrentImageSaveIndex == TotalImageSaveCount - 1, dupeCount);
			}
			IsSavingImageList = false;
		}
	}
}
