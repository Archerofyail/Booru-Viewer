using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Booru_Viewer.ViewModels
{
	public class ThumbnailViewModel
	{



		public string PreviewURL { get; set; }
		public string FullURL { get; set; }
		void SaveImageExecute()
		{

		}
		bool SaveImageCanExecute()
		{
			return true;
		}

		public ICommand SaveImage { get { return new RelayCommand(SaveImageExecute, SaveImageCanExecute); } }
	}
}
