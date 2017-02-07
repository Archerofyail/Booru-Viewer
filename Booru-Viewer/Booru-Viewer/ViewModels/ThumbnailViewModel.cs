using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Booru_Viewer.Types;

namespace Booru_Viewer.ViewModels
{
	public class ThumbnailViewModel
	{

		public ThumbnailViewModel(string prevUrl, string fullUrl, MainPageViewModel parentvm)
		{
			PreviewURL = prevUrl;
			FullURL = fullUrl;
			parentVM = parentvm;
		}

		public  MainPageViewModel parentVM;
		public string PreviewURL { get; set; }
		public string FullURL { get; set; }
		void SaveImageExecute()
		{
			ImageSaver.SaveImage(FullURL);
		}
		

		void EnableMultiSelectExecute()
		{
			parentVM.ImageSelectionMode = ListViewSelectionMode.Multiple;
		}
		

		public ICommand SaveImage { get { return new RelayCommand(SaveImageExecute); } }
		public ICommand EnableMultiSelect { get { return new RelayCommand(EnableMultiSelectExecute); } }
	}
}
