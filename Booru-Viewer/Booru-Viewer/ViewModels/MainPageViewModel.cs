using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml;
using Windows.Storage;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;
using Windows.Foundation.Collections;
using GalaSoft.MvvmLight;
using Windows.Web.Http;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;

//TODO: Saved Searches (should be somewhat straightforward)
namespace Booru_Viewer.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		private IPropertySet appSettings;
		public MainPageViewModel()
		{

			appSettings = ApplicationData.Current.RoamingSettings.Values;
			var savedUN = appSettings["Username"] as string;
			var savedAPIKey = appSettings["APIKey"] as string;
			if (appSettings["PerPage"] != null)
			{
				perPage = (int)appSettings["PerPage"];
			}
			if (ApplicationData.Current.LocalSettings.Values["ImageSize"] != null)
			{
				ImageSize = (int)ApplicationData.Current.LocalSettings.Values["ImageSize"];
			}
			if (appSettings["SafeChecked"] != null)
			{
				safeChecked = (bool)appSettings["SafeChecked"];
			}
			if (appSettings["QuestionableChecked"] != null)
			{
				questionableChecked = (bool)appSettings["QuestionableChecked"];
			}
			if (appSettings["ExplicitChecked"] != null)
			{
				explicitChecked = (bool)appSettings["ExplicitChecked"];
			}



			GlobalInfo.ContentCheck[0] = safeChecked;
			GlobalInfo.ContentCheck[1] = questionableChecked;
			GlobalInfo.ContentCheck[2] = explicitChecked;

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

			StartSearchExecute();
			Debug.WriteLine("Count for saved Searches is " + SavedSearches.Count);
			BooruAPI.TagSearchCompletedHandler += (sender, tuple) =>
			{
				if (tuple.Item1)
				{
					suggestedTags.Clear();
					foreach (var tag in tuple.Item2)
					{
						suggestedTags.Add(tag.Name);

					}
					RaisePropertyChanged("SuggestedTags");
				}
			};
			GlobalInfo.SavedSearchesLoadedEventHandler += (sender, e) =>
			{
				if (GlobalInfo.SavedSearches.Count > 0)
				{
					foreach (var search in GlobalInfo.SavedSearches)
					{
						savedSearches.Add(new SavedSearchViewModel(search, this));
					}
					RaisePropertyChanged("SavedSearches");
					RaisePropertyChanged("HaveSavedSearches");
				}
			};

			thumbnails.CollectionChanged += (sender, args) => RaisePropertyChanged("Thumbnails");
		}

		private int totalTagCount
		{
			get { return CurrentTags.Count + (checkedList.All(x => x) || checkedList.All(x => !x) ? 0 : 1) + (selectedOrderIndex > 0 ? 1 : 0); }
		}

		public bool IsSignedOutWithMoreThan2Tags => totalTagCount > 2 && Username == "" && APIKey == "";

		private bool safeChecked = true;
		private bool questionableChecked = false;
		private bool explicitChecked = false;

		private bool[] checkedList => new[] { safeChecked, questionableChecked, explicitChecked };

		public bool SafeChecked
		{
			get { return safeChecked; }
			set
			{
				safeChecked = value;
				GlobalInfo.ContentCheck[0] = value;

				appSettings["SafeChecked"] = value;

				RaisePropertyChanged();
				StartSearchExecute();
			}
		}

		public bool QuestionableChecked
		{
			get { return questionableChecked; }
			set
			{
				questionableChecked = value;
				GlobalInfo.ContentCheck[1] = value;

				appSettings["QuestionableChecked"] = value;

				RaisePropertyChanged();
				StartSearchExecute();
			}
		}

		public bool ExplicitChecked
		{
			get { return explicitChecked; }
			set
			{
				explicitChecked = value;
				GlobalInfo.ContentCheck[2] = value;
				appSettings["ExplicitChecked"] = value;

				RaisePropertyChanged();
				StartSearchExecute();
			}
		}

		private int startingPage = 1;
		public int PageNum
		{
			get { return startingPage; }
			set
			{
				startingPage = value;
				RaisePropertyChanged();
			}
		}

		private int perPage = 20;

		public int PerPage
		{
			get { return perPage; }
			set
			{
				if (value > perPage)
				{
					//Load enough images to fit the full page of the higher value. Then do the same as above.
					//Find pages that will fit, then find number of times load next page needs to be called
					var imagesNeeded = value - GlobalInfo.CurrentSearch.Count;
					var timesToCall = (int)Math.Ceiling((double)imagesNeeded / perPage);
					for (int i = 0; i < timesToCall; i++)
					{
						LoadNextPageExecute(0);
					}

					return;
				}



				double newPageNum = ((double)(BooruAPI.Page * perPage)) / value;
				int newPage = (int)Math.Floor(newPageNum);
				BooruAPI.Page = newPage == 0 ? 1 : newPage;
				int picsToRemove = (int)((newPageNum - newPage) * value);
				int picsToKeep = newPage * value;
				for (int i = Thumbnails.Count - 1; i > picsToKeep - 1; i--)
				{
					Thumbnails.RemoveAt(i);
					GlobalInfo.CurrentSearch.RemoveAt(i);
				}
				RaisePropertyChanged("Thumbnails");
				perPage = value;
				ApplicationData.Current.RoamingSettings.Values["PerPage"] = value;
				RaisePropertyChanged();


			}
		}

		private int imageSize = 250;

		public int ImageSize
		{
			get { return imageSize; }
			set
			{
				imageSize = value;
				ApplicationData.Current.LocalSettings.Values["ImageSize"] = value;
				RaisePropertyChanged();
			}
		}
		private ObservableCollection<string> suggestedTags = new ObservableCollection<string>();

		public ObservableCollection<string> SuggestedTags
		{
			get { return suggestedTags; }
			set
			{
				suggestedTags = value;
				RaisePropertyChanged();
			}
		}

		private int suggestedTagIndex = -1;

		public int SuggestedTagIndex
		{
			get { return suggestedTagIndex; }
			set
			{
				if (value < suggestedTags.Count && value >= 0)
				{
					suggestedTagIndex = value;
					CurrentTag = suggestedTags[suggestedTagIndex];
					RaisePropertyChanged();
				}
			}
		}

		private ObservableCollection<SavedSearchViewModel> savedSearches = new ObservableCollection<SavedSearchViewModel>();

		public ObservableCollection<SavedSearchViewModel> SavedSearches
		{
			get
			{
				if (savedSearches.Count == 0)
				{

					foreach (var search in GlobalInfo.SavedSearches)
					{
						savedSearches.Add(new SavedSearchViewModel(search, this));
					}
					RaisePropertyChanged("HaveSavedSearches");
				}
				return savedSearches;
			}
		}

		private PaginatedThumbnailList thumbnails = new PaginatedThumbnailList();

		public PaginatedThumbnailList Thumbnails
		{
			get
			{
				if (thumbnails.LoadMoreItemsDelegate == null)
				{
					thumbnails.load = LoadNextPageExecute;

				}
				return thumbnails;
			}
			set
			{
				if (thumbnails.Count != GlobalInfo.CurrentSearch.Count)
				{
					ResyncThumbnails();
				}
			}
		}

		public ObservableCollection<string> Prefixes => new ObservableCollection<string>(new[] { "none", "~", "-" });
		private string orderPrefix = "order:";
		public ObservableCollection<string> OrderOptions => new ObservableCollection<string>(new[]
		{
			"default", "id", "id_desc", "score", "score_asc", "favcount", "favcount_asc", "change", "change_asc", "comment",
			"comment_asc", "note", "note_asc", "mpixels", "mpixels_asc", "portrait", "landscape", "filesize",
			"filesize_asc", "rank", "random"
		});

		private int selectedPrefixIndex;

		public int SelectedPrefixIndex
		{
			get
			{
				return selectedPrefixIndex;
			}
			set
			{
				selectedPrefixIndex = value;
				RaisePropertyChanged();
			}
		}

		private int selectedOrderIndex;

		public int SelectedOrderIndex
		{
			get { return selectedOrderIndex; }
			set
			{
				selectedOrderIndex = value;
				RaisePropertyChanged();
				GlobalInfo.CurrentOrdering = SelectedOrderIndex > 0 ? OrderOptions[SelectedOrderIndex] : "";
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			}
		}

		public ObservableCollection<TagViewModel> CurrentTags => GlobalInfo.CurrentTags;

		private string currentTag = "";

		public string CurrentTag
		{
			get
			{
				return currentTag;
			}
			set
			{
				currentTag = value;
				RaisePropertyChanged();
				if (!string.IsNullOrEmpty(value))
				{
					BooruAPI.SearchTags(value.Replace(" ", "_"), 6);
				}
			}
		}

		private string username;
		public string Username
		{
			get { return BooruAPI.Username; }
			set
			{
				username = value;
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
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
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
				RaisePropertyChanged();
			}
		}

		private bool dontHaveImages = false;

		public bool DontHaveImages
		{
			get { return dontHaveImages; }
			set
			{
				dontHaveImages = value;
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
		public ListViewSelectionMode ImageSelectionMode
		{
			get { return isMultiSelectOn ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single; }
			set
			{
				isMultiSelectOn = value == ListViewSelectionMode.Multiple;
				RaisePropertyChanged("MultiSelectButtonIcon");
				RaisePropertyChanged();
			}
		}
		public SymbolIcon MultiSelectButtonIcon
		{
			get { return isMultiSelectOn ? new SymbolIcon(Symbol.Cancel) : new SymbolIcon(Symbol.SelectAll); }
			set
			{
				isMultiSelectOn = value == new SymbolIcon(Symbol.Cancel);
				RaisePropertyChanged();
			}
		}

		public Visibility IsFavButtonVisible => Username != "" && APIKey != "" ? Visibility.Visible : Visibility.Collapsed;

		public Visibility HaveSavedSearches => SavedSearches.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

		private int selectedSavedSearch = 0;

		public int SelectedSavedSearch
		{
			get { return selectedSavedSearch; }
			set
			{
				selectedSavedSearch = value;
				RaisePropertyChanged();
			}
		}



		public void DeleteSavedSearch(SavedSearchViewModel search)
		{
			SavedSearches.Remove(search);
			RaisePropertyChanged("HaveSavedSearches");
		}

		public void RemoveTagExec(TagViewModel tag)
		{
			CurrentTags.Remove(tag);
			RaisePropertyChanged("CurrentTags");
		}

		void AddTagExecute()
		{
			var prefix = "";
			if (selectedPrefixIndex > 0)
			{
				prefix = Prefixes[selectedPrefixIndex];
			}
			CurrentTags.Add(new TagViewModel(prefix + CurrentTag.Trim(), this));
			CurrentTag = "";
			suggestedTagIndex = -1;
			SuggestedTags.Clear();
			RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			RaisePropertyChanged("CurrentTag");
			RaisePropertyChanged("SuggestedTagIndex");

			RaisePropertyChanged("SuggestedTags");
			RaisePropertyChanged("CurrentTags");
		}

		bool AddTagCanExecute()
		{
			return true;
		}

		void SaveLoginDataExecute()
		{


			BooruAPI.SetLogin(username, apiKey);

			ApplicationData.Current.RoamingSettings.Values["Username"] = BooruAPI.Username;
			ApplicationData.Current.RoamingSettings.Values["APIKey"] = BooruAPI.APIKey;
			ApplicationData.Current.RoamingSettings.Values["PerPage"] = PerPage;
			ApplicationData.Current.RoamingSettings.Values["ImageSize"] = ImageSize;
			RaisePropertyChanged("APIKey");
			RaisePropertyChanged("IsFavButtonVisible");
		}

		bool SaveLoginDataCanExecute()
		{
			return true;
		}

		async void StartSearchExecute()
		{
			try
			{


				var tags = await PrepTags();
				GlobalInfo.CurrentSearch.Clear();
				ResyncThumbnails();
				GlobalInfo.CurrentSearchTags.Clear();

				GlobalInfo.CurrentSearchTags.AddRange(tags);
				var result = await BooruAPI.SearchPosts(tags, startingPage, PerPage, new[] { safeChecked, questionableChecked, explicitChecked });
				if (result.Item2 != null)
				{

					if (result.Item2.Count > 0 && result.Item3 == HttpStatusCode.Ok.ToString())
					{
						AddThumbnails(result.Item2);
						DontHaveImages = false;

					}
					else if (result.Item3 == HttpStatusCode.NoContent.ToString() || result.Item2.Count == 0)
					{
						DontHaveImages = true;
						NoImagesText = "No Images Found with those tags, try a different combination";
						RaisePropertyChanged("NoImagesText");
					}

				}
				else
				{
					DontHaveImages = true;
					NoImagesText = "Failed to grab images: " + result.Item3;
					RaisePropertyChanged("NoImagesText");
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				throw;
			}
		}

		void AddThumbnails(IEnumerable<ImageModel> tn)
		{

			foreach (var post in tn)
			{
				thumbnails.Add(new ThumbnailViewModel(post.Large_File_Url == "" ? post.File_Url : post.Large_File_Url, post.Large_File_Url == "" ? post.File_Url : post.Large_File_Url));
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
			thumbnails.Clear();
			foreach (var post in GlobalInfo.CurrentSearch)
			{
				thumbnails.Add(new ThumbnailViewModel(post.Large_File_Url == "" ? post.File_Url : post.Large_File_Url, post.Large_File_Url == "" ? post.File_Url : post.Large_File_Url));
			}

			RaisePropertyChanged("Thumbnails");
		}

		void SearchFavouritesExecute()
		{
			CurrentTags.Clear();
			CurrentTag = "ordfav:" + Username;
			RaisePropertyChanged("CurrentTags");
			AddTagExecute();
			StartSearchExecute();
		}



		async Task<IEnumerable<ImageModel>> LoadNextPageExecute(uint count)
		{
			BooruAPI.Page++;
			var tags = new List<string>(GlobalInfo.CurrentSearchTags);
			var result = await BooruAPI.SearchPosts(tags.ToArray(), BooruAPI.Page, PerPage, new[] { safeChecked, questionableChecked, explicitChecked }, false);
			if (result.Item3 == HttpStatusCode.Ok.ToString())
			{
				//
			}
			return result.Item2;
		}

		async void LoadNextPageE()
		{
			AddThumbnails(await LoadNextPageExecute(0));
		}


		void ChangeSelectionModeExecute()
		{
			//ImageSelectionMode = (ImageSelectionMode == SelectionMode.Multiple) ? SelectionMode.Single : SelectionMode.Multiple;
			isMultiSelectOn = !isMultiSelectOn;
			RaisePropertyChanged("ImageSelectionMode");
			RaisePropertyChanged("MultiSelectButtonIcon");
		}

		void SaveSearchExecute()
		{
			string[] tags = new string[CurrentTags.Count];
			if (CurrentTags.Count > 0)
			{
				for (int i = 0; i < tags.Length; i++)
				{
					tags[i] = CurrentTags[i].Tag;
				}
				var count = SavedSearches.Count;
				GlobalInfo.SavedSearches.Add(tags);
				if (count != 0)
				{
					SavedSearches.Add(new SavedSearchViewModel(tags, this));
				}
				RaisePropertyChanged("SavedSearches");
				RaisePropertyChanged("HaveSavedSearches");
			}
			GlobalInfo.SaveSearches();
		}

		public void StartSavedSearch(string[] tags)
		{
			CurrentTags.Clear();

			foreach (var tag in tags)
			{
				CurrentTags.Add(new TagViewModel(tag, this));
			}
			RaisePropertyChanged("CurrentTags");
			StartSearchExecute();
		}

		public ICommand AddTag => new RelayCommand(AddTagExecute);

		public ICommand SaveLoginData => new RelayCommand(SaveLoginDataExecute, SaveLoginDataCanExecute);
		public ICommand StartSearch => new RelayCommand(StartSearchExecute);
		public ICommand LoadNextPage => new RelayCommand(LoadNextPageE);
		public ICommand ChangeSelectionMode => new RelayCommand(ChangeSelectionModeExecute);
		public ICommand SearchFavourites => new RelayCommand(SearchFavouritesExecute);
		public ICommand SaveSearch => new RelayCommand(SaveSearchExecute);
		public ICommand SavedSearchSelected => new RelayCommand<SavedSearchViewModel>(SavedSearchSelectedExec);
		void SavedSearchSelectedExec(SavedSearchViewModel savedSearch)
		{
			Debug.WriteLine("Saved Search tapped in mainpageviewmodel");
			StartSavedSearch(savedSearch.Tags);
		}

		public ICommand DecrementPage => new RelayCommand(DecrementPageEx);
		void DecrementPageEx()
		{
			if (PageNum > 1)
			{
				PageNum -= 1;
			}
		}

		public ICommand IncrementPage => new RelayCommand(IncrementPageEx);
		void IncrementPageEx()
		{
			PageNum += 1;
		}


		public ICommand CheckBoxChanged => new RelayCommand<CheckBox>(CheckBoxChangedEx);
		void CheckBoxChangedEx(CheckBox checkBox)
		{

			int trueCount = 0;
			var checkedBools = new[] { safeChecked, questionableChecked, explicitChecked };
			foreach (var b in checkedBools)
			{
				if (b)
				{
					trueCount++;
				}
			}
			if (!checkBox.IsChecked.Value && trueCount <= 0)
			{
				checkBox.IsChecked = true;
			}

		}
	}
}
