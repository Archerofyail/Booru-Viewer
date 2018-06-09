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
		private MainPageViewModel parentVM;
		public Tag Tag;
		public TagViewModel(Tag tag, MainPageViewModel pageVM)
		{
			Tag = tag;
			name = tag.Name;
			parentVM = pageVM;
			_type = tag.category;
		}

		public TagViewModel(Tag tag)
		{
			Tag = new Tag(tag.Name.Substring(0), tag.category);
			name = tag.Name;
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

		private string name;
		public string Name { get => name; set { name = value; RaisePropertyChanged(); } }

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

			parentVM?.RemoveTagExec(this);
			parentVM?.RaisePropertyChanged("IsSignedOutWithMoreThan2Tags");
			parentVM?.RaisePropertyChanged("TotalTagCount");
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
					var taginf = (await BooruAPI.GetTagInfo(Tag.Name));
					if (taginf != null)
					{
						Tag.Category = taginf.category;
					}
				}
				
				GlobalInfo.FavouriteTags.Add(Tag);

				parentVM?.FavouriteTags.First(x => x.Key == Tag.category).Add(this);
				
				parentVM?.FavouriteTags.First(x => x.Key == Tag.category).Sort();
				parentVM?.RaisePropertyChanged("FavouriteTags");
			}
			else
			{
				GlobalInfo.FavouriteTags.Remove(Tag);
				parentVM?.FavouriteTags.First(x => x.Key == Tag.category).Remove(this);
			}
			parentVM?.RaisePropertyChanged("FavouriteTags");
			RaisePropertyChanged("IsFavourite");
			RaisePropertyChanged("FavouriteIcon");
			parentVM?.RaisePropertyChanged("DontHaveSavedSearches");
			await GlobalInfo.SaveFavouriteTags();
		}

		async void UnfavouriteTagEx()
		{
			GlobalInfo.FavouriteTags.Remove(Tag);
			parentVM?.FavouriteTags.First(x => x.Key == Tag.category).Remove(this);
			RaisePropertyChanged("IsFavourite");
			RaisePropertyChanged("FavouriteIcon");
			parentVM?.RaisePropertyChanged("DontHaveSavedSearches");
			await GlobalInfo.SaveFavouriteTags();
		}

		void AddTagToSearchEx()
		{
			if (parentVM == null)
				return;

			if (parentVM.CurrentTags.Any(x => x.Name.TrimStart('-', '~') == Tag.Name))
			{
				return;
			}
			parentVM?.CurrentTags.Add(new TagViewModel(Tag, parentVM));
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
