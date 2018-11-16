using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Booru_Viewer.Types;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Booru_Viewer.ViewModels;
using Booru_Viewer.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using ListViewBase = Windows.UI.Xaml.Controls.ListViewBase;


namespace Booru_Viewer
{
	//TODO:Add navigation pane so you can have multiple searches at the same time Going to need to use code to duplicate the views I think

	public sealed partial class MainPage : Page
	{
		private MainPageViewModel ViewModel { get; set; }
		private GridView _imageGridView;
		private Button _searchButton;
		private Button _searchFavouritesButton;
		private Button _addTagButton;
		private ContentDialog _confirmAgeDialog;
		private ContentDialog _searchDialog;
		private ListView _savedSearchesList;
		private AutoSuggestBox _tagTextBox;
		private ListView _savedSearchesListForReal;

		private Button _savedSearchInvoke;
		//private ListView SavedSearchesList;
		private bool _isMultiSelectEnabled = false;

		public MainPage()
		{
			InitializeComponent();



			//var favs = BooruAPI.GetUserFavourites().Result;
			//foreach (var image in favs)
			//{
			//	GlobalInfo.FavouriteImages.Add(image.id.ToString(), image);
			//}
			this.NavigationCacheMode = NavigationCacheMode.Required;


			Loaded += (sender, args) =>
			{
				ViewModel = DataContext as MainPageViewModel;
				var childrenOfHub = AllChildren(MainHub);
				_imageGridView = childrenOfHub.OfType<GridView>().First(x => x.Name == "ImageGridView");

				_confirmAgeDialog = childrenOfHub.OfType<ContentDialog>().First(x => x.Name == "ConfirmAgeDialog");
				_searchDialog = childrenOfHub.OfType<ContentDialog>().First(x => x.Name == "SearchDialog");
				//SavedSearchCommandInvoker = childrenOfHub.OfType<InvokeCommandAction>().First();

				SavedSearchesHub.Loaded += (o, eventArgs1) =>
				{
					var childs = AllChildren(SavedSearchesHub);
					_savedSearchesListForReal = childs.OfType<ListView>().FirstOrDefault(x => x.Name == "SavedSearchesListForReal");
					_savedSearchInvoke = childs.OfType<Button>().FirstOrDefault(x => x.Name == "SavedSearchInvoke");
				};
				_imageGridView.Loaded += (sender3, args3) =>
				{
					_imageGridView.RightTapped += (o, eventArgs) =>
					{
						eventArgs.Handled = false;
						var elements = VisualTreeHelper.FindElementsInHostCoordinates(eventArgs.GetPosition(o as UIElement), _imageGridView);
						var imageView = elements.First((x) => x.GetType() == typeof(ImageEx)) as ImageEx;
						Debug.WriteLine("imageView is " + imageView);
						ViewModel.ImageContextOpened = imageView.DataContext as FullImageViewModel;
					};

					if (GlobalInfo.CurrentSearch.Count > 0)
					{
						Debug.WriteLine("ImageCount coming back is: " + GlobalInfo.CurrentSearch.Count);
						int layoutcount = 0;
						_imageGridView.LayoutUpdated += (sender1, o) =>
						{
							if (layoutcount == 0)
							{
								layoutcount++;
								if (_imageGridView.Items.Count > 0)
								{
									var image = _imageGridView.Items[GlobalInfo.SelectedImage];
									_imageGridView.ScrollIntoView(image, ScrollIntoViewAlignment.Leading);
								}
							}

						};
						_imageGridView.UpdateLayout();

					}

				};
				_searchDialog.Opened += (sender4, args4) =>
				{
					var childrenOfDialog = AllChildren(_searchDialog.ContentTemplateRoot);
					_addTagButton = childrenOfDialog.OfType<Button>().First(x => x.Name == "AddTagButton");
					_searchButton = childrenOfDialog.OfType<Button>().First(x => x.Name == "SearchButton");
					_searchFavouritesButton = childrenOfDialog.OfType<Button>().First(x => x.Name == "SearchFavouritesButton");
					_savedSearchesList = childrenOfDialog.OfType<ListView>().First(x => x.Name == "SavedSearchesList");
					_searchButton.Loaded += (sender1, args1) => { _searchButton.CommandParameter = SearchAppBarButton; };
					_searchFavouritesButton.Loaded += (sender2, args2) => { _searchButton.CommandParameter = SearchAppBarButton; };
					_tagTextBox = childrenOfDialog.OfType<AutoSuggestBox>().First(x => x.Name == "TagTextBox");

				};
				SearchClicked(null, null);
				if (_searchButton != null)
				{
					var command = _searchButton.Command;
					if (command != null)
					{
						command.Execute(_searchButton);
					}
					else
					{
						Debug.WriteLine("search command returned null");
					}

				}
			};



			QuestionableCheckbox.Loaded += (sender, args) =>
			{
				var isOver18 = ApplicationData.Current.RoamingSettings.Values["IsOver18"];
				if (isOver18 is bool b)
				{
					QuestionableCheckbox.IsEnabled = b;
				}
				else
				{
					QuestionableCheckbox.IsEnabled = false;
				}
			};
			ExplicitCheckbox.Loaded += (sender, args) =>
			{
				var isOver18 = ApplicationData.Current.RoamingSettings.Values["IsOver18"];
				if (isOver18 is bool b)
				{
					ExplicitCheckbox.IsEnabled = b;
				}
				else
				{
					ExplicitCheckbox.IsEnabled = false;
				}
			};
			SafeCheckbox.Loaded += (sender, args) =>
			{
				var isOver18 = ApplicationData.Current.RoamingSettings.Values["IsOver18"];
				if (isOver18 is bool b)
				{
					SafeCheckbox.IsEnabled = b;
				}
				else
				{
					SafeCheckbox.IsEnabled = false;
				}
			};
			UnlockExplicitContentButton.Loaded += (sender, args) =>
			{
				var isOver18 = ApplicationData.Current.RoamingSettings.Values["IsOver18"];
				if (isOver18 is bool b)
				{
					UnlockExplicitContentButton.Visibility = b ? Visibility.Collapsed : Visibility.Visible;
				}
				else
				{
					UnlockExplicitContentButton.Visibility = Visibility.Visible;
				}
			};
		}



		private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//if (sender is GridView grid && grid.SelectionMode == ListViewSelectionMode.Single)
			//{

			//	GlobalInfo.SelectedImage = grid.SelectedIndex;
			//}
			//Frame.Navigate(typeof(SwipeView));
		}

		private void GridView_MultiSelectChanged(object sender, SelectionChangedEventArgs e)
		{
			SaveButton.IsEnabled = _imageGridView.SelectedItems.Count > 0;

		}

		private void ImageGridView_OnItemClick(object sender, ItemClickEventArgs e)
		{
			if (sender is GridView grid)
			{
				if (grid.Name == "ImageGridView")
				{
					var index = grid.Items.IndexOf(e.ClickedItem);
					Debug.WriteLine("Index is " + index);
					GlobalInfo.SelectedImage = index;
					Frame.Navigate(typeof(SwipeView));
				}
				
			}
			else if (sender is ListView list && list.Name == "SavedForLaterGridView")
			{
				var index = list.Items.IndexOf(e.ClickedItem);
				Debug.WriteLine("Index is " + index);
				GlobalInfo.SelectedImage = index;
				Frame.Navigate(typeof(SavedForLaterSwipeView));
			}

		}

		private void AddTagClicked(object sender, RoutedEventArgs e)
		{

			_tagTextBox.GetBindingExpression(AutoSuggestBox.TextProperty).UpdateSource();
		}

		private void SaveLoginDataButtonTapped(object sender, TappedRoutedEventArgs e)
		{
			UsernameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			APIKeyTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void TagTextBox_TextChanged(AutoSuggestBox autoSuggestBox, AutoSuggestBoxTextChangedEventArgs args)
		{

		}

		private void APIKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			APIKeyTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void TagTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
		{

			if (e.Key == Windows.System.VirtualKey.Enter && e.KeyStatus.IsKeyReleased)
			{
				if (_addTagButton.Command.CanExecute(_addTagButton))
				{
					_addTagButton.Command.Execute(_addTagButton);
				}
			}

		}

		private void TagTapped(object sender, TappedRoutedEventArgs e)
		{
			((sender as FrameworkElement)?.DataContext as TagViewModel)?.SelectedTag.Execute(null);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			ViewModel?.RaisePropertyChanged("Thumbnails");


			ViewModel?.RaisePropertyChanged("FavouriteTags");

			Frame rootFrame = Window.Current.Content as Frame;
			if (rootFrame.CanGoBack)
			{
				// Show UI in title bar if opted-in and in-app backstack is not empty.
				SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
					AppViewBackButtonVisibility.Visible;
			}
			else
			{
				// Remove the UI from the title bar if in-app back stack is empty.
				SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
					AppViewBackButtonVisibility.Collapsed;
			}
			base.OnNavigatedTo(e);
			ViewModel?.RaisePropertyChanged("SavedForLater");
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			Debug.WriteLine("ImageCount is: " + GlobalInfo.CurrentSearch.Count);
			Debug.WriteLine("Globalinfo imageviewmodels count is " + GlobalInfo.ImageViewModels.Count + ", itemssource is " + (_imageGridView.Items).Count);
		}

		private void SavedSearchSelection(object sender, SelectionChangedEventArgs e)
		{
			var list = sender as ListView;

		}

		private void SettingsTapped(object sender, TappedRoutedEventArgs e)
		{


		}

		private void SearchButton_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			e.Handled = true;
			SearchAppBarButton.Flyout?.Hide();
		}


		private void BackToTopTapped(object sender, TappedRoutedEventArgs e)
		{
			if (_imageGridView.Items.Count > 0)
			{
				_imageGridView.ScrollIntoView(_imageGridView.Items[0]);
			}
		}

		private void SavedSearchesList_OnItemClick(object sender, ItemClickEventArgs e)
		{


		}

		private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			var textbox = sender as TextBox;
			textbox?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
		}

		private void ImageOpened(object sender, ImageExOpenedEventArgs e)
		{


		}

		private void ImageEx_OnImageExFailed(object sender, ImageExFailedEventArgs e)
		{
			var img = sender as ImageEx;
			img.SetValue(ImageEx.SourceProperty, img.Source);
			Debug.WriteLine("Failed to open " + (sender as ImageEx).Source + ".\nError message: " + e.ErrorMessage + ". \nException: " + e.ErrorException.Message);
		}

		private void PerPageSlider_OnLoaded(object sender, RoutedEventArgs e)
		{

		}

		async void SavedSearchesList_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			Debug.WriteLine("Drag Finished");
			await GlobalInfo.SaveSearches();
		}

		private void SaveAllDialog(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			sender.Hide();
		}



		private void TagTextBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			ViewModel.TagSuggestionChosen = true;
			ViewModel.CurrentTag = args.SelectedItem as string;
		}



		private void MultiSelectButtonTapped(object sender, RoutedEventArgs e)
		{
			var selectButton = sender as AppBarButton;
			if (!_isMultiSelectEnabled)
			{

				_imageGridView.ItemClick -= ImageGridView_OnItemClick;
				_imageGridView.SelectionChanged -= GridView_SelectionChanged;
				_imageGridView.SelectionChanged += GridView_MultiSelectChanged;
				selectButton.Icon = new SymbolIcon(Symbol.Cancel);
				_isMultiSelectEnabled = true;
				_imageGridView.SelectionMode = ListViewSelectionMode.Multiple;
				SearchAppBarButton.Visibility = Visibility.Collapsed;
				SettingsButton.Visibility = Visibility.Collapsed;
				SelectAllButton.Visibility = Visibility.Visible;
				SaveButton.Visibility = Visibility.Visible;
			}
			else
			{
				SaveButton.Visibility = Visibility.Collapsed;
				Debug.WriteLine(_imageGridView.SelectedRanges);
				_imageGridView.SelectedItems.Clear();
				_imageGridView.SelectionChanged -= GridView_MultiSelectChanged;
				_imageGridView.ItemClick += ImageGridView_OnItemClick;
				_imageGridView.SelectionChanged += GridView_SelectionChanged;

				SelectAllButton.Visibility = Visibility.Collapsed;
				SaveButton.Visibility = Visibility.Collapsed;
				selectButton.Icon = new SymbolIcon(Symbol.Bullets);
				_isMultiSelectEnabled = false;
				_imageGridView.SelectionMode = ListViewSelectionMode.None;
				SearchAppBarButton.Visibility = Visibility.Visible;
				SettingsButton.Visibility = Visibility.Visible;
			}


		}

		private void SelectAllClicked(object sender, RoutedEventArgs e)
		{
			foreach (var img in _imageGridView.Items)
			{
				_imageGridView.SelectedItems.Add(img);
			}
		}

		private void ImageContextMenuSelectClick(object sender, RoutedEventArgs e)
		{
			var imageViewModel = (sender as FrameworkElement).DataContext as FullImageViewModel;

			MultiSelectButtonTapped(SelectButton, new RoutedEventArgs());
			_imageGridView.SelectedItems.Add(imageViewModel);

		}

		private void SaveButton_OnClick(object sender, RoutedEventArgs e)
		{
			ViewModel.SaveSelectedImages.Execute(_imageGridView.SelectedItems);

			SelectAllButton.Visibility = Visibility.Collapsed;
			SaveButton.Visibility = Visibility.Collapsed;
			_isMultiSelectEnabled = false;
			SelectButton.Icon = new SymbolIcon(Symbol.Bullets);
			_imageGridView.SelectionMode = ListViewSelectionMode.None;
			SearchAppBarButton.Visibility = Visibility.Visible;
			SettingsButton.Visibility = Visibility.Visible;
			SaveButton.IsEnabled = false;
			_imageGridView.SelectionChanged -= GridView_MultiSelectChanged;
			_imageGridView.ItemClick += ImageGridView_OnItemClick;
			_imageGridView.SelectionChanged += GridView_SelectionChanged;
		}

		private void Over18Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			ApplicationData.Current.RoamingSettings.Values["IsOver18"] = true;
			QuestionableCheckbox.IsEnabled = true;
			ExplicitCheckbox.IsEnabled = true;
			SafeCheckbox.IsEnabled = true;
			UnlockExplicitContentButton.Visibility = Visibility.Collapsed;
		}

		private void Under18Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			ApplicationData.Current.RoamingSettings.Values["IsOver18"] = false;
			sender.Hide();
		}

		private async void UnlockExplicitContentButton_OnClick(object sender, RoutedEventArgs e)
		{
			await _confirmAgeDialog.ShowAsync();
		}

		private void NoImagestextSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Debug.WriteLine("no image text visibility is " + (sender as TextBlock).Visibility);
		}

		private async void SearchButtonClicked(object sender, RoutedEventArgs e)
		{
			await _searchDialog.ShowAsync();
		}

		private void SearchClicked(object sender, RoutedEventArgs e)
		{
			_searchDialog?.Hide();
		}

		private void CloseSearchDialogClick(object sender, RoutedEventArgs e)
		{
			_searchDialog.Hide();
		}

		public List<Control> AllChildren(DependencyObject parent)
		{
			var list = new List<Control>();
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is Control)
					list.Add(child as Control);
				list.AddRange(AllChildren(child));
			}
			return list;
		}

		private void SavedSearchesListForReal_OnItemClick(object sender, ItemClickEventArgs e)
		{
			if (_savedSearchInvoke != null || _savedSearchesListForReal != null)
			{
				_savedSearchInvoke.Command.Execute(e.ClickedItem);
				MainHub.ScrollToSection(SearchResultsSection);
			}
			else
			{
				Debug.WriteLine("SavedSearchInvoke is null");
			}
		}

		private void ImageExBase_OnImageExOpened(object sender, ImageExOpenedEventArgs e)
		{
			Debug.WriteLine("ImageEx opened with source: " + (sender as ImageEx).Source);
		}

		private void PreviewPictureClicked(object sender, ItemClickEventArgs e)
		{
			var list = sender as ListView;
			var parent = list.FindAscendant<ListView>();
			if (parent != null)
			{
				var i = _savedSearchesListForReal.Items.IndexOf(list.DataContext);
				_savedSearchInvoke.Command.Execute(_savedSearchesListForReal.Items[i]);
				MainHub.ScrollToSection(SearchResultsSection);
			}
		}

		private void ListViewBase_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			Debug.WriteLine("Items in saved list changed");
		}
	}
}
