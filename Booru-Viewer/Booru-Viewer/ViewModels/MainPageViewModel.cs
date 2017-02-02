using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml;
using Windows.Storage;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using Windows.Web.Http;
using Windows.UI.Xaml.Controls;

//TODO: Saved Searches (should be somewhat straightforward)
namespace Booru_Viewer.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		private int page = 1;
		public MainPageViewModel()
		{

			var appSettings = ApplicationData.Current.RoamingSettings.Values;
			var savedUN = appSettings["Username"] as string;
			var savedAPIKey = appSettings["APIKey"] as string;
			if (!string.IsNullOrEmpty(savedUN))
			{
				Debug.WriteLine("username not empty it's " + appSettings["Username"] + ". APIKey is " + appSettings["APIKey"]);

				Username = savedUN;
				if (!string.IsNullOrEmpty(savedAPIKey))
				{

					APIKey = savedAPIKey;
					BooruAPI.SetLogin(savedUN, savedAPIKey);
				}
				else
				{
					APIKey = "";
					BooruAPI.SetLogin(savedUN, "");
				}

			}

		}


		//private bool shouldClearFocus = false;
		//public bool ShouldClearFocus
		//{
		//	get { return shouldClearFocus; }
		//	set
		//	{
		//		shouldClearFocus = value;
		//		RaisePropertyChanged();
		//	}
		//}
		private ObservableCollection<ThumbnailViewModel> thumbnails = new ObservableCollection<ThumbnailViewModel>();
		public ObservableCollection<ThumbnailViewModel> Thumbnails { get { return thumbnails; } }

		public ObservableCollection<TagViewModel> CurrentTags
		{
			get { RaisePropertyChanged(); return GlobalInfo.CurrentTags; }
		}

		private string currentTag = "";

		public string CurrentTag
		{
			get
			{
				return currentTag;
			}
			set { currentTag = value; RaisePropertyChanged(); }
		}

		private string username;
		public string Username
		{
			get { return BooruAPI.Username; }
			set
			{
				username = value;
				RaisePropertyChanged();
			}
		}

		private string apiKey;
		public string APIKey
		{
			get { return BooruAPI.APIKey; }
			set
			{

				apiKey = value;
				RaisePropertyChanged();
			}
		}

		private bool haveImages = true;

		public bool HaveImages
		{
			get { return haveImages; }
			set
			{
				haveImages = value;
				RaisePropertyChanged();
			}
		}

		private string noImagesText = "Press the search key to get started";
		public string NoImagesText
		{
			get { return noImagesText; }
			set
			{
				noImagesText = value;
				RaisePropertyChanged();
			}
		}


		private bool isMultiSelectOn = false;
		public SelectionMode ImageSelectionMode
		{
			get { return isMultiSelectOn ? SelectionMode.Multiple : SelectionMode.Single; }
			set
			{
				isMultiSelectOn =  value == SelectionMode.Multiple;
				RaisePropertyChanged();
			}
		}
		public SymbolIcon MultiSelectButtonIcon
		{
			get { return isMultiSelectOn ? new SymbolIcon(Symbol.Cancel) : new SymbolIcon(Symbol.SelectAll); }
		}
		public void RemoveTag(TagViewModel tag)
		{
			CurrentTags.Remove(tag);
			GlobalInfo.CurrentTags.Remove(tag);
			RaisePropertyChanged("CurrentTags");
		}

		void AddTagExecute()
		{

			CurrentTags.Add(new TagViewModel(CurrentTag, this));
			CurrentTag = "";
			RaisePropertyChanged("CurrentTags");
		}

		bool AddTagCanExecute()
		{
			return true;
		}

		bool SearchCanExecute()
		{
			return true;
		}


		void SaveLoginDataExecute()
		{
			RaisePropertyChanged("APIKey");
			BooruAPI.SetLogin(username, apiKey);

			ApplicationData.Current.RoamingSettings.Values["Username"] = BooruAPI.Username;
			ApplicationData.Current.RoamingSettings.Values["APIKey"] = BooruAPI.APIKey;
		}

		bool SaveLoginDataCanExecute()
		{
			return true;
		}

		async void StartSearchExecute()
		{
			GlobalInfo.CurrentSearch.Clear();
			ResyncThumbnails();
			var tags = await PrepTags();
			var result = await BooruAPI.SearchPosts(tags, 0);
			if (result.Item2.Count > 0)
			{
				HaveImages = true;
			}
			else
			{
				HaveImages = false;
				NoImagesText = "No Images Found with those tags, try a different combination";
			}
			if (result.Item3 == HttpStatusCode.Ok)
			{
				AddThumbnails(result.Item2);
			}
		}

		void AddThumbnails(List<ImageModel> thumbnails)
		{

			foreach (var post in thumbnails)
			{
				Thumbnails.Add(new ThumbnailViewModel { PreviewURL = BooruAPI.BaseURL + post.Large_File_Url, FullURL = BooruAPI.BaseURL + post.Large_File_Url });
			}


			RaisePropertyChanged("Thumbnails");
		}

		async Task<string[]> PrepTags()
		{
			string[] tags = new string[CurrentTags.Count];
			var tagModels = new ObservableCollection<TagViewModel>(CurrentTags);
			await Task.Run(() =>
			 {
				 var i = 0;
				 foreach (var tag in tagModels)
				 {
					 if (!string.IsNullOrEmpty(tag.Tag))
					 {
						 tags[i] = tag.Tag.Replace(" ", "_") + " ";
					 }
					 i++;
				 }
			 });


			return tags;
		}

		void ResyncThumbnails()
		{
			Thumbnails.Clear();
			foreach (var post in GlobalInfo.CurrentSearch)
			{
				Thumbnails.Add(new ThumbnailViewModel { PreviewURL = BooruAPI.BaseURL + post.Large_File_Url, FullURL = BooruAPI.BaseURL + post.Large_File_Url });
			}

			RaisePropertyChanged("Thumbnails");
		}

		bool StartSearchCanExecute()
		{
			return true;
		}

		async void LoadNextPageExecute()
		{
			page++;
			var result = await BooruAPI.SearchPosts(await PrepTags(), page, false);
			if (result.Item3 == HttpStatusCode.Ok)
			{
				AddThumbnails(result.Item2);
			}
		}
		bool LoadNextPageCanExecute()
		{
			return true;
		}
		public ICommand AddTag { get { return new RelayCommand(AddTagExecute, AddTagCanExecute); } }

		public ICommand SaveLoginData { get { return new RelayCommand(SaveLoginDataExecute, SaveLoginDataCanExecute); } }
		public ICommand StartSearch { get { return new RelayCommand(StartSearchExecute, StartSearchCanExecute); } }
		public ICommand LoadNextPage { get { return new RelayCommand(LoadNextPageExecute, LoadNextPageCanExecute); } }
	}
}
