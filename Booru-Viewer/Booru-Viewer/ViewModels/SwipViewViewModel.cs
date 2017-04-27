using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Web.Http;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

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

		private ObservableCollection<FullImageViewModel> images = new ObservableCollection<FullImageViewModel>();

		public ObservableCollection<FullImageViewModel> Images
		{
			get
			{
				if (images.Count == 0)
				{
					foreach (var image in GlobalInfo.CurrentSearch)
					{
						images.Add(new FullImageViewModel(image.File_Url, image.Large_File_Url, image.image_width, image.image_height));
					}
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
				if (GlobalInfo.CurrentSearch.Count - value < 4)
				{

					LoadMoreImages();
				}
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

		async void LoadMoreImages()
		{
			BooruAPI.Page++;
			var result = await BooruAPI.SearchPosts(await PrepTags(), BooruAPI.Page, perPage, GlobalInfo.ContentCheck, false);
			if (result.Item3 == HttpStatusCode.Ok.ToString())
			{
				AddImages(result.Item2);
			}
		}


		async Task<string[]> PrepTags()
		{
			string[] tags = new string[GlobalInfo.CurrentTags.Count];
			var tagModels = new ObservableCollection<TagViewModel>(GlobalInfo.CurrentTags);
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

		void AddImages(List<ImageModel> thumbnails)
		{

			foreach (var post in thumbnails)
			{
				Images.Add(new FullImageViewModel(post.File_Url, post.Large_File_Url, post.image_width, post.image_height));
			}


			RaisePropertyChanged("Images");
		}

		async void SaveImageExec()
		{
			Saving = true;
			SaveImageFailureReason = await ImageSaver.SaveImage(images[Index].LargeImage);

		}



		public ICommand SaveImage => new RelayCommand(SaveImageExec);

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
