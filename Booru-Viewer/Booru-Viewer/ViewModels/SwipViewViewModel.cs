using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Web.Http;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Windows.Storage;

namespace Booru_Viewer.ViewModels
{
	class SwipViewViewModel : ViewModelBase
	{

		private int perPage = 20;

		public SwipViewViewModel()
		{
			var settings = ApplicationData.Current.RoamingSettings.Values;
			perPage = (int)settings["PerPage"];
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
						images.Add(new FullImageViewModel(BooruAPI.BaseURL + image.Large_File_Url));
					}
					if (GlobalInfo.SelectedImage < GlobalInfo.CurrentSearch.Count && GlobalInfo.SelectedImage >= 0)
					{
						var tags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Tags;

						Tags.Clear();
						foreach (var tag in tags)
						{
							Tags.Add(new TagViewModel(tag));
						}
					}
				}
				return images;
			}

		}

		public int Index
		{
			get
			{

				return GlobalInfo.SelectedImage;
			}
			set
			{
				GlobalInfo.SelectedImage = value;
				if (GlobalInfo.CurrentSearch.Count - value < 2)
				{

					LoadMoreImages();
				}
				var tags = GlobalInfo.CurrentSearch[GlobalInfo.SelectedImage].Tags;
				Tags.Clear();
				foreach (var tag in tags)
				{
					Tags.Add(new TagViewModel(tag));
				}
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<TagViewModel> tags = new ObservableCollection<TagViewModel>();

		public ObservableCollection<TagViewModel> Tags
		{
			get { return tags; }
			set { tags = value; }
		}

		async void LoadMoreImages()
		{
			BooruAPI.Page++;
			var result = await BooruAPI.SearchPosts(await PrepTags(), BooruAPI.Page, perPage, false);
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
				Images.Add(new FullImageViewModel(BooruAPI.BaseURL + post.Large_File_Url));
			}


			RaisePropertyChanged("Images");
		}

		void SaveImageExec()
		{
			ImageSaver.SaveImage(images[Index].FullImage);
		}



		public ICommand SaveImage => new RelayCommand(SaveImageExec);

	}
}
