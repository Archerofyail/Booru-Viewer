using Booru_Viewer.Models;
using Booru_Viewer.ViewModels;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Booru_Viewer.Types
{
	public static class GlobalInfo
	{
		public static ObservableCollection<ImageModel> CurrentSearch { get; set; } = new ObservableCollection<ImageModel>();
		public static IncrementalLoadingCollection<PostSource, FullImageViewModel> ImageViewModels { get; set; } = new IncrementalLoadingCollection<PostSource, FullImageViewModel>();

		public static ObservableCollection<TagViewModel> CurrentTags { get; set; } = new ObservableCollection<TagViewModel>();
		public static List<string> CurrentSearchTags { get; set; } = new List<string>();
		private static string _currentOrdering = "";

		public static EventHandler FavouriteImagesLoadedEventHandler;
		private static List<string> _favouriteImages;

		public static List<string> FavouriteImages
		{
			get
			{
				if (_favouriteImages == null)
				{
					_favouriteImages = new List<string>();
					LoadFavouritePosts();
				}

				return _favouriteImages;
			}
			set => _favouriteImages = value;
		}

		public static string CurrentOrdering
		{
			get => _currentOrdering; set => _currentOrdering = (value == "" ? "" : "order:" + value);
		}

		public static bool[] ContentCheck { get; set; } = { true, true, true };

		private static ObservableCollection<string[]> _savedSearches;
		private static ObservableCollection<Tag> _favouriteTags;
		public static ObservableCollection<Tag> FavouriteTags
		{
			get
			{
				if (_favouriteTags == null)
				{
					_favouriteTags = new ObservableCollection<Tag>();
					LoadFavouriteTags();
				}
				return _favouriteTags;
			}
			set => _favouriteTags = value;
		}

		public static EventHandler SavedSearchesLoaded;
		public static EventHandler FavouriteTagsLoaded;
		public static ObservableCollection<string[]> SavedSearches
		{
			get
			{
				if (_savedSearches == null)
				{
					_savedSearches = new ObservableCollection<string[]>();
					LoadSavedSearches();
				}

				return _savedSearches;
			}
		}

		public static int SelectedImage { get; set; } = 0;
		private static StorageFile _searchesFile;
		public static void RemoveTag(TagViewModel tag)
		{
			CurrentTags.Remove(tag);
		}

		public static EventHandler ImagesSavedForLaterLoaded;
		private static List<ImageModel> _imagesSavedForLater = null;

		public static List<ImageModel> ImagesSavedForLater
		{
			get
			{
				if (_imagesSavedForLater == null)
				{
					_imagesSavedForLater = new List<ImageModel>();
					LoadSavedForLaterImages();
				}

				return _imagesSavedForLater;
			}
		}

		public static async Task SaveSearches(StorageFolder baseFolder = null)
		{
			SaveDataToFile(_savedSearches.ToList(), "SavedSearches.json");
		}

		public static async Task LoadSavedSearches(StorageFolder searchesFolder = null)
		{

			var searches = await LoadDataToObject<List<string[]>>("SavedSearches.json");
			_savedSearches.Clear();
			foreach (var search in searches)
			{
				_savedSearches.Add(search);
			}
			SavedSearchesLoaded?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}

		public static async Task<ObservableCollection<Tag>> GetFavouriteTags()
		{
			await LoadFavouriteTags();
			return _favouriteTags;

		}

		private static async Task SaveDataToFile(object data, string fileName, StorageFolder baseFolder = null)
		{
			try
			{
				StorageFolder folder;
				if (baseFolder != null)
				{
					if (baseFolder.DisplayName != "Booru-Viewer")
					{
						var potFold = await baseFolder.TryGetItemAsync("Booru-Viewer");
						if (potFold != null)
						{
							folder = await baseFolder.GetFolderAsync("Booru-Viewer");
						}
						else
						{
							folder = await baseFolder.CreateFolderAsync("Booru-Viewer");
						}
					}
					else
					{
						folder = baseFolder;
					}
				}
				else
				{
					folder = ApplicationData.Current.RoamingFolder;
				}

				var item = await folder.TryGetItemAsync(fileName);
				if (item != null)
				{
					_searchesFile = await folder.GetFileAsync(fileName);
				}
				else
				{
					_searchesFile = await folder.CreateFileAsync(fileName);
				}

				var json = JsonConvert.SerializeObject(data);
				await FileIO.WriteTextAsync(_searchesFile, json);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

		}

		private static async Task<T> LoadDataToObject<T>(string fileName, StorageFolder tagsFolder = null) where T : class, new()
		{
			T data = null;
			StorageFolder folder;
			if (tagsFolder != null)
			{
				if (tagsFolder.DisplayName != "Booru-Viewer")
				{
					var potFold = await tagsFolder.TryGetItemAsync("Booru-Viewer");
					if (potFold != null)
					{
						folder = await tagsFolder.GetFolderAsync("Booru-Viewer");
					}
					else
					{
						folder = await tagsFolder.CreateFolderAsync("Booru-Viewer");
					}
				}
				else
				{
					folder = tagsFolder;
				}
			}
			else
			{
				folder = ApplicationData.Current.RoamingFolder;
			}
			if (tagsFolder != null)
			{
				folder = tagsFolder;
			}
			var item = await folder.TryGetItemAsync(fileName);
			if (item == null)
			{
				Debug.WriteLine("File was null");
				return new T();
			}
			_searchesFile = await folder.GetFileAsync(fileName);
			var json = await FileIO.ReadTextAsync(_searchesFile);
			try
			{
				data = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
				{
					Error = (sender, args) =>
					{
						args.ErrorContext.Handled = true;
					}
				});
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			return data;

		}

		public static async Task SaveSavedForLaterImages(StorageFolder baseFolder = null)
		{
			await SaveDataToFile(_imagesSavedForLater, "ImagesSavedForLater.json");
		}

		public static async Task LoadSavedForLaterImages(StorageFolder baseFolder = null)
		{
			_imagesSavedForLater = await LoadDataToObject<List<ImageModel>>("ImagesSavedForLater.json");
			ImagesSavedForLaterLoaded?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}

		public static async Task SaveFavouriteTags(StorageFolder baseFolder = null)
		{
			await SaveDataToFile(_favouriteTags.ToList(), "FavouriteTags.json");

		}

		public static async Task LoadFavouriteTags(StorageFolder tagsFolder = null)
		{

			var tags = await LoadDataToObject<List<Tag>>("FavouriteTags.json");
			_favouriteTags.Clear();
			foreach (var search in tags)
			{
				var tag = search;

				tag.Name = tag.Name.TrimStart('-', '~');
				if (!_favouriteTags.Any(x => x.Name == tag.Name))
				{
					_favouriteTags.Add(tag);
				}

			}
			FavouriteTagsLoaded?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}

		public static async Task SaveFavouritePosts(StorageFolder baseFolder = null)
		{
			await SaveDataToFile(_favouriteImages, "FavouritePosts.json");
		}
		public static async Task LoadFavouritePosts(StorageFolder searchesFolder = null)
		{
			var favouritePosts = await LoadDataToObject<List<string>>("FavouritePosts.json");
			_favouriteImages.Clear();
			foreach (var image in favouritePosts)
			{

				_favouriteImages.Add(image);
			}
			FavouriteImagesLoadedEventHandler?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}
	}
}
