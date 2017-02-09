using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

namespace Booru_Viewer.ViewModels
{
	public class TagViewModel : ViewModelBase
	{
		private MainPageViewModel parentVM;

		public TagViewModel(string tag, MainPageViewModel pageVM)
		{
			this.tag = tag;
			parentVM = pageVM;
		}

		public TagViewModel(string tag)
		{
			this.tag = tag;
		}
		private string tag;
		public string Tag { get { return tag; } set { tag = value; RaisePropertyChanged(); } }

		void RemoveTagExecute()
		{
			
				parentVM.RemoveTag(this);
			
		}

		bool RemoveTagCanExecute()
		{
			return true;
		}
		void CopyTagExec(string value)
		{
			DataPackage dp = new DataPackage();
			dp.RequestedOperation = DataPackageOperation.Copy;
			dp.SetText(value);
			Clipboard.SetContent(dp);

		}
		public ICommand CopyTag => new RelayCommand<string>(CopyTagExec);
		public ICommand RemoveTag => new RelayCommand(RemoveTagExecute, RemoveTagCanExecute);
	}
}
