﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Web.Http;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Booru_Viewer.ViewModels
{
	class SwipViewViewModel : ViewModelBase
	{
	

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
				RaisePropertyChanged();
			}
		}

		async void LoadMoreImages()
		{
			BooruAPI.Page++;
			var result = await BooruAPI.SearchPosts(await PrepTags(), BooruAPI.Page, false);
			if (result.Item3 == HttpStatusCode.Ok)
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
