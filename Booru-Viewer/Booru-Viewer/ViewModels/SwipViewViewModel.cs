using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Booru_Viewer.ViewModels
{
	class SwipViewViewModel : ViewModelBase
	{

		private int perPage = 20;

		public SwipViewViewModel()
		{
			var settings = ApplicationData.Current.RoamingSettings.Values;
			if (settings["PerPage"] != null)
			{
				perPage = (int)settings["PerPage"];

			}
			images = GlobalInfo.ImageViewModels;
			Index = GlobalInfo.SelectedImage;
			Debug.WriteLine("iamges count is " + images.Count + ", globalinfo version is " + GlobalInfo.ImageViewModels.Count);
		}

		private string saveImageFailureReason = "";

		public string SaveImageFailureReason
		{
			get => saveImageFailureReason; set
			{
				saveImageFailureReason = value;
				RaisePropertyChanged();
			}
		}

		private bool saving;
		public bool Saving
		{
			get => saving; set
			{
				saving = value;
				RaisePropertyChanged();
			}
		}

		private IncrementalLoadingCollection<PostSource, FullImageViewModel> images;

		public IncrementalLoadingCollection<PostSource, FullImageViewModel> Images
		{
			get
			{

				images = GlobalInfo.ImageViewModels;



				if (GlobalInfo.SelectedImage < GlobalInfo.CurrentSearch.Count && GlobalInfo.SelectedImage >= 0)
				{
					var genTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].GeneralTags;
					var charTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].CharacterTags;
					var artistTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].ArtistTags;
					var copyTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].CopyrightTags;

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
				return images;
			}

		}

		public int Index
		{
			get => GlobalInfo.SelectedImage;
			set
			{
				GlobalInfo.SelectedImage = value;
				if (value > 0 && value < GlobalInfo.ImageViewModels.Count)
				{
					var genTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].GeneralTags;
					var charTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].CharacterTags;
					var artistTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].ArtistTags;
					var copyTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].CopyrightTags;
					Rating = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Rating;
					var favourites = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Favourites;
					GeneralTags.Clear();
					CharacterTags.Clear();
					ArtistTags.Clear();
					CopyrightTags.Clear();
					RaisePropertyChanged("GeneralTags");
					RaisePropertyChanged("ArtistTags");
					RaisePropertyChanged("CopyrightTags");
					RaisePropertyChanged("CharacterTags");
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

					if (favourites.Contains("fav:" + BooruAPI.UserModel.ID))
					{
						FavIcon = Symbol.Favorite;
						FavString = "Unfavourite";
					}
					else
					{
						FavIcon = Symbol.OutlineStar;
						FavString = "Favourite";
					}
					if (Images.Count - value < 5)
					{
						Images.LoadMoreItemsAsync(Convert.ToUInt32(perPage));
					}
				}

				RaisePropertyChanged();

			}
		}

		private string rating = "";

		public string Rating
		{
			get

			{
				rating = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Rating;
				return " " + rating;
			}

			set
			{
				rating = value;
				RaisePropertyChanged();
			}
		}

		public ObservableCollection<TagViewModel> GeneralTags { get; set; } = new ObservableCollection<TagViewModel>();


		public ObservableCollection<TagViewModel> CharacterTags { get; set; } = new ObservableCollection<TagViewModel>();


		public ObservableCollection<TagViewModel> ArtistTags { get; set; } = new ObservableCollection<TagViewModel>();


		public ObservableCollection<TagViewModel> CopyrightTags { get; set; } = new ObservableCollection<TagViewModel>();

		private Symbol favIcon = Symbol.OutlineStar;

		public Symbol FavIcon
		{
			get
			{
				if (GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Favourites.Contains("fav:" + BooruAPI.UserModel.ID))
				{
					favIcon = Symbol.Favorite;
				}
				return favIcon;
			}
			set
			{
				favIcon = value;
				RaisePropertyChanged();
			}
		}

		private string favString = "Favourite";

		public string FavString
		{
			get
			{
				if (GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Favourites.Contains("fav:" + BooruAPI.UserModel.ID))
				{
					favString = "Unfavourite";
				}
				return favString;
			}
			set
			{
				favString = value;
				RaisePropertyChanged();
			}
		}

		async void SaveImageExec(bool showNotification = true)
		{
			Saving = true;
			SaveImageFailureReason = await ImageSaver.SaveImage(images[Index].LargeImageURL);
			Saving = false;
			if (showNotification)
			{
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
									Source = images[Index].FullImageURL
								},
								new AdaptiveText()
								{
									Text = SaveImageFailureReason
								}
							}
						}
					}
				};
				Windows.Data.Xml.Dom.XmlDocument doc = content.GetXml();
				ToastNotification not = new ToastNotification(doc);
				ToastNotificationManager.ConfigureNotificationMirroring(NotificationMirroring.Disabled);
				ToastNotificationManager.CreateToastNotifier().Show(not);
				ToastNotificationManager.History.Clear();
			}
		}



		public ICommand SaveImage => new RelayCommand<bool>(SaveImageExec);

		public ICommand FavouriteImage => new RelayCommand(FavouriteImageExec);
		async void FavouriteImageExec()
		{
			var postIndex = GlobalInfo.SelectedImage;
			if (FavIcon == Symbol.OutlineStar)
			{
				if (await BooruAPI.FavouriteImage(GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage]))
				{
					GlobalInfo.CurrentSearch[postIndex].Fav_String += " fav:" + BooruAPI.UserModel.ID;
					FavIcon = Symbol.Favorite;
					FavString = "Unfavourite";
				}

			}
			else
			{
				if (await BooruAPI.UnfavouriteImage(GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage]))
				{
					var im = GlobalInfo.CurrentSearch[postIndex];
					im.Fav_String = im.Fav_String.Replace(" fav:" + BooruAPI.UserModel.ID, "");
					FavIcon = Symbol.OutlineStar;
					FavString = "Favourite";
				}
			}
		}

	}
}
