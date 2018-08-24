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
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.HockeyApp.DataContracts;

namespace Booru_Viewer.Types
{
	public static class GlobalInfo
	{
		public static ObservableCollection<ImageModel> CurrentSearch { get; set; } = new ObservableCollection<ImageModel>();
		public static IncrementalLoadingCollection<PostSource, FullImageViewModel> ImageViewModels { get; set; } = new IncrementalLoadingCollection<PostSource, FullImageViewModel>();

		public static ObservableCollection<TagViewModel> CurrentTags { get; set; } = new ObservableCollection<TagViewModel>();
		public static List<string> CurrentSearchTags { get; set; } = new List<string>();
		private static string currentOrdering = "";

		public static EventHandler FavouriteImagesLoadedEventHandler;
		private static List<string> favouriteImages;

		public static List<string> FavouriteImages
		{
			get
			{
				if (favouriteImages == null)
				{
					favouriteImages = new List<string>();
					LoadFavouritePosts();
				}

				return favouriteImages;
			}
			set => favouriteImages = value;
		}

		public static string CurrentOrdering
		{
			get => currentOrdering; set => currentOrdering = (value == "" ? "" : "order:" + value);
		}

		public static bool[] ContentCheck { get; set; } = { true, true, true };

		private static ObservableCollection<string[]> savedSearches;
		private static ObservableCollection<Tag> favouriteTags;
		public static ObservableCollection<Tag> FavouriteTags
		{
			get
			{
				if (favouriteTags == null)
				{
					favouriteTags = new ObservableCollection<Tag>();
					LoadFavouriteTags();
				}
				return favouriteTags;
			}
			set => favouriteTags = value;
		}

		public static EventHandler SavedSearchesLoaded;
		public static EventHandler FavouriteTagsLoaded;
		public static ObservableCollection<string[]> SavedSearches
		{
			get
			{
				if (savedSearches == null)
				{
					savedSearches = new ObservableCollection<string[]>();
					LoadSavedSearches();
				}

				return savedSearches;
			}
		}

		public static int SelectedImage { get; set; } = 0;
		private static StorageFile SearchesFile;
		public static void RemoveTag(TagViewModel tag)
		{
			CurrentTags.Remove(tag);
		}

		public static EventHandler ImagesSavedForLaterLoaded;
		private static List<ImageModel> imagesSavedForLater = null;

		public static List<ImageModel> ImagesSavedForLater
		{
			get
			{
				if (imagesSavedForLater == null)
				{
					imagesSavedForLater = new List<ImageModel>();
					LoadSavedForLaterImages();
				}

				return imagesSavedForLater;
			}
		}

		public static async Task SaveSearches(StorageFolder baseFolder = null)
		{
			SaveDataToFile(savedSearches.ToList(), "SavedSearches.json");
		}

		public static async Task LoadSavedSearches(StorageFolder searchesFolder = null)
		{

			var searches = await LoadDataToObject<List<string[]>>("SavedSearches.json");
			savedSearches.Clear();
			foreach (var search in searches)
			{
				savedSearches.Add(search);
			}
			SavedSearchesLoaded?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}

		public static async Task<ObservableCollection<Tag>> GetFavouriteTags()
		{
			await LoadFavouriteTags();
			return favouriteTags;

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
					SearchesFile = await folder.GetFileAsync(fileName);
				}
				else
				{
					SearchesFile = await folder.CreateFileAsync(fileName);
				}

				var json = JsonConvert.SerializeObject(data);
				await FileIO.WriteTextAsync(SearchesFile, json);
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
			SearchesFile = await folder.GetFileAsync(fileName);
			var json = await FileIO.ReadTextAsync(SearchesFile);
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
			await SaveDataToFile(imagesSavedForLater, "ImagesSavedForLater.json");
		}

		public static async Task LoadSavedForLaterImages(StorageFolder baseFolder = null)
		{
			imagesSavedForLater = await LoadDataToObject<List<ImageModel>>("ImagesSavedForLater.json");
			ImagesSavedForLaterLoaded?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}

		public static async Task SaveFavouriteTags(StorageFolder baseFolder = null)
		{
			await SaveDataToFile(favouriteTags.ToList(), "FavouriteTags.json");

		}

		public static async Task LoadFavouriteTags(StorageFolder tagsFolder = null)
		{

			var tags = await LoadDataToObject<List<Tag>>("FavouriteTags.json");
			favouriteTags.Clear();
			foreach (var search in tags)
			{
				var tag = search;

				tag.Name = tag.Name.TrimStart('-', '~');
				if (!favouriteTags.Any(x => x.Name == tag.Name))
				{
					favouriteTags.Add(tag);
				}

			}
			FavouriteTagsLoaded?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}

		public static async Task SaveFavouritePosts(StorageFolder baseFolder = null)
		{
			await SaveDataToFile(favouriteImages, "FavouritePosts.json");
		}
		public static async Task LoadFavouritePosts(StorageFolder searchesFolder = null)
		{
			var favouritePosts = await LoadDataToObject<List<string>>("FavouritePosts.json");
			favouriteImages.Clear();
			foreach (var image in favouritePosts)
			{

				favouriteImages.Add(image);
			}
			FavouriteImagesLoadedEventHandler?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}
	}
}
