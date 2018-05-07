﻿using System;
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
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Notifications;
using GalaSoft.MvvmLight;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Booru_Viewer.Models;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;

//TODO: Saved Searches (should be somewhat straightforward)
namespace Booru_Viewer.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		private IPropertySet _appSettings;

		public MainPageViewModel()
		{
			GetSaveFolder();

			_appSettings = ApplicationData.Current.RoamingSettings.Values;
			var savedUn = _appSettings["Username"] as string;
			var savedApiKey = _appSettings["APIKey"] as string;
			if (_appSettings["PerPage"] != null)
			{
				_perPage = (int)_appSettings["PerPage"];
				RaisePropertyChanged("PerPage");
			}
			if (_appSettings["SafeChecked"] != null)
			{
				_safeChecked = (bool)_appSettings["SafeChecked"];
			}
			if (_appSettings["QuestionableChecked"] != null)
			{
				_questionableChecked = (bool)_appSettings["QuestionableChecked"];
			}
			if (_appSettings["ExplicitChecked"] != null)
			{
				_explicitChecked = (bool)_appSettings["ExplicitChecked"];
			}
			if (_appSettings["UseLargerImagesForThumbnails"] != null)
			{
				_useLargerImagesForThumbnails = (bool)_appSettings["UseLargerImagesForThumbnails"];
			}


			GlobalInfo.ContentCheck[0] = _safeChecked;
			GlobalInfo.ContentCheck[1] = _questionableChecked;
			GlobalInfo.ContentCheck[2] = _explicitChecked;

			if (!string.IsNullOrEmpty(savedUn))
			{
				Debug.WriteLine("username not empty it's " + _appSettings["Username"] + ". APIKey is " + _appSettings["APIKey"]);

				_username = savedUn;
				if (!string.IsNullOrEmpty(savedApiKey))
				{

					ApiKey = savedApiKey;
					BooruAPI.SetLogin(savedUn, savedApiKey);
				}
				else
				{
					ApiKey = "";
					BooruAPI.SetLogin(savedUn, "");
				}

			}

			//StartSearchExecute();
			Debug.WriteLine("Count for saved Searches is " + SavedSearches.Count);
			BooruAPI.TagSearchCompletedHandler += (sender, tuple) =>
			{
				if (tuple.Item1 && _currentTag.Length > 0)
				{

					foreach (var tag in tuple.Item2)
					{
						_suggestedTags.Add(tag.Name);

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
						_savedSearches.Add(new SavedSearchViewModel(search, this));
					}
					RaisePropertyChanged("SavedSearches");
					RaisePropertyChanged("DontHaveSavedSearches");
				}
			};

			GlobalInfo.FavouriteTagsLoadedEventHandler += (sender, args) =>
			{
				if (GlobalInfo.FavouriteTags.Count > 0)
				{
					foreach (var tag in GlobalInfo.FavouriteTags)
					{
						_favouriteTags.First(x => x.Key == tag.category).Add(new TagViewModel(tag, this));

					}
					foreach (GroupInfoList t1 in _favouriteTags)
					{
						var tagList = t1;
						var result = from t in tagList
									 orderby t.Name
									 select t;
						tagList = new GroupInfoList(result.ToList());
					}
					RaisePropertyChanged("FavouriteTags");
					RaisePropertyChanged("DontHaveSavedSearches");
				}
			};

			
			//thumbnails.CollectionChanged += (sender, args) => RaisePropertyChanged("Thumbnails");
			RaisePropertyChanged("SelectedPrefixIndex");


			_thumbnails = new IncrementalLoadingCollection<PostSource, FullImageViewModel>(_perPage);
			foreach (var model in GlobalInfo.ImageViewModels)
			{
				_thumbnails.Add(model);
			}
			Thumbnails.OnStartLoading = () =>
			{
				IsLoading = true;
				NoImagesText = "Loading";
			};
			Thumbnails.OnEndLoading = ImageOnLoadFinish;
			Thumbnails.OnError = ImageLoadOnError;
			Thumbnails.RefreshAsync();

			BooruAPI.UserLookupEvent += (sender, args) =>
			{
				var tags = BooruAPI.UserModel?.blacklisted_tags.Split('\r');
				if (tags != null && tags.Length > 0)
				{
					foreach (var tag in tags)
					{
						if (tag.Trim() != "")
						{
							var name = tag.Replace("\n", "");
							ExcludedTags.Add(new TagViewModel(new Tag(name.Trim())));
						}
					}
				}
			};
			BooruAPI.GetUser();
		}

		private bool _useLargerImagesForThumbnails;

		public bool UseLargerImagesForThumbnails
		{
			get => _useLargerImagesForThumbnails;
			set
			{
				_useLargerImagesForThumbnails = value;
				_appSettings["UseLargerImagesForThumbnails"] = value;
				if (value)
				{
					foreach (var image in Thumbnails)
					{
						image.PreviewURL = image.FullImageURL;
						image.RaisePropertyChanged("PreviewURL");
					}
				}
				else
				{
					foreach (var image in Thumbnails)
					{
						image.PreviewURL = image.PreviewURL;
						image.RaisePropertyChanged("PreviewURL");
					}
				}
				RaisePropertyChanged();
			}
		}

		public string VersionNumber
		{

			get
			{
				var packageVersion = Package.Current.Id.Version;
				return packageVersion.Major + "." + packageVersion.Minor + "." + packageVersion.Build + "." +
					   packageVersion.Revision;
			}
		}

		public int TotalTagCount
		{
			get
			{
				return CurrentTags.Count + (CheckedList.All(x => x) || CheckedList.All(x => !x) ? 0 : 1) +
					   (_selectedOrderIndex > 1 ? 1 : 0);
			}
		}

		public bool IsSignedOutWithMoreThan2Tags => TotalTagCount > 2 && Username == "" && ApiKey == "";

		private bool _safeChecked = true;
		private bool _questionableChecked = false;
		private bool _explicitChecked = false;

		private bool[] CheckedList => new[] { _safeChecked, _questionableChecked, _explicitChecked };

		public bool SafeChecked
		{
			get
			{
				if (_appSettings["SafeChecked"] != null)
				{
					_safeChecked = (bool)_appSettings["SafeChecked"];
				}
				return _safeChecked;
			}
			set
			{
				_safeChecked = value;
				GlobalInfo.ContentCheck[0] = value;

				_appSettings["SafeChecked"] = value;

				RaisePropertyChanged();
				StartSearchExecute();
			}
		}

		public bool QuestionableChecked
		{
			get
			{
				if (_appSettings["QuestionableChecked"] != null)
				{
					_questionableChecked = (bool)_appSettings["QuestionableChecked"];
				}
				return _questionableChecked;
			}
			set
			{
				_questionableChecked = value;
				GlobalInfo.ContentCheck[1] = value;

				_appSettings["QuestionableChecked"] = value;

				RaisePropertyChanged();
				StartSearchExecute();
			}
		}

		public bool ExplicitChecked
		{
			get
			{
				if (_appSettings["ExplicitChecked"] != null)
				{
					_explicitChecked = (bool)_appSettings["ExplicitChecked"];
				}
				return _explicitChecked;
			}
			set
			{
				_explicitChecked = value;
				GlobalInfo.ContentCheck[2] = value;
				_appSettings["ExplicitChecked"] = value;

				RaisePropertyChanged();
				StartSearchExecute();
			}
		}

		private int _startingPage = 1;

		public int PageNum
		{
			get => _startingPage;
			set
			{
				_startingPage = value;
				RaisePropertyChanged();
			}
		}

		private int _perPage = 20;

		public int PerPage
		{
			get
			{
				if (_appSettings["PerPage"] != null)
				{
					_perPage = (int)_appSettings["PerPage"];
				}
				return _perPage;
			}
			set
			{
				ApplicationData.Current.RoamingSettings.Values["PerPage"] = value;
				RaisePropertyChanged();


			}
		}

		private int _imageSize = 250;

		public int ImageSize
		{
			get => _imageSize;
			set
			{
				_imageSize = value;
				ApplicationData.Current.LocalSettings.Values["ImageSize"] = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<string> _suggestedTags = new ObservableCollection<string>();

		public ObservableCollection<string> SuggestedTags
		{
			get => _suggestedTags;
			set
			{
				_suggestedTags = value;
				RaisePropertyChanged();
			}
		}

		private int _suggestedTagIndex = -1;

		public int SuggestedTagIndex
		{
			get => _suggestedTagIndex;
			set
			{
				if (value < _suggestedTags.Count && value >= 0)
				{
					_suggestedTagIndex = value;
					CurrentTag = _suggestedTags[_suggestedTagIndex];
					RaisePropertyChanged();
				}
			}
		}

		public bool TagSuggestionChosen { get; set; } = false;
		public bool ExcludedTagSuggestionChosen { get; set; } = false;
		private bool _isLoading = false;

		public bool IsLoading
		{
			get => _isLoading;
			set
			{
				_isLoading = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<SavedSearchViewModel> _savedSearches = new ObservableCollection<SavedSearchViewModel>();

		public ObservableCollection<SavedSearchViewModel> SavedSearches
		{
			get
			{
				if (_savedSearches.Count == 0)
				{

					foreach (var search in GlobalInfo.SavedSearches)
					{
						_savedSearches.Add(new SavedSearchViewModel(search, this));
					}
					RaisePropertyChanged("DontHaveSavedSearches");
				}
				return _savedSearches;
			}
		}

		private ObservableCollection<TagViewModel> _excludedTags = null;

		public ObservableCollection<TagViewModel> ExcludedTags
		{
			get
			{
				if (_excludedTags == null)
				{
					_excludedTags = new ObservableCollection<TagViewModel>();

				}

				return _excludedTags;
			}
		}

		private ObservableCollection<GroupInfoList> _favouriteTags = new ObservableCollection<GroupInfoList>();

		public ObservableCollection<GroupInfoList> FavouriteTags
		{
			get
			{
				_favouriteTags.Clear();
				_favouriteTags.Add(new GroupInfoList { Key = TagType.General });
				_favouriteTags.Add(new GroupInfoList { Key = TagType.Artist });
				_favouriteTags.Add(new GroupInfoList { Key = TagType.Character });
				_favouriteTags.Add(new GroupInfoList { Key = TagType.Copyright });
				_favouriteTags.Add(new GroupInfoList { Key = TagType.Unknown });

				foreach (var search in GlobalInfo.FavouriteTags)
				{
					_favouriteTags.First(x => x.Key == search.category).Add(new TagViewModel(search, this));
				}
				foreach (var tagList in _favouriteTags)
				{
					tagList.Sort();
				}
				RaisePropertyChanged("DontHaveSavedSearches");
				return _favouriteTags;
			}
			set => _favouriteTags = value;
		}

		private IncrementalLoadingCollection<PostSource, FullImageViewModel> _thumbnails;

		public IncrementalLoadingCollection<PostSource, FullImageViewModel> Thumbnails
		{
			get
			{
				if (_thumbnails.Count == 0)
				{
					DontHaveImages = true;
				}
				return _thumbnails;
			}
		}

		public ObservableCollection<string> Prefixes => new ObservableCollection<string>(new[] { "none", "~", "-" });
		private string _orderPrefix = "order:";

		public ObservableCollection<string> OrderOptions => new ObservableCollection<string>(new[]
		{
			"default", "id", "id_desc", "score", "score_asc", "favcount", "favcount_asc", "change", "change_asc", "comment",
			"comment_asc", "note", "note_asc", "mpixels", "mpixels_asc", "portrait", "landscape", "filesize",
			"filesize_asc", "rank", "random"
		});

		private int _selectedPrefixIndex;

		public int SelectedPrefixIndex
		{
			get => _selectedPrefixIndex;
			set
			{
				_selectedPrefixIndex = value;
				RaisePropertyChanged();
			}
		}

		private int _selectedOrderIndex;

		public int SelectedOrderIndex
		{
			get => _selectedOrderIndex;
			set
			{
				_selectedOrderIndex = value;
				RaisePropertyChanged();
				GlobalInfo.CurrentOrdering = SelectedOrderIndex > 0 ? OrderOptions[SelectedOrderIndex] : "";
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
				RaisePropertyChanged("TotalTagCount");
			}
		}

		public ObservableCollection<TagViewModel> CurrentTags => GlobalInfo.CurrentTags;



		private string _currentTag = "";
		private DateTime _start;
		private TimeSpan _timeSinceChange;

		public string CurrentTag
		{
			get => _currentTag;
			set
			{
				if (TagSuggestionChosen)
				{
					TagSuggestionChosen = false;
					_suggestedTags.Clear();
					_currentTag = value;
					AddTagExecute();
					_currentTag = "";
				}
				_timeSinceChange = DateTime.Now - _start;
				RaisePropertyChanged();
				if (!string.IsNullOrEmpty(value) && _timeSinceChange.TotalSeconds > 0.10f)
				{
					_start = DateTime.Now;
					_timeSinceChange = TimeSpan.Zero;
					SearchForCurrentTag(value);
				}
				if (string.IsNullOrEmpty(value))
				{
					SuggestedTags.Clear();
				}
				_currentTag = value;
			}
		}

		async void SearchForCurrentTag(string val)
		{
			SuggestedTags.Clear();
			await BooruAPI.SearchTags(val.Replace(" ", "_"), 1, true);
			await BooruAPI.SearchTags(val.Replace(" ", "_"), 6);
			SuggestedTags = new ObservableCollection<string>(SuggestedTags.Distinct());
		}

		private string _username;

		public string Username
		{
			get => BooruAPI.Username;
			set
			{
				_username = value;
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
				RaisePropertyChanged();
			}
		}

		private string _apiKey;

		public string ApiKey
		{
			get => BooruAPI.APIKey;
			set
			{

				_apiKey = value;
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
				RaisePropertyChanged();
			}
		}

		public string SaveFolder => ImageSaver.ImageFolder.Path;

		public async void GetSaveFolder()
		{

			await ImageSaver.GetFolderAsync();

		}

		private bool _dontHaveImages = false;

		public bool DontHaveImages
		{
			get => Thumbnails.Count == 0;
			private set
			{
				_dontHaveImages = value;
				RaisePropertyChanged();
			}
		}

		private string _noImagesText = "Press the search key to get started";

		public string NoImagesText
		{
			get => _noImagesText;
			set
			{
				_noImagesText = value;
				RaisePropertyChanged();
			}
		}


		private bool _isMultiSelectOn = false;

		public ListViewSelectionMode ImageSelectionMode
		{
			get => _isMultiSelectOn ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
			set
			{
				_isMultiSelectOn = value == ListViewSelectionMode.Multiple;
				RaisePropertyChanged("MultiSelectButtonIcon");
				RaisePropertyChanged();
			}
		}

		public ListViewSelectionMode GridViewSelectMode { get; set; }

		public SymbolIcon MultiSelectButtonIcon
		{
			get => _isMultiSelectOn ? new SymbolIcon(Symbol.Cancel) : new SymbolIcon(Symbol.SelectAll);
			set
			{
				_isMultiSelectOn = value == new SymbolIcon(Symbol.Cancel);
				RaisePropertyChanged();
			}
		}

		public Visibility IsFavButtonVisible => Username != "" && ApiKey != "" ? Visibility.Visible : Visibility.Collapsed;

		public bool DontHaveSavedSearches => FavouriteTags.All(x => x.Count == 0);

		private int _selectedSavedSearch = 0;

		public int SelectedSavedSearch
		{
			get => _selectedSavedSearch;
			set
			{
				_selectedSavedSearch = value;
				RaisePropertyChanged();
			}


		}




		public void DeleteSavedSearch(SavedSearchViewModel search)
		{
			try
			{
				SavedSearches.Remove(search);
				RaisePropertyChanged("DontHaveSavedSearches");
				GlobalInfo.SaveSearches(_savedSearches.ToList());
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		}

		public void RemoveTagExec(TagViewModel tag)
		{
			CurrentTags.Remove(tag);
			RaisePropertyChanged("CurrentTags");

		}

		void AddTagExecute()
		{
			if (CurrentTags.Any(x => x.Name.TrimStart('-', '~') == CurrentTag))
			{
				return;
			}

			var prefix = "";
			if (_selectedPrefixIndex > 0)
			{
				prefix = Prefixes[_selectedPrefixIndex];
			}
			CurrentTags.Add(new TagViewModel(new Tag(prefix + CurrentTag.Trim().ToLower()), this));
			CurrentTag = "";
			_suggestedTagIndex = -1;
			SuggestedTags.Clear();
			RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			RaisePropertyChanged("CurrentTag");
			RaisePropertyChanged("SuggestedTagIndex");

			RaisePropertyChanged("SuggestedTags");
			RaisePropertyChanged("CurrentTags");
			RaisePropertyChanged("TotalTagCount");
		}

		bool AddTagCanExecute()
		{
			return true;
		}

		void SaveLoginDataExecute()
		{


			BooruAPI.SetLogin(_username, _apiKey);

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

		void ImageLoadOnError(Exception e)
		{
			DontHaveImages = true;
			NoImagesText = "Failed to grab images: " + e.Message;
		}

		void ImageOnLoadFinish()
		{
			Debug.WriteLine("ImagesFinished Loading");
			if (_thumbnails.Count == 0 && !NoImagesText.Contains("Failed"))
			{
				NoImagesText = "No Images found with those tags, try a different search";
				DontHaveImages = true;
			}
			else
			{
				DontHaveImages = false;
				IsLoading = false;
			}
			RaisePropertyChanged("Thumbnails");
			GlobalInfo.ImageViewModels = Thumbnails;
			RaisePropertyChanged("TotalPageNumber");
		}

		public async void StartSearchExecute()
		{
			try
			{

				SelectedPrefixIndex = 0;
				var tags = await PrepTags();
				GlobalInfo.CurrentSearch.Clear();

				GlobalInfo.CurrentSearchTags.Clear();

				GlobalInfo.CurrentSearchTags.AddRange(tags);
				BooruAPI.Page = 1;
				await Thumbnails.RefreshAsync();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				throw;
			}
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
					if (!string.IsNullOrEmpty(tag.Name))
					{
						tags[i] = tag.Name.Replace(" ", "_") + " ";
					}
					i++;
				}
			});



			return tags;
		}

		void SearchFavouritesExecute()
		{

			CurrentTags.Clear();
			CurrentTag = "ordfav:" + Username;
			RaisePropertyChanged("CurrentTags");
			RaisePropertyChanged("TotalTagCount");
			AddTagExecute();
			StartSearchExecute();
		}



		async Task LoadNextPageExecute(uint count)
		{
			await Thumbnails.LoadMoreItemsAsync(Convert.ToUInt32(PerPage));
		}

		async void LoadNextPageE()
		{
			await LoadNextPageExecute(0);
		}


		void ChangeSelectionModeExecute()
		{
			//ImageSelectionMode = (ImageSelectionMode == SelectionMode.Multiple) ? SelectionMode.Single : SelectionMode.Multiple;
			_isMultiSelectOn = !_isMultiSelectOn;
			RaisePropertyChanged("ImageSelectionMode");
			RaisePropertyChanged("MultiSelectButtonIcon");
		}

		async void SaveSearchExecute()
		{
			string[] tags = new string[CurrentTags.Count];
			if (CurrentTags.Count > 0)
			{
				for (int i = 0; i < tags.Length; i++)
				{
					tags[i] = CurrentTags[i].Name;
				}
				var count = SavedSearches.Count;
				GlobalInfo.SavedSearches.Add(tags);
				if (count != 0)
				{
					SavedSearches.Add(new SavedSearchViewModel(tags, this));
				}
				RaisePropertyChanged("SavedSearches");
				RaisePropertyChanged("DontHaveSavedSearches");
			}
			await GlobalInfo.SaveSearches(_savedSearches.ToList());
		}

		public void StartSavedSearch(string[] tags)
		{
			CurrentTags.Clear();

			foreach (var tag in tags)
			{
				CurrentTags.Add(new TagViewModel(new Tag(tag), this));
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
		public ICommand ClearSearch => new RelayCommand(ClearSearchEx);

		
		void ClearSearchEx()
		{
			CurrentTags.Clear();
			RaisePropertyChanged("CurrentTags");
			RaisePropertyChanged("TotalTagCount");
		}
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
			var checkedBools = new[] { _safeChecked, _questionableChecked, _explicitChecked };
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
			RaisePropertyChanged("TotalTagCount");

		}

		public ICommand ChooseSaveFolder => new RelayCommand<Button>(ChooseSaveFolderExec);
		async void ChooseSaveFolderExec(Button b)
		{
			await b.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{

				FolderPicker picker = new FolderPicker
				{
					SuggestedStartLocation = PickerLocationId.PicturesLibrary,
					FileTypeFilter = { "*" }
				};


				var folder = await picker.PickSingleFolderAsync();

				if (folder != null)
				{

					ImageSaver.ImageFolder = folder;
					var token = StorageApplicationPermissions.FutureAccessList.Add(ImageSaver.ImageFolder);
					ApplicationData.Current.LocalSettings.Values["SaveFolderToken"] = token;
					RaisePropertyChanged("SaveFolder");
				}
			});
		}

		public ICommand Backup => new RelayCommand(BackupEx);

		async void BackupEx()
		{
			var picker = new FolderPicker
			{
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
				CommitButtonText = "Select Folder",
				ViewMode = PickerViewMode.List,
				FileTypeFilter = { "*" }
			};
			var result = await picker.PickSingleFolderAsync();

			if (result != null)
			{
				await GlobalInfo.SaveSearches(SavedSearches.ToList(), result);
				await GlobalInfo.SaveFavouriteTags(result);
				SettingsData settings = new SettingsData()
				{
					PerPage = PerPage,
					Username = BooruAPI.Username,
					APIKey = BooruAPI.APIKey,
					contentChecks = new[] { _safeChecked, QuestionableChecked, ExplicitChecked }
				};
				var saveFolder = result;

				if (result.DisplayName != "Booru-Viewer")
				{
					saveFolder = await result.GetFolderAsync("Booru-Viewer");
				}
				var obj = await saveFolder.TryGetItemAsync("Settings.json");
				StorageFile settingsFile;
				if (obj != null)
				{
					settingsFile = await saveFolder.GetFileAsync("Settings.json");
				}
				else
				{
					settingsFile = await saveFolder.CreateFileAsync("Settings.json");
				}
				await FileIO.WriteTextAsync(settingsFile, JsonConvert.SerializeObject(settings));
			}
		}

		public ICommand Restore => new RelayCommand(RestoreEx);
		async void RestoreEx()
		{
			var picker = new FolderPicker
			{
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
				FileTypeFilter = { "*" }
			};
			var result = await picker.PickSingleFolderAsync();
			if (result != null)
			{
				await GlobalInfo.LoadSavedSearches(result);
				await GlobalInfo.SaveSearches(SavedSearches.ToList());
				await GlobalInfo.LoadFavouriteTags(result);
				await GlobalInfo.SaveFavouriteTags();
				var saveFolder = result;
				if (result.Name != "Booru-Viewer")
				{
					saveFolder = await result.GetFolderAsync("Booru-Viewer");
				}
				var obj = await saveFolder.TryGetItemAsync("Settings.json");
				StorageFile settingsFile;
				if (obj != null)
				{
					settingsFile = await saveFolder.GetFileAsync("Settings.json");
					var json = await FileIO.ReadTextAsync(settingsFile);
					SettingsData data = JsonConvert.DeserializeObject<SettingsData>(json);
					PerPage = data.PerPage;
					BooruAPI.SetLogin(data.Username, data.APIKey);
					SafeChecked = data.contentChecks[0];
					QuestionableChecked = data.contentChecks[1];
					ExplicitChecked = data.contentChecks[2];

				}
			}
		}

		//public ICommand PerPageChanged => new RelayCommand<int>(PerPageChangedEx);
		public async void PerPageChangedEx(object sender, PointerRoutedEventArgs e)
		{
			if (_perPage != (int)(sender as Slider).Value)
			{
				Debug.WriteLine("Per Page Changed, old value is {0}, new value is {1}", _perPage, (sender as Slider).Value);
				Debug.WriteLine("PerPage Changed");
				BooruAPI.Page = 1;
				_perPage = (int)(sender as Slider).Value;
				ApplicationData.Current.RoamingSettings.Values["PerPage"] = _perPage;
				_thumbnails =
					new IncrementalLoadingCollection<PostSource, FullImageViewModel>(_perPage, null, ImageOnLoadFinish,
						ImageLoadOnError);

				await Thumbnails.RefreshAsync();
				RaisePropertyChanged("Thumbnails");
			}
		}

		public FullImageViewModel ImageContextOpened { get; set; }

		public ICommand SaveImage => new RelayCommand<bool>(SaveImageExec);

		async void SaveImageExec(bool showNotification = true)
		{
			Debug.WriteLine("Started saving image");
			IsLoading = true;
			var saveImageFailureReason = await ImageSaver.SaveImage(ImageContextOpened.LargeImageURL);
			IsLoading = false;
			Debug.WriteLine("Finished saving image: " + saveImageFailureReason);
			ToastContent content = new ToastContent()
			{
				Visual = new ToastVisual()
				{
					BindingGeneric = new ToastBindingGeneric()
					{
						Children =
						{
							new AdaptiveImage()
							{
								Source = ImageContextOpened.FullImageURL
							},
							new AdaptiveText()
							{
								Text = saveImageFailureReason.Item2
							}
						}
					}
				}
			};
			Windows.Data.Xml.Dom.XmlDocument doc = content.GetXml();
			ToastNotification not = new ToastNotification(doc);
			ToastNotificationManager.ConfigureNotificationMirroring(NotificationMirroring.Disabled);
			ToastNotificationManager.CreateToastNotifier().Show(not);
		}

		public bool IsSavingImages { get; set; }

		private int _imageSaveCount = 60;

		public int ImageSaveCount
		{
			get => _imageSaveCount;
			set { _imageSaveCount = value; RaisePropertyChanged(); }
		}

		private int _duplicateSaveCount = 0;

		public int DuplicateSaveCount
		{
			get => _duplicateSaveCount;
			set { _duplicateSaveCount = value;RaisePropertyChanged(); }
		}

		private int _currentImageSaveIndex = 0;

		public int CurrentImageSaveIndex
		{
			get => _currentImageSaveIndex;
			set
			{
				_currentImageSaveIndex = value;
				RaisePropertyChanged();
			}
		}

		public ICommand SaveSelectedImages => new RelayCommand<IList<object>>(SaveSelectedImagesEx);

		async void SaveSelectedImagesEx(IList<object> images)
		{
			IsSavingImages = true;
			RaisePropertyChanged("IsSavingImages");
			var imageList = images.ToList();
			GridViewSelectMode = ListViewSelectionMode.None;
			RaisePropertyChanged("GridViewSelectionMode");

			ImageSaver.ImageFinishedSavingEvent += ImageFinishedSave;

			await ImageSaver.SaveImagesFromList(imageList.Select(x => (x as FullImageViewModel).FullImageURL).ToList());
			IsSavingImages = false;
			RaisePropertyChanged("IsSavingImages");
		}


		void ImageFinishedSave(int index, int count, bool lastImage, int dupeCount)
		{
			CurrentImageSaveIndex = index;
			ImageSaveCount = count;
			IsSavingImages = !lastImage;
			DuplicateSaveCount = dupeCount;
			if (lastImage)
			{
				ImageSaver.ImageFinishedSavingEvent -= ImageFinishedSave;
			}
			RaisePropertyChanged("IsSavingImages");
		}
	}


}



