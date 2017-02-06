using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;

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
			get { return GlobalInfo.SelectedImage; }
			set
			{
				GlobalInfo.SelectedImage = value;
				RaisePropertyChanged();
			}
		}
	}
}
