﻿using Booru_Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace Booru_Viewer.Types
{
	public static class GlobalInfo
	{
		public static ObservableCollection<ImageModel> CurrentSearch { get; set; } = new ObservableCollection<ImageModel>();
		public static ObservableCollection<TagViewModel> CurrentTags { get; set; } = new ObservableCollection<TagViewModel>();
		private static ObservableCollection<string[]> savedSearches;

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

		public async static void SaveSearches()
		{

			var item = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("SavedSearches.json");
			if (item != null)
			{
				SearchesFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("SavedSearches.json");
			}
			else
			{
				SearchesFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync("SavedSearches.json");
			}

			var json = JsonConvert.SerializeObject(SavedSearches.ToList());
			await FileIO.WriteTextAsync(SearchesFile, json);
		}

		public static async void LoadSavedSearches()
		{
			savedSearches.Clear();
			var item = await ApplicationData.Current.RoamingFolder.TryGetItemAsync("SavedSearches.json");
			if (item == null)
			{
				return;
			}
			SearchesFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("SavedSearches.json");
			var json = await FileIO.ReadTextAsync(SearchesFile);
			var searchList = JsonConvert.DeserializeObject<List<string[]>>(json);
			if (searchList != null)
			{
				foreach (var search in searchList)
				{
					savedSearches.Add(search);
				}
			}
		}
	}
}
