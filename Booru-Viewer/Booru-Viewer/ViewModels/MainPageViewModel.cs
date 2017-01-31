using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace Booru_Viewer.ViewModels
{
	class MainPageViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<string> ImageLinks { get { return GlobalInfo.ImageURLs; } }
		
		public ObservableCollection<string> CurrentTags { get { return GlobalInfo.CurrentTags; } 
		}

		private string currentTag = "";

		public string CurrentTag {
			get
			{
				return currentTag;
			}
			set { NotifyPropertyChanged("CurrentTag"); }
	}
	

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string memberName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
		}

		void AddTagExecute()
		{
			CurrentTags.Add(CurrentTag);
			CurrentTag = "";
			NotifyPropertyChanged("CurrentTags");
		}

		bool AddTagCanExecute()
		{
			return true;
		}

		void RemoveTageExecute()
		{

		}

		bool RemoveTagCanExecute()
		{
			return true;
		}

		public ICommand AddTag { get { return  new RelayCommand(AddTagExecute, AddTagCanExecute);} }
		public ICommand RemoveTag { get { return new RelayCommand(RemoveTageExecute, RemoveTagCanExecute); } }
	}
}
