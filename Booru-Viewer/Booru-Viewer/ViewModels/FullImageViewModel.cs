using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Booru_Viewer.ViewModels
{
	public class FullImageViewModel : ViewModelBase
	{
		public string CurrentImage { get; set; }
		public string FullImageURL { get; set; }
		public string FullImageWithLoginURL { get; set; }
		public string LargeImageURL { get; set; }
		public string PreviewURL { get; set; }
		private string id;
		public int Width { get; set; }
		public int Height { get; set; }
		public string WebsiteURL { get; set; }
		public ObservableCollection<FullImageViewModel> ChildrenImages { get; set; }
		public FullImageViewModel ParentImage;
		public ImageModel Image { get; private set; }

		public string SelectedImagePostID
		{
			get
			{
				if (ChildrenImages.Count > 0)
				{
					if (ChildrenImages.FirstOrDefault(x => x.CurrentImage == CurrentImage) != null)
					{
						return ChildrenImages.First(x => x.CurrentImage == CurrentImage).id;
					}
				}

				return id;
			}
		}

		public FullImageViewModel(ImageModel image, string id, string previewURL, string imageURLUrl, string websiteURL, List<ImageModel> childImages, FullImageViewModel parentImage, string largeImage = null, int width = 0, int height = 0)
		{
			Image = image;
			FullImageURL = imageURLUrl;
			FullImageWithLoginURL = imageURLUrl + "?login=" + BooruAPI.Username + "&api_key=" + BooruAPI.APIKey;
			LargeImageURL = largeImage ?? imageURLUrl;
			PreviewURL = previewURL;
			Width = width;
			Height = height;
			WebsiteURL = websiteURL;
			ChildrenImages = new ObservableCollection<FullImageViewModel>();
			this.id = id;
			if (childImages != null && childImages.Count > 0)
			{
				foreach (var childImage in childImages)
				{
					ChildrenImages.Add(new FullImageViewModel(childImage, childImage.id, childImage.Preview_File_Url, childImage.File_Url,
						BooruAPI.BaseURL + "/posts/" + childImage.id, null, this, childImage.Large_File_Url));
				}
			}

			if (parentImage != null)
			{
				ParentImage = parentImage;
			}
			else
			{
				CurrentImage = FullImageURL;
			}
		}

		void SwitchImageEx(string url)
		{
			ParentImage?.SwitchImage.Execute(url);
			if (ParentImage == null)
			{
				CurrentImage = url;

				RaisePropertyChanged("CurrentImage");
			}
		}
		public ICommand SwitchImage => new RelayCommand<string>(SwitchImageEx);
	}
}
