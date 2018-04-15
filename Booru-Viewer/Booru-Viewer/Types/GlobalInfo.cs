using Booru_Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Booru_Viewer.Models;
using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;

namespace Booru_Viewer.Types
{
	public static class GlobalInfo
	{
		public static ObservableCollection<ImageModel> CurrentSearch { get; set; } = new ObservableCollection<ImageModel>();
		public static IncrementalLoadingCollection<PostSource, FullImageViewModel> ImageViewModels { get; set; } = new IncrementalLoadingCollection<PostSource, FullImageViewModel>();

		public static ObservableCollection<TagViewModel> CurrentTags { get; set; } = new ObservableCollection<TagViewModel>();
		public static List<string> CurrentSearchTags { get; set; } = new List<string>();
		private static string currentOrdering = "";

		public static string CurrentOrdering
		{
			get => currentOrdering; set => currentOrdering = (value == "" ? "" : "order:" + value);
		}

		public static bool[] ContentCheck { get; set; } = { true, true, true };

		private static ObservableCollection<string[]> savedSearches;
		private static ObservableCollection<Tag> excludedTags;

		public static ObservableCollection<Tag> ExcludedTags
		{
			get
			{
				if (excludedTags == null)
				{
					excludedTags = new ObservableCollection<Tag>();
					LoadExcludedTags();
				}

				return excludedTags;
			}
			set => excludedTags = value;
		}
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

		public static EventHandler ExcludedTagsLoadedEventHandler;
		public static EventHandler SavedSearchesLoadedEventHandler;
		public static EventHandler FavouriteTagsLoadedEventHandler;
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

		public static async Task SaveSearches(List<SavedSearchViewModel> searches, StorageFolder baseFolder = null)
		{
			if (searches == null)
			{
				throw new NullReferenceException("searches list can't be null");
			}
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

				var item = await folder.TryGetItemAsync("SavedSearches.json");
				if (item != null)
				{
					SearchesFile = await folder.GetFileAsync("SavedSearches.json");
				}
				else
				{
					SearchesFile = await folder.CreateFileAsync("SavedSearches.json");
				}

				var json = JsonConvert.SerializeObject(searches.Select(x => x.Tags));
				await FileIO.WriteTextAsync(SearchesFile, json);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);

			}

		}

		public static async Task LoadSavedSearches(StorageFolder searchesFolder = null)
		{


			StorageFolder folder = ApplicationData.Current.RoamingFolder;
			if (searchesFolder != null)
			{
				folder = searchesFolder;
			}
			var item = await folder.TryGetItemAsync("SavedSearches.json");
			if (item == null)
			{
				Debug.WriteLine("File was null");
				return;
			}
			SearchesFile = await folder.GetFileAsync("SavedSearches.json");
			var json = await FileIO.ReadTextAsync(SearchesFile);
			try
			{
				var searchList = JsonConvert.DeserializeObject<List<string[]>>(json);
				if (searchList != null)
				{
					savedSearches.Clear();
					foreach (var search in searchList)
					{

						savedSearches.Add(search);
					}
				}
				else
				{
					Debug.WriteLine("SearchList was null");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}


			SavedSearchesLoadedEventHandler?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}

		public static async Task SaveExcludedTags(StorageFolder baseFolder = null)
		{
			if (favouriteTags == null)
			{
				await LoadFavouriteTags();
			}
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

				var item = await folder.TryGetItemAsync("ExcludedTags.json");
				if (item != null)
				{
					SearchesFile = await folder.GetFileAsync("ExcludedTags.json");
				}
				else
				{
					SearchesFile = await folder.CreateFileAsync("ExcludedTags.json");
				}

				var json = JsonConvert.SerializeObject(excludedTags.ToList());
				await FileIO.WriteTextAsync(SearchesFile, json);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);

			}

		}

		public static async Task LoadExcludedTags(StorageFolder tagsFolder = null)
		{
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
			var item = await folder.TryGetItemAsync("ExcludedTags.json");
			if (item == null)
			{
				Debug.WriteLine("File was null");
				return;
			}
			SearchesFile = await folder.GetFileAsync("ExcludedTags.json");
			var json = await FileIO.ReadTextAsync(SearchesFile);
			try
			{
				var searchList = JsonConvert.DeserializeObject<List<Tag>>(json, new JsonSerializerSettings
				{
					Error = (sender, args) =>
					{
						args.ErrorContext.Handled = true;
					}
				});
				if (searchList != null)
				{
					excludedTags.Clear();
					foreach (var search in searchList)
					{
						var tag = search;
						
						tag.Name = tag.Name.TrimStart('-', '~');

						excludedTags.Add(tag);
					}
				}
				else
				{
					Debug.WriteLine("ExcludedTagslist was null");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}


			ExcludedTagsLoadedEventHandler?.Invoke(typeof(GlobalInfo), EventArgs.Empty);

			
		}

		public static async Task<ObservableCollection<Tag>> GetFavouriteTags()
		{
			await LoadFavouriteTags();
			return favouriteTags;

		}

		public static async Task SaveFavouriteTags(StorageFolder baseFolder = null)
		{
			if (favouriteTags == null)
			{
				await LoadFavouriteTags();
			}
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

				var item = await folder.TryGetItemAsync("FavouriteTags.json");
				if (item != null)
				{
					SearchesFile = await folder.GetFileAsync("FavouriteTags.json");
				}
				else
				{
					SearchesFile = await folder.CreateFileAsync("FavouriteTags.json");
				}

				var json = JsonConvert.SerializeObject(favouriteTags.ToList());
				await FileIO.WriteTextAsync(SearchesFile, json);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);

			}

		}

		public static async Task LoadFavouriteTags(StorageFolder tagsFolder = null)
		{
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
			var item = await folder.TryGetItemAsync("FavouriteTags.json");
			if (item == null)
			{
				Debug.WriteLine("File was null");
				return;
			}
			SearchesFile = await folder.GetFileAsync("FavouriteTags.json");
			var json = await FileIO.ReadTextAsync(SearchesFile);
			try
			{
				var searchList = JsonConvert.DeserializeObject<List<Tag>>(json, new JsonSerializerSettings
				{
					Error = (sender, args) =>
					{
						args.ErrorContext.Handled = true;
					}
				});
				if (searchList != null)
				{
					favouriteTags.Clear();
					foreach (var search in searchList)
					{
						var tag = search;
						
						tag.Name = tag.Name.TrimStart('-', '~');

					favouriteTags.Add(tag);
					}
				}
				else
				{
					Debug.WriteLine("FavouriteTagslist was null");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}


			FavouriteTagsLoadedEventHandler?.Invoke(typeof(GlobalInfo), EventArgs.Empty);
		}
	}
}
