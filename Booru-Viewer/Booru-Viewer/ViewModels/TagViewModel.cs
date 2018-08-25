using System;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Booru_Viewer.Models;
using Booru_Viewer.Types;

namespace Booru_Viewer.ViewModels
{
	public class TagViewModel : ViewModelBase, IComparable
	{
		private MainPageViewModel _parentVm;
		public Tag Tag;
		public TagViewModel(Tag tag, MainPageViewModel pageVm)
		{
			Tag = tag;
			_name = tag.Name;
			_parentVm = pageVm;
			_type = tag.category;
		}

		public TagViewModel(Tag tag)
		{
			Tag = new Tag(tag.Name.Substring(0), tag.category);
			_name = tag.Name;
		}

		public TagViewModel()
		{
		}

		private TagType _type;
		public string Type
		{
			get => _type.ToString();
			set
			{
				if (Enum.TryParse(value, out TagType result))
				{
					_type = result;
				}
				RaisePropertyChanged();
			}
		}

		private string _name;
		public string Name { get => _name; set { _name = value; RaisePropertyChanged(); } }

		public bool IsFavourite
		{
			get
			{
				foreach (var tag in GlobalInfo.FavouriteTags)
				{
					if (tag.Name == Name)
					{
						return true;
					}
				}
				return false;
			}
		} 

		public Symbol FavouriteIcon => IsFavourite ? Symbol.SolidStar : Symbol.OutlineStar;

		public Visibility Selected { get; set; } = Visibility.Collapsed;


		void RemoveTagExecute()
		{

			_parentVm?.RemoveTagExec(this);
			_parentVm?.RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			_parentVm?.RaisePropertyChanged("TotalTagCount");
		}

		bool RemoveTagCanExecute()
		{
			return true;
		}
		void CopyTagExec(string value)
		{
			var dp = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
			dp.SetText(value);
			Clipboard.SetContent(dp);

		}

		async void FavouriteTagEx()
		{
			if (!IsFavourite && GlobalInfo.FavouriteTags.All(x => x.Name.TrimStart('~','-') != Tag.Name.TrimStart('~','-')))
			{

				if (Tag.category == TagType.Unknown || Tag.category == TagType.General)
				{
					var taginf = (await BooruApi.GetTagInfo(Tag.Name));
					if (taginf != null)
					{
						Tag.Category = taginf.category;
					}
				}
				
				GlobalInfo.FavouriteTags.Add(Tag);

				_parentVm?.FavouriteTags.First(x => x.Key == Tag.category).Add(this);
				
				_parentVm?.FavouriteTags.First(x => x.Key == Tag.category).Sort();
				_parentVm?.RaisePropertyChanged("FavouriteTags");
			}
			else
			{
				GlobalInfo.FavouriteTags.Remove(Tag);
				_parentVm?.FavouriteTags.First(x => x.Key == Tag.category).Remove(this);
			}
			_parentVm?.RaisePropertyChanged("FavouriteTags");
			RaisePropertyChanged("IsFavourite");
			RaisePropertyChanged("FavouriteIcon");
			_parentVm?.RaisePropertyChanged("DontHaveSavedSearches");
			await GlobalInfo.SaveFavouriteTags();
		}

		async void UnfavouriteTagEx()
		{
			GlobalInfo.FavouriteTags.Remove(Tag);
			_parentVm?.FavouriteTags.First(x => x.Key == Tag.category).Remove(this);
			RaisePropertyChanged("IsFavourite");
			RaisePropertyChanged("FavouriteIcon");
			_parentVm?.RaisePropertyChanged("DontHaveSavedSearches");
			await GlobalInfo.SaveFavouriteTags();
		}

		void AddTagToSearchEx()
		{
			if (_parentVm == null)
				return;

			if (_parentVm.CurrentTags.Any(x => x.Name.TrimStart('-', '~') == Tag.Name))
			{
				return;
			}
			_parentVm?.CurrentTags.Add(new TagViewModel(Tag, _parentVm));
			_parentVm?.RaisePropertyChanged("TotalTagCount");
		}

		void StartSearchFromThisEx()
		{
			_parentVm?.CurrentTags.Clear();
			_parentVm?.CurrentTags.Add(this);
			_parentVm?.RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			_parentVm?.RaisePropertyChanged("TotalTagCount");
			_parentVm?.StartSearchExecute();
		}

		void AddPrefixEx(string prefix)
		{
			Selected = Visibility.Collapsed;
			RaisePropertyChanged("Selected");
			if (prefix[0] == Name[0])
			{
				Name = Name.TrimStart('~', '-');
				return;
			}
			Name = Name.TrimStart('-', '~');
			Name = Name.Insert(0, prefix);
		}
		public ICommand SelectedTag => new RelayCommand(() => { Selected = (Selected == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible); RaisePropertyChanged("Selected"); });
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
				return String.Compare(Name, ((TagViewModel)obj).Name, StringComparison.CurrentCultureIgnoreCase);
			}
			return 1;
		}
	}
}
