using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Booru_Viewer.Types;
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
			
			GlobalInfo.SavedSearches.FirstOrDefault((x) =>
			{
				if (x.Length == Tags.Length)
				{

					for (var i = 0; i < x.Length; i++)
					{
						var tag = x[i];
						if (tag != Tags[i])
						{
							return false;

						}
					}
				}
				return true;
			});
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
