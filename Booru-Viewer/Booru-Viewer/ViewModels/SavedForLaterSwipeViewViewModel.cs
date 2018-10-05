using System;
using System.Collections.Immutable;
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
using Booru_Viewer.Models;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Booru_Viewer.ViewModels
{
	class SavedForLaterSwipeViewViewModel : ViewModelBase
	{

		private int _perPage = 20;

		public SavedForLaterSwipeViewViewModel()
		{
			var settings = ApplicationData.Current.RoamingSettings.Values;
			if (settings["PerPage"] != null)
			{
				_perPage = (int)settings["PerPage"];

			}
			_images = new ObservableCollection<FullImageViewModel>();
			foreach (var img in GlobalInfo.ImagesSavedForLater)
			{
				_images.Add(new FullImageViewModel(img, img.id, img.Preview_File_Url, img.Large_File_Url, "https://danbooru.donmai.us/posts/" + img.id, null, null));
			}
			GlobalInfo.SelectedImage = GlobalInfo.SelectedImage;
			Debug.WriteLine("iamges count is " + _images.Count + ", globalinfo version is " + GlobalInfo.ImageViewModels.Count);
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

		private ObservableCollection<FullImageViewModel> _images;

		public ObservableCollection<FullImageViewModel> Images
		{
			get
			{

				_images = new ObservableCollection<FullImageViewModel>();
				foreach (var img in GlobalInfo.ImagesSavedForLater)
				{
					_images.Add(new FullImageViewModel(img, img.id, img.Preview_File_Url, img.Large_File_Url, "https://danbooru.donmai.us/posts/" + img.id, null, null));
				}
				if (GlobalInfo.SelectedImage < GlobalInfo.ImagesSavedForLater.Count && GlobalInfo.SelectedImage >= 0)
				{
					var genTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].GeneralTags;
					var charTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].CharacterTags;
					var artistTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].ArtistTags;
					var copyTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].CopyrightTags;

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

				}
				return _images;
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
					var genTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].GeneralTags;
					var charTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].CharacterTags;
					var artistTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].ArtistTags;
					var copyTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].CopyrightTags;
					var metaTags = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].MetaTags;
					Rating = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].Rating;
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
					CurrentUrl = GlobalInfo.ImagesSavedForLater[value].Large_File_Url;
					RaisePropertyChanged("CurrentURL");
					RaisePropertyChanged("FavIcon");
				}

				RaisePropertyChanged();

			}
		}

		private string _rating = "";

		public string Rating
		{
			get

			{
				_rating = GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].Rating;
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
				if (GlobalInfo.FavouriteImages.Contains(Images[GlobalInfo.SelectedImage].SelectedImagePostId))
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
				if (GlobalInfo.CurrentSearchTags.Contains("fav:archerofyail") || GlobalInfo.CurrentSearchTags.Contains("ordfav:archerofyail"))
				{
					_favString = "Unfavourite";
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
				if (GlobalInfo.ImagesSavedForLater.Any(x => x.id == Images[GlobalInfo.SelectedImage].Image.id))
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
				if (GlobalInfo.ImagesSavedForLater.Any(x => x.id == Images[GlobalInfo.SelectedImage].Image.id))
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
			SaveImageFailureReason = (await ImageSaver.SaveImage(_images[GlobalInfo.SelectedImage].LargeImageURL)).Item2;
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
									Source = _images[GlobalInfo.SelectedImage].FullImageURL
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
				if (await BooruApi.FavouriteImage(GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage]))
				{
					
					GlobalInfo.FavouriteImages.Add(GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage].id);
					await GlobalInfo.SaveFavouritePosts();
					FavIcon = Symbol.Favorite;
					FavString = "Unfavourite";
				}

			}
			else
			{
				if (await BooruApi.UnfavouriteImage(GlobalInfo.ImagesSavedForLater[GlobalInfo.SelectedImage]))
				{
					var im = GlobalInfo.ImagesSavedForLater[postIndex];
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
			await Launcher.LaunchUriAsync(new Uri(_images[GlobalInfo.SelectedImage].WebsiteUrl));
		}


		public ICommand SaveImageForLater => new RelayCommand(SaveImageForLaterEx);

		async void SaveImageForLaterEx()
		{
			if (SaveForLaterIcon == "\uE728")
			{
				GlobalInfo.ImagesSavedForLater.Add(Images[GlobalInfo.SelectedImage].Image);
			}
			else
			{
				GlobalInfo.ImagesSavedForLater.Remove(GlobalInfo.ImagesSavedForLater.First(x=>x.id == Images[GlobalInfo.SelectedImage].Image.id));
				if (GlobalInfo.ImagesSavedForLater.Count == 1)
				{
					//Images.RemoveAt(0);
					
				}
				if (GlobalInfo.SelectedImage == Images.Count - 1)
				{
					//Images.RemoveAt(Images.Count - 1);
					
				}
				else
				{
				//Images.Remove(Images[GlobalInfo.SelectedImage]);

				}
				
			}
			await GlobalInfo.SaveSavedForLaterImages();
			RaisePropertyChanged("SaveForLaterIcon");
			RaisePropertyChanged("SaveForLaterString");
			RaisePropertyChanged("Images");
		}
	}
}
