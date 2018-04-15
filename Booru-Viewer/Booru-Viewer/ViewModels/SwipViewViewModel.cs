using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

		public string CurrentURL { get; set; } = "";

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
						GeneralTags.Add(new TagViewModel(new Tag(tag)));
					}
					CharacterTags.Clear();
					foreach (var tag in charTags)
					{
						CharacterTags.Add(new TagViewModel(new Tag(tag)));
					}
					ArtistTags.Clear();
					foreach (var tag in artistTags)
					{
						ArtistTags.Add(new TagViewModel(new Tag(tag)));
					}
					CopyrightTags.Clear();
					foreach (var tag in copyTags)
					{
						CopyrightTags.Add(new TagViewModel(new Tag(tag)));
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
					var metaTags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].MetaTags;
					Rating = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Rating;
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
						GeneralTags.Add(new TagViewModel(new Tag(tag)));
					}

					foreach (var tag in charTags)
					{
						CharacterTags.Add(new TagViewModel(new Tag(tag)));
					}

					foreach (var tag in artistTags)
					{
						ArtistTags.Add(new TagViewModel(new Tag(tag)));
					}

					foreach (var tag in copyTags)
					{
						CopyrightTags.Add(new TagViewModel(new Tag(tag)));
					}

					foreach (var metaTag in metaTags)
					{
						MetaTags.Add(new TagViewModel(new Tag(metaTag)));
					}

					if (GlobalInfo.CurrentSearchTags.Contains("fav:archerofyail") || GlobalInfo.CurrentSearchTags.Contains("ordfav:archerofyail"))
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
					CurrentURL = GlobalInfo.CurrentSearch[value].File_Url;
					RaisePropertyChanged("CurrentURL");
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

		public ObservableCollection<TagViewModel> MetaTags { get; set; } = new ObservableCollection<TagViewModel>();

		private Symbol favIcon = Symbol.OutlineStar;

		public Symbol FavIcon
		{
			get
			{
				if (GlobalInfo.CurrentSearchTags.Contains("fav:archerofyail") || GlobalInfo.CurrentSearchTags.Contains("ordfav:archerofyail"))
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
				if (GlobalInfo.CurrentSearchTags.Contains("fav:archerofyail") || GlobalInfo.CurrentSearchTags.Contains("ordfav:archerofyail"))
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
					FavIcon = Symbol.Favorite;
					FavString = "Unfavourite";
				}

			}
			else
			{
				if (await BooruAPI.UnfavouriteImage(GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage]))
				{
					var im = GlobalInfo.CurrentSearch[postIndex];
					
					FavIcon = Symbol.OutlineStar;
					FavString = "Favourite";
				}
			}
		}

		public ICommand OpenPostInWebsite => new RelayCommand(OpenPostInWebsiteEx);

		async void OpenPostInWebsiteEx()
		{
			await Launcher.LaunchUriAsync(new Uri(images[Index].WebsiteURL));
		}

	}
}
