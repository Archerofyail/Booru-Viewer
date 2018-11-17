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
		private string _id;
		public string WebsiteUrl { get; set; }
		public ObservableCollection<FullImageViewModel> ChildrenImages { get; set; }
		public FullImageViewModel ParentImage;
		public ImageModel Image { get; private set; }
		public SwipeViewViewModel parentVm { get; set; }

		public string SelectedImagePostId
		{
			get
			{
				if (ChildrenImages.Count > 0)
				{
					if (ChildrenImages.FirstOrDefault(x => x.FullImageURL == CurrentImage) != null)
					{
						return ChildrenImages.First(x => x.FullImageURL == CurrentImage)._id;
					}
				}

				return _id;
			}
		}

		public FullImageViewModel(ImageModel image, string id, string previewUrl, string imageUrlUrl, string websiteUrl, List<ImageModel> childImages, FullImageViewModel parentImage, string largeImage = null, SwipeViewViewModel parentVM = null)
		{
			Image = image;
			FullImageURL = imageUrlUrl;
			FullImageWithLoginURL = imageUrlUrl + "?login=" + BooruApi.Username + "&api_key=" + BooruApi.ApiKey;
			LargeImageURL = largeImage ?? imageUrlUrl;
			PreviewURL = previewUrl;
			WebsiteUrl = websiteUrl;
			ChildrenImages = new ObservableCollection<FullImageViewModel>();
			this._id = id;
			if (childImages != null && childImages.Count > 0)
			{
				foreach (var childImage in childImages)
				{
					ChildrenImages.Add(new FullImageViewModel(childImage, childImage.id, childImage.Preview_File_Url, childImage.File_Url,
						BooruApi.BaseUrl + "/posts/" + childImage.id, null, this, childImage.Large_File_Url, parentVM));
				}
			}

			if (parentImage != null)
			{
				ParentImage = parentImage;
				//CurrentImage = FullImageURL;
			}
			else
			{
				CurrentImage = FullImageURL;
			}

			this.parentVm = parentVM;
		}

		void SwitchImageEx(string url)
		{
			ParentImage?.SwitchImage.Execute(url);
			if (ParentImage == null)
			{
				CurrentImage = url;
				
				parentVm?.ChangeImageData(this, EventArgs.Empty);

				RaisePropertyChanged("CurrentImage");
			}
		}
		public ICommand SwitchImage => new RelayCommand<string>(SwitchImageEx);
	}
}
