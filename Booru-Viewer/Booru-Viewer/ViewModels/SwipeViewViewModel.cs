using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Windows.Storage;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Booru_Viewer.ViewModels
{
	public class SwipeViewViewModel : ViewModelBase
	{

		private int _perPage = 20;
		public EventHandler ImageChangedEvent;
		public SwipeViewViewModel()
		{
			var settings = ApplicationData.Current.RoamingSettings.Values;
			if (settings["PerPage"] != null)
			{
				_perPage = (int)settings["PerPage"];

			}
			_images = GlobalInfo.ImageViewModels;
			Index = GlobalInfo.SelectedImage;
			Debug.WriteLine("iamges count is " + _images.Count + ", globalinfo version is " + GlobalInfo.ImageViewModels.Count);
			ImageChangedEvent += ChangeImageData;
		}

		public string CurrentUrl { get; set; } = "";

		private string _saveImageFailureReason = "";

		public string SaveImageFailureReason
		{
			get => _saveImageFailureReason; set
			{
				_saveImageFailureReason = value;
				RaisePropertyChanged();
			}
		}

		private bool _saving;
		public bool Saving
		{
			get => _saving; set
			{
				_saving = value;
				RaisePropertyChanged();
			}
		}

		private IncrementalLoadingCollection<PostSource, FullImageViewModel> _images;

		public IncrementalLoadingCollection<PostSource, FullImageViewModel> Images
		{
			get
			{
				if (_images.Count == 0)
				{
					_images = GlobalInfo.ImageViewModels;
				}

				foreach (var image in _images)
				{
					image.parentVm = this;
				}
				//ImageChangedEvent?.Invoke(this, EventArgs.Empty);

				return _images;
			}

		}

		public void ChangeImageData(object sender, EventArgs eventArgs)
		{
			if (GlobalInfo.SelectedImage < GlobalInfo.CurrentSearch.Count && GlobalInfo.SelectedImage >= 0)
			{
				ImageModel img = null;
				if (GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].id ==
				    Images[GlobalInfo.SelectedImage].SelectedImagePostId)
				{
					img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage];
				}
				else
				{
					img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].ChildrenImages
						.First(x => Images[GlobalInfo.SelectedImage].SelectedImagePostId == x.id);
				}

				var genTags = img.GeneralTags;
				var charTags = img.CharacterTags;
				var artistTags = img.ArtistTags;
				var copyTags = img.CopyrightTags;

				GeneralTags.Clear();
				foreach (var tag in genTags)
				{
					GeneralTags.Add(new TagViewModel(tag));
				}
				CharacterTags.Clear();
				foreach (var tag in charTags)
				{
					CharacterTags.Add(new TagViewModel(tag));
				}
				ArtistTags.Clear();
				foreach (var tag in artistTags)
				{
					ArtistTags.Add(new TagViewModel(tag));
				}
				CopyrightTags.Clear();
				foreach (var tag in copyTags)
				{
					CopyrightTags.Add(new TagViewModel(tag));
				}
				if (GlobalInfo.FavouriteImages.Contains(Images[Index].SelectedImagePostId))
				{
					FavIcon = Symbol.Favorite;
					FavString = "Unfavourite";
				}
				else
				{
					FavString = "Favourite";
					FavIcon = Symbol.OutlineStar;
				}
			}
		}


		public int Index
		{
			get => GlobalInfo.SelectedImage;
			set
			{
				GlobalInfo.SelectedImage = value;
				if (value >= 0 && value < GlobalInfo.ImageViewModels.Count)
				{
					ImageModel img = null;
					if (GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].id ==
					    Images[GlobalInfo.SelectedImage].SelectedImagePostId)
					{
						img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage];
					}
					else
					{
						img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].ChildrenImages
							.First(x => Images[GlobalInfo.SelectedImage].SelectedImagePostId == x.id);
					}

					var genTags = img.GeneralTags;
					var charTags = img.CharacterTags;
					var artistTags = img.ArtistTags;
					var copyTags = img.CopyrightTags;
					var metaTags = img.MetaTags;
					Rating = img.Rating;
					GeneralTags.Clear();
					CharacterTags.Clear();
					ArtistTags.Clear();
					CopyrightTags.Clear();
					MetaTags.Clear();
					RaisePropertyChanged("GeneralTags");
					RaisePropertyChanged("ArtistTags");
					RaisePropertyChanged("CopyrightTags");
					RaisePropertyChanged("CharacterTags");
					RaisePropertyChanged("MetaTags");
					foreach (var tag in genTags)
					{
						GeneralTags.Add(new TagViewModel(tag));
					}

					foreach (var tag in charTags)
					{
						CharacterTags.Add(new TagViewModel(tag));
					}

					foreach (var tag in artistTags)
					{
						ArtistTags.Add(new TagViewModel(tag));
					}

					foreach (var tag in copyTags)
					{
						CopyrightTags.Add(new TagViewModel(tag));
					}

					foreach (var metaTag in metaTags)
					{
						MetaTags.Add(new TagViewModel(metaTag));
					}

					var favTag = BooruApi.Username.ToLower() + " ";
					if (Images.Count - value < 5)
					{
						Images.LoadMoreItemsAsync(Convert.ToUInt32(_perPage));
					}
					CurrentUrl = GlobalInfo.CurrentSearch[value].Large_File_Url;
					RaisePropertyChanged("CurrentURL");
					RaisePropertyChanged("FavIcon");
					RaisePropertyChanged("SaveForLaterIcon");
					RaisePropertyChanged("FavString");
					RaisePropertyChanged("SaveForLaterString");

				}

				RaisePropertyChanged();

			}
		}

		private string _rating = "";

		public string Rating
		{
			get

			{
				_rating = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Rating;
				return " " + _rating;
			}

			set
			{
				_rating = value;
				RaisePropertyChanged();
			}
		}

		public ObservableCollection<TagViewModel> GeneralTags { get; set; } = new ObservableCollection<TagViewModel>();


		public ObservableCollection<TagViewModel> CharacterTags { get; set; } = new ObservableCollection<TagViewModel>();


		public ObservableCollection<TagViewModel> ArtistTags { get; set; } = new ObservableCollection<TagViewModel>();


		public ObservableCollection<TagViewModel> CopyrightTags { get; set; } = new ObservableCollection<TagViewModel>();

		public ObservableCollection<TagViewModel> MetaTags { get; set; } = new ObservableCollection<TagViewModel>();

		private Symbol _favIcon = Symbol.OutlineStar;

		public Symbol FavIcon
		{
			get
			{
				if (GlobalInfo.FavouriteImages.Contains(Images[Index].SelectedImagePostId))
				{
					_favIcon = Symbol.Favorite;
				}
				else
				{
					_favIcon = Symbol.OutlineStar;
				}
				return _favIcon;
			}
			set
			{
				_favIcon = value;
				RaisePropertyChanged();
			}
		}

		private string _favString = "Favourite";

		public string FavString
		{
			get
			{
				if (GlobalInfo.FavouriteImages.Contains(Images[Index].SelectedImagePostId))
				{
					_favString = "Unfavourite";
				}
				else
				{
					_favString = "Favourite";
				}
				return _favString;
			}
			set
			{
				_favString = value;
				RaisePropertyChanged();
			}
		}

		private string _saveForLaterIcon = "\uE728";

		public string SaveForLaterIcon
		{
			get
			{
				if (GlobalInfo.ImagesSavedForLater.Any(x => x.id == Images[Index].SelectedImagePostId))
				{
					return "\uE8D9";
				}
				else
				{
					return "\uE728";
				}

			}
		}

		private string _saveForLaterString = "Save For Later";

		public string SaveForLaterString
		{
			get
			{
				if (GlobalInfo.ImagesSavedForLater.Any(x => x.id == Images[Index].Image.id))
				{
					_saveForLaterString = "Remove from list";
					return _saveForLaterString;
				}
				else
				{
					_saveForLaterString = "Save For Later";
					return _saveForLaterString;
				}
			}
			set
			{
				_saveForLaterString = value;
				RaisePropertyChanged();
			}
		}

		async void SaveImageExec(bool showNotification = true)
		{
			Saving = true;
			SaveImageFailureReason = (await ImageSaver.SaveImage(_images[Index].LargeImageURL)).Item2;
			Saving = false;
			if (showNotification)
			{
				ToastContent content = new ToastContent
				{
					Visual = new ToastVisual
					{
						BindingGeneric = new ToastBindingGeneric
						{
							Children =
							{
								new AdaptiveImage
								{
									Source = _images[Index].FullImageURL
								},
								new AdaptiveText
								{
									Text = SaveImageFailureReason
								}
							}
						}
					},
				};
				Windows.Data.Xml.Dom.XmlDocument doc = content.GetXml();
				ToastNotification not = new ToastNotification(doc);
				ToastNotificationManager.ConfigureNotificationMirroring(NotificationMirroring.Disabled);
				ToastNotificationManager.CreateToastNotifier().Show(not);
			}
		}



		public ICommand SaveImage => new RelayCommand<bool>(SaveImageExec);

		public ICommand FavouriteImage => new RelayCommand(FavouriteImageExec);
		async void FavouriteImageExec()
		{
			var postIndex = GlobalInfo.SelectedImage;
			if (FavIcon == Symbol.OutlineStar)
			{
				ImageModel img = null;
				if (GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].id ==
				    Images[GlobalInfo.SelectedImage].SelectedImagePostId)
				{
					img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage];
				}
				else
				{
					img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].ChildrenImages
						.First(x => Images[GlobalInfo.SelectedImage].SelectedImagePostId == x.id);
				}
				if (await BooruApi.FavouriteImage(img))
				{

					GlobalInfo.FavouriteImages.Add(img.id);
					await GlobalInfo.SaveFavouritePosts();
					FavIcon = Symbol.Favorite;
					FavString = "Unfavourite";
				}

			}
			else
			{
				ImageModel img = null;
				if (GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].id ==
				    Images[GlobalInfo.SelectedImage].SelectedImagePostId)
				{
					img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage];
				}
				else
				{
					img = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].ChildrenImages
						.First(x => Images[GlobalInfo.SelectedImage].SelectedImagePostId == x.id);
				}
				if (await BooruApi.UnfavouriteImage(img))
				{
					var im = GlobalInfo.CurrentSearch[postIndex];


					GlobalInfo.FavouriteImages.Remove(im.id);
					await GlobalInfo.SaveFavouritePosts();
					FavIcon = Symbol.OutlineStar;
					FavString = "Favourite";

				}
			}
		}

		public ICommand OpenPostInWebsite => new RelayCommand(OpenPostInWebsiteEx);

		async void OpenPostInWebsiteEx()
		{
			await Launcher.LaunchUriAsync(new Uri(_images[Index].WebsiteUrl));
		}


		public ICommand SaveImageForLater => new RelayCommand(SaveImageForLaterEx);

		async void SaveImageForLaterEx()
		{
			if (SaveForLaterIcon == "\uE728")
			{
				GlobalInfo.ImagesSavedForLater.Add(Images[Index].Image);
			}
			else
			{
				GlobalInfo.ImagesSavedForLater.Remove(GlobalInfo.ImagesSavedForLater.First(x => x.id == Images[Index].Image.id));
			}
			await GlobalInfo.SaveSavedForLaterImages();
			RaisePropertyChanged("SaveForLaterIcon");
			RaisePropertyChanged("SaveForLaterString");

		}
	}
}
