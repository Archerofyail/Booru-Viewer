using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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

		public TagViewModel()
		{
		}

		private string tag;
		public string Tag { get => tag; set { tag = value; RaisePropertyChanged(); } }

		void RemoveTagExecute()
		{
			
				parentVM.RemoveTagExec(this);
			parentVM.RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
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
