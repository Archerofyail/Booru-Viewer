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

namespace Booru_Viewer.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
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
		public ObservableCollection<string> ImageLinks { get { RaisePropertyChanged(); return GlobalInfo.ImageURLs; } }
		
		public ObservableCollection<TagViewModel> CurrentTags { get { RaisePropertyChanged(); return GlobalInfo.CurrentTags; } 
		}

		private string currentTag = "";

		public string CurrentTag {
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

		public void RemoveTag(TagViewModel tag)
		{
			CurrentTags.Remove(tag);
			RaisePropertyChanged("CurrentTags");
		}

		void AddTagExecute()
		{
			
			CurrentTags.Add(new TagViewModel(CurrentTag));
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

		void SearchExecute()
		{
			
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

		void StartSearchExecute()
		{

		}

		bool StartSearchCanExecute()
		{
			return true;
		}

		public ICommand AddTag { get { return  new RelayCommand(AddTagExecute, AddTagCanExecute);} }
		public ICommand Search { get { return new RelayCommand(SearchExecute, SearchCanExecute);} }
		public ICommand SaveLoginData { get { return new RelayCommand(SaveLoginDataExecute, SaveLoginDataCanExecute); } }
		public ICommand StartSearch { get { return new RelayCommand(StartSearchExecute, StartSearchCanExecute); } }
	}
}
