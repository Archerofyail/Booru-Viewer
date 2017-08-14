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
using System.Globalization;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Notifications;
using GalaSoft.MvvmLight;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Booru_Viewer.Models;
using Dropbox.Api;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;

//TODO: Saved Searches (should be somewhat straightforward)
namespace Booru_Viewer.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		private IPropertySet appSettings;
		public MainPageViewModel()
		{
			GetSaveFolder();
			appSettings = ApplicationData.Current.RoamingSettings.Values;
			var savedUN = appSettings["Username"] as string;
			var savedAPIKey = appSettings["APIKey"] as string;
			if (appSettings["PerPage"] != null)
			{
				perPage = (int)appSettings["PerPage"];
				RaisePropertyChanged("PerPage");
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

			//StartSearchExecute();
			Debug.WriteLine("Count for saved Searches is " + SavedSearches.Count);
			BooruAPI.TagSearchCompletedHandler += (sender, tuple) =>
			{
				if (tuple.Item1)
				{

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

			GlobalInfo.FavouriteTagsLoadedEventHandler += (sender, args) =>
			{
				if (GlobalInfo.FavouriteTags.Count > 0)
				{
					foreach (var tag in GlobalInfo.FavouriteTags)
					{
						favouriteTags.Add(new TagViewModel(tag, this));
					}
					RaisePropertyChanged("FavouriteTags");
					RaisePropertyChanged("HaveSavedSearches");
				}
			};

			//thumbnails.CollectionChanged += (sender, args) => RaisePropertyChanged("Thumbnails");
			RaisePropertyChanged("SelectedPrefixIndex");


			thumbnails = new IncrementalLoadingCollection<PostSource, FullImageViewModel>(perPage);
			foreach (var model in GlobalInfo.ImageViewModels)
			{
				thumbnails.Add(model);
			}
			Thumbnails.OnStartLoading = () =>
			{
				IsLoading = true;
				NoImagesText = "Loading";
			};
			Thumbnails.OnEndLoading = ImageOnLoadFinish;
			Thumbnails.OnError = ImageLoadOnError;
			Thumbnails.RefreshAsync();
			BooruAPI.GetUser();

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
			get
			{
				if (appSettings["SafeChecked"] != null)
				{
					safeChecked = (bool)appSettings["SafeChecked"];
				}
				return safeChecked;
			}
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
			get
			{
				if (appSettings["QuestionableChecked"] != null)
				{
					questionableChecked = (bool)appSettings["QuestionableChecked"];
				}
				return questionableChecked;
			}
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
			get
			{
				if (appSettings["ExplicitChecked"] != null)
				{
					explicitChecked = (bool)appSettings["ExplicitChecked"];
				}
				return explicitChecked;
			}
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
			get => startingPage; set
			{
				startingPage = value;
				RaisePropertyChanged();
			}
		}

		private int perPage = 20;

		public int PerPage
		{
			get
			{
				if (appSettings["PerPage"] != null)
				{
					perPage = (int)appSettings["PerPage"];
				}
				return perPage;
			}
			set
			{
				ApplicationData.Current.RoamingSettings.Values["PerPage"] = value;
				RaisePropertyChanged();


			}
		}

		private int imageSize = 250;

		public int ImageSize
		{
			get => imageSize; set
			{
				imageSize = value;
				ApplicationData.Current.LocalSettings.Values["ImageSize"] = value;
				RaisePropertyChanged();
			}
		}
		private ObservableCollection<string> suggestedTags = new ObservableCollection<string>();

		public ObservableCollection<string> SuggestedTags
		{
			get => suggestedTags; set
			{
				suggestedTags = value;
				RaisePropertyChanged();
			}
		}

		private int suggestedTagIndex = -1;

		public int SuggestedTagIndex
		{
			get => suggestedTagIndex; set
			{
				if (value < suggestedTags.Count && value >= 0)
				{
					suggestedTagIndex = value;
					CurrentTag = suggestedTags[suggestedTagIndex];
					RaisePropertyChanged();
				}
			}
		}

		private bool isLoading = false;

		public bool IsLoading
		{
			get => isLoading;
			set
			{
				isLoading = value;
				RaisePropertyChanged();
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

		private ObservableCollection<TagViewModel> favouriteTags = new ObservableCollection<TagViewModel>();

		public ObservableCollection<TagViewModel> FavouriteTags
		{
			get
			{
				favouriteTags.Clear();

				foreach (var search in GlobalInfo.FavouriteTags)
				{
					favouriteTags.Add(new TagViewModel(search, this));
				}
				RaisePropertyChanged("HaveSavedSearches");


				return favouriteTags;
			}
			set => favouriteTags = value;
		}

		private IncrementalLoadingCollection<PostSource, FullImageViewModel> thumbnails;

		public IncrementalLoadingCollection<PostSource, FullImageViewModel> Thumbnails
		{
			get => thumbnails;
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
			get => selectedPrefixIndex;
			set
			{
				selectedPrefixIndex = value;
				RaisePropertyChanged();
			}
		}

		private int selectedOrderIndex;

		public int SelectedOrderIndex
		{
			get => selectedOrderIndex; set
			{
				selectedOrderIndex = value;
				RaisePropertyChanged();
				GlobalInfo.CurrentOrdering = SelectedOrderIndex > 0 ? OrderOptions[SelectedOrderIndex] : "";
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			}
		}

		public ObservableCollection<TagViewModel> CurrentTags => GlobalInfo.CurrentTags;

		private string currentTag = "";
		private DateTime start;
		private TimeSpan timeSinceChange;
		public string CurrentTag
		{
			get => currentTag;
			set
			{
				timeSinceChange = DateTime.Now - start;
				RaisePropertyChanged();
				if (!string.IsNullOrEmpty(value) && timeSinceChange.TotalSeconds > 0.10f)
				{
					start = DateTime.Now;
					timeSinceChange = TimeSpan.Zero;
					SearchForCurrentTag(value);
				}
				if (string.IsNullOrEmpty(value))
				{
					SuggestedTags.Clear();
				}
				currentTag = value;
			}
		}

		async void SearchForCurrentTag(string val)
		{
			SuggestedTags.Clear();
			await BooruAPI.SearchTags(val.Replace(" ", "_"), 1, true);
			await BooruAPI.SearchTags(val.Replace(" ", "_"), 6);
			SuggestedTags = new ObservableCollection<string>(SuggestedTags.Distinct());
		}

		private string username;
		public string Username
		{
			get => BooruAPI.Username; set
			{
				username = value;
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
				RaisePropertyChanged();
			}
		}

		private string apiKey;
		public string APIKey
		{
			get => BooruAPI.APIKey; set
			{

				apiKey = value;
				RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
				RaisePropertyChanged();
			}
		}

		public string SaveFolder => ImageSaver.ImageFolder.Path;

		public async void GetSaveFolder()
		{

			await ImageSaver.GetFolderAsync();

		}

		private bool dontHaveImages = false;

		public bool DontHaveImages
		{
			get => dontHaveImages;
			private set
			{
				dontHaveImages = value;
				RaisePropertyChanged();
			}
		}

		private string noImagesText = "Press the search key to get started";
		public string NoImagesText
		{
			get => noImagesText; set
			{
				noImagesText = value;
				RaisePropertyChanged();
			}
		}


		private bool isMultiSelectOn = false;
		public ListViewSelectionMode ImageSelectionMode
		{
			get => isMultiSelectOn ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single; set
			{
				isMultiSelectOn = value == ListViewSelectionMode.Multiple;
				RaisePropertyChanged("MultiSelectButtonIcon");
				RaisePropertyChanged();
			}
		}
		public SymbolIcon MultiSelectButtonIcon
		{
			get => isMultiSelectOn ? new SymbolIcon(Symbol.Cancel) : new SymbolIcon(Symbol.SelectAll); set
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
			get => selectedSavedSearch; set
			{
				selectedSavedSearch = value;
				RaisePropertyChanged();
			}
		}



		public void DeleteSavedSearch(SavedSearchViewModel search)
		{
			try
			{
				SavedSearches.Remove(search);
				RaisePropertyChanged("HaveSavedSearches");
				GlobalInfo.SaveSearches(savedSearches.ToList());
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
			var prefix = "";
			if (selectedPrefixIndex > 0)
			{
				prefix = Prefixes[selectedPrefixIndex];
			}
			CurrentTags.Add(new TagViewModel(prefix + CurrentTag.Trim().ToLower(), this));
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

		void ImageLoadOnError(Exception e)
		{
			DontHaveImages = true;
			NoImagesText = "Failed to grab images: " + e.Message;
		}

		void ImageOnLoadFinish()
		{
			Debug.WriteLine("ImagesFinished Loading");
			if (thumbnails.Count == 0)
			{
				NoImagesText = "No Images found with those tags, try a different search";
				DontHaveImages = true;
			}
			else
			{
				NoImagesText = "";
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
					if (!string.IsNullOrEmpty(tag.Tag))
					{
						tags[i] = tag.Tag.Replace(" ", "_") + " ";
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
			AddTagExecute();
			StartSearchExecute();
		}



		async void LoadNextPageExecute(uint count)
		{
			await Thumbnails.LoadMoreItemsAsync(Convert.ToUInt32(PerPage));
		}

		async void LoadNextPageE()
		{
			LoadNextPageExecute(0);
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
			GlobalInfo.SaveSearches(savedSearches.ToList());
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
		public ICommand ClearSearch => new RelayCommand(ClearSearchEx);

		void ClearSearchEx()
		{
			CurrentTags.Clear();
			RaisePropertyChanged("CurrentTags");
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

		public ICommand ChooseSaveFolder => new RelayCommand<Button>(ChooseSaveFolderExec);
		async void ChooseSaveFolderExec(Button b)
		{
			await b.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
			{
				FolderPicker picker = new FolderPicker { CommitButtonText = "Select" };

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

		public ICommand SelectDropboxFolder => new RelayCommand(SelectDropboxFolderExec);
		async void SelectDropboxFolderExec()
		{
			StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///DropboxInfo.txt"));
			string[] dbInfo = (await FileIO.ReadTextAsync(file)).Split('\n');
			var dBclient = new DropboxAppClient(dbInfo[0], dbInfo[1]);
			var authURI = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, dbInfo[0], "https://localhost/authorize");

		}

		public ICommand Backup => new RelayCommand(BackupEx);

		async void BackupEx()
		{
			var picker = new FolderPicker();
			picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			var result = await picker.PickSingleFolderAsync();
			if (result != null)
			{
				await GlobalInfo.SaveSearches(SavedSearches.ToList(), result);
				SettingsData settings = new SettingsData()
				{
					PerPage = PerPage,
					Username = BooruAPI.Username,
					APIKey = BooruAPI.APIKey,
					contentChecks = new[] { safeChecked, QuestionableChecked, ExplicitChecked }
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
			var picker = new FolderPicker();
			picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			var result = await picker.PickSingleFolderAsync();
			if (result != null)
			{
				await GlobalInfo.LoadSavedSearches(result);
				await GlobalInfo.SaveSearches(SavedSearches.ToList());
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
			if (perPage != (int)(sender as Slider).Value)
			{
				Debug.WriteLine("Per Page Changed, old value is {0}, new value is {1}", perPage, (sender as Slider).Value);
				Debug.WriteLine("PerPage Changed");
				BooruAPI.Page = 1;
				perPage = (int)(sender as Slider).Value;
				ApplicationData.Current.RoamingSettings.Values["PerPage"] = perPage;
				thumbnails =
					new IncrementalLoadingCollection<PostSource, FullImageViewModel>(perPage, null, ImageOnLoadFinish,
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
								Text = saveImageFailureReason
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

		public ICommand StartImageSave => new RelayCommand<int>(StartImageSaveEx);
		async void StartImageSaveEx(int pageTo)
		{

			int imageCount = (SelectedPageToSave + 1) * PerPage;
			for (int i = 0; i < imageCount; i++)
			{
				if (i > GlobalInfo.CurrentSearch.Count - 1)
				{
					break;
				}
				await ImageSaver.SaveImage(string.IsNullOrEmpty(GlobalInfo.CurrentSearch[i].Large_File_Url)
					? GlobalInfo.CurrentSearch[i].File_Url
					: GlobalInfo.CurrentSearch[i].Large_File_Url);
			}
		}

		public int SelectedPageToSave { get; set; } = 0;

		public int[] TotalPageNumber
		{
			get
			{
				int[] pages = new int[BooruAPI.Page];
				for (int i = 0; i < pages.Length; i++)
				{
					pages[i] = i + 1;
				}
				return pages;
			}
		}
	}


}

