using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Booru_Viewer.ViewModels
{
	public class SavedSearchViewModel : ViewModelBase
	{

		private MainPageViewModel parentVM;
		public SavedSearchViewModel(string[] tags, MainPageViewModel pVM)
		{
			Tags = tags;
			parentVM = pVM;
		}

		public string Search
		{
			get
			{
				var text = "";
				if (Tags.Length > 0)
				{
					foreach (var tag in Tags)
					{
						text += tag + ", ";
					}
					text = text.Substring(0, text.Length - 2);
				}
				return text;
			}
		}

		public string[] Tags { get; set; }

		void DeleteSearchExecute()
		{
			
			parentVM.DeleteSavedSearch(this);
		}

		void StartSearchExecute()
		{
			parentVM.StartSavedSearch(Tags);
		}
		public ICommand StartSearch => new RelayCommand(StartSearchExecute);
		public ICommand DeleteSearch => new RelayCommand(DeleteSearchExecute);
	}
}
