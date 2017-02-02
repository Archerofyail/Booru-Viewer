using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

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

		}
		bool SaveImageCanExecute()
		{
			return true;
		}

		void EnableMultiSelectExecute()
		{
			parentVM.ImageSelectionMode = SelectionMode.Multiple;
		}
		bool EnableMultiSelectCanExecute()
		{
			return true;
		}

		public ICommand SaveImage { get { return new RelayCommand(SaveImageExecute, SaveImageCanExecute); } }
		public ICommand EnableMultiSelect { get { return new RelayCommand(EnableMultiSelectExecute, EnableMultiSelectCanExecute); } }
	}
}
