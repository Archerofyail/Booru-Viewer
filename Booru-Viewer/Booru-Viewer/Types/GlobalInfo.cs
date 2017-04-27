﻿using Booru_Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
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
			get => currentOrdering; set => currentOrdering = value == "" ? "" : "order:" + value;
		}

		public static bool[] ContentCheck { get; set; } = {true, true, true};

		private static ObservableCollection<string[]> savedSearches;
		public static EventHandler SavedSearchesLoadedEventHandler;
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
					var potFold = await baseFolder.TryGetItemAsync("BooruViewer");
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

		public static async void LoadSavedSearches()
		{
			savedSearches.Clear();
			var item = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("SavedSearches.json");
			if (item == null)
			{
				Debug.WriteLine("File was null");
				return;
			}
			SearchesFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("SavedSearches.json");
			var json = await FileIO.ReadTextAsync(SearchesFile);
			try
			{
				var searchList = JsonConvert.DeserializeObject<List<string[]>>(json);
				if (searchList != null)
				{
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
	}
}
