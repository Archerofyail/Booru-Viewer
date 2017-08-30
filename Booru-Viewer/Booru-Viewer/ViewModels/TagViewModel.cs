using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Booru_Viewer.Types;

namespace Booru_Viewer.ViewModels
{
	public class TagViewModel : ViewModelBase, IComparable
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
		public bool IsFavourite => GlobalInfo.FavouriteTags.Contains(tag);

		public Symbol FavouriteIcon => IsFavourite ? Symbol.SolidStar : Symbol.OutlineStar;

		public Visibility Selected { get; set; } = Visibility.Collapsed;
		

		void RemoveTagExecute()
		{

			parentVM.RemoveTagExec(this);
			parentVM.RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			parentVM.RaisePropertyChanged("TotalTagCount");
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

		void FavouriteTagEx()
		{
			if (!IsFavourite)
			{

				GlobalInfo.FavouriteTags.Add(tag);

				parentVM?.FavouriteTags.Add(this);
				var tagList = parentVM?.FavouriteTags.ToList();
				tagList?.Sort();
				GlobalInfo.FavouriteTags = new ObservableCollection<string>(tagList.Select(x => x.tag));
				if (parentVM != null)
				{
					parentVM.FavouriteTags = new ObservableCollection<TagViewModel>(tagList);
				}
			}
			else
			{
				GlobalInfo.FavouriteTags.Remove(tag);
				parentVM?.FavouriteTags.Remove(this);
			}
			parentVM?.RaisePropertyChanged("FavouriteTags");
			RaisePropertyChanged("IsFavourite");
			RaisePropertyChanged("FavouriteIcon");
			GlobalInfo.SaveFavouriteTags();
		}

		void UnfavouriteTagEx()
		{
			GlobalInfo.FavouriteTags.Remove(tag);
			parentVM?.FavouriteTags.Remove(this);
			RaisePropertyChanged("IsFavourite");
			RaisePropertyChanged("FavouriteIcon");
			GlobalInfo.SaveFavouriteTags();
		}

		void AddTagToSearchEx()
		{
			parentVM?.CurrentTags.Add(new TagViewModel(tag, parentVM));
			parentVM?.RaisePropertyChanged("TotalTagCount");
		}

		void StartSearchFromThisEx()
		{
			parentVM?.CurrentTags.Clear();
			parentVM?.CurrentTags.Add(this);
			parentVM?.RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			parentVM?.RaisePropertyChanged("TotalTagCount");
			parentVM?.StartSearchExecute();
		}

		void AddPrefixEx(string prefix)
		{
			if (prefix[0] == tag[0])
			{
				Tag = tag.Replace("~", "").Replace("-", "");
				return;
			}
			Tag = tag.Replace("~", "").Replace("-", "");
			Tag = tag.Insert(0, prefix);
		}
		public ICommand SelectedTag => new RelayCommand(() => {Selected = (Selected == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible); RaisePropertyChanged("Selected"); });
		public ICommand AddPrefix => new RelayCommand<string>(AddPrefixEx);
		public ICommand AddTagToSearch => new RelayCommand(AddTagToSearchEx);
		public ICommand UnfavouriteTag => new RelayCommand(UnfavouriteTagEx);
		public ICommand FavouriteTag => new RelayCommand(FavouriteTagEx);
		public ICommand CopyTag => new RelayCommand<string>(CopyTagExec);
		public ICommand RemoveTag => new RelayCommand(RemoveTagExecute, RemoveTagCanExecute);
		public ICommand StartSearchFromFavourite => new RelayCommand(StartSearchFromThisEx);
		public int CompareTo(object obj)
		{
			if (obj != null && obj.GetType() == typeof(TagViewModel))
			{
				return String.Compare(tag, ((TagViewModel)obj).tag, StringComparison.CurrentCultureIgnoreCase);
			}
			return 1;
		}
	}
}
