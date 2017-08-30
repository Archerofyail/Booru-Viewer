using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Booru_Viewer.Types;
using System.Diagnostics;
using System.Linq;
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

		private bool isMultiSelectEnabled = false;
		public MainPage()
		{
			InitializeComponent();
			SearchClicked(null, null);
			if (SearchButton != null)
			{
				var command = SearchButton.Command;
				if (command != null)
				{
					command.Execute(SearchButton);
				}
				else
				{
					Debug.WriteLine("search command returned null");
				}

			}


			this.NavigationCacheMode = NavigationCacheMode.Required;
			SearchButton.Loaded += (sender, args) => { SearchButton.CommandParameter = SearchAppBarButton; };
			SearchFavouritesButton.Loaded += (sender, args) => { SearchButton.CommandParameter = SearchAppBarButton; };

			SavedSearchesList.Loaded += (sender, args) =>
			{
				SavedSearchesList.GetBindingExpression(ListView.ItemsSourceProperty).UpdateSource();
			};
			Loaded += (sender, args) =>
			{
				ViewModel = DataContext as MainPageViewModel;
			};
			ImageGridView.Loaded += (sender, args) =>
			{
				ImageGridView.RightTapped += (o, eventArgs) =>
				{
					//ImageContextFlyout.ShowAt(o as UIElement, eventArgs.GetPosition(null));
					eventArgs.Handled = false;
					var elements = VisualTreeHelper.FindElementsInHostCoordinates(eventArgs.GetPosition(o as UIElement), ImageGridView);
					var imageView = elements.First((x) => x.GetType() == typeof(ImageEx)) as ImageEx;
					Debug.WriteLine("imageView is " + imageView);
					ViewModel.ImageContextOpened = imageView.DataContext as FullImageViewModel;
				};

				if (GlobalInfo.CurrentSearch.Count > 0)
				{
					Debug.WriteLine("ImageCount coming back is: " + GlobalInfo.CurrentSearch.Count);
					int layoutcount = 0;
					ImageGridView.LayoutUpdated += (sender1, o) =>
					{
						if (layoutcount == 0)
						{
							layoutcount++;
							var image = ImageGridView.Items[GlobalInfo.SelectedImage];
							ImageGridView.ScrollIntoView(image, ScrollIntoViewAlignment.Leading);
						}

					};
					ImageGridView.UpdateLayout();

				}

			};



		}



		private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var grid = sender as GridView;
			if (grid != null && grid.SelectionMode == ListViewSelectionMode.Single)
			{

				GlobalInfo.SelectedImage = grid.SelectedIndex;
			}
			Frame.Navigate(typeof(SwipeView));
		}

		private void GridView_MultiSelectChanged(object sender, SelectionChangedEventArgs e)
		{
			SaveButton.IsEnabled = ImageGridView.SelectedItems.Count > 0;
			//SaveButton.GetBindingExpression(Button.CommandParameterProperty)?.UpdateSource();
		}

		private void ImageGridView_OnItemClick(object sender, ItemClickEventArgs e)
		{
			var grid = sender as GridView;
			if (grid != null)
			{
				var index = grid.Items.IndexOf(e.ClickedItem);
				Debug.WriteLine("Index is " + index);
				GlobalInfo.SelectedImage = index;
				Frame.Navigate(typeof(SwipeView));
			}
		}

		private void SearchClicked(object sender, RoutedEventArgs e)
		{

		}

		private void AddTagClicked(object sender, RoutedEventArgs e)
		{
			//AddTagButton.Focus(FocusState.Programmatic);
			TagTextBox.GetBindingExpression(AutoSuggestBox.TextProperty).UpdateSource();
		}

		private void SaveLoginDataButtonTapped(object sender, TappedRoutedEventArgs e)
		{
			UsernameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			APIKeyTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void TagTextBox_TextChanged(AutoSuggestBox autoSuggestBox, AutoSuggestBoxTextChangedEventArgs args)
		{
			//TagTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void APIKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			APIKeyTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void TagTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
		{

			if (e.Key == Windows.System.VirtualKey.Enter && e.KeyStatus.IsKeyReleased)
			{
				if (AddTagButton.Command.CanExecute(AddTagButton))
				{
					AddTagButton.Command.Execute(AddTagButton);
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
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			Debug.WriteLine("ImageCount is: " + GlobalInfo.CurrentSearch.Count);
			Debug.WriteLine("Globalinfo imageviewmodels count is " + GlobalInfo.ImageViewModels.Count + ", itemssource is " + (ImageGridView.Items).Count);
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
			StartPageTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
			e.Handled = true;
			SearchAppBarButton.Flyout?.Hide();
		}


		private void BackToTopTapped(object sender, TappedRoutedEventArgs e)
		{
			if (ImageGridView.Items.Count > 0)
			{
				ImageGridView.ScrollIntoView(ImageGridView.Items[0]);
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

			//Debug.WriteLine("Image opened at URI:" + (sender as ImageEx).Source + "\n, index is " + ImageGridView.Items.IndexOf((sender as ImageEx).DataContext));
		}

		private void ImageEx_OnImageExFailed(object sender, ImageExFailedEventArgs e)
		{
			var img = sender as ImageEx;
			img.SetValue(ImageEx.SourceProperty, img.Source);
			Debug.WriteLine("Failed to open " + (sender as ImageEx).Source + ".\nError message: " + e.ErrorMessage + ".\nException: " + e.ErrorException);
		}

		private void PerPageSlider_OnLoaded(object sender, RoutedEventArgs e)
		{
			//var perPage = ApplicationData.Current.RoamingSettings.Values["PerPage"] as int?;
			//if (perPage != null)
			//{
			//	PerPageSlider.Value = perPage.Value;
			//}
		}

		async void SavedSearchesList_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
		{
			Debug.WriteLine("Drag Finished");
			await GlobalInfo.SaveSearches(SavedSearchesList.ItemsSource as List<SavedSearchViewModel>);
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
			if (!isMultiSelectEnabled)
			{

				ImageGridView.ItemClick -= ImageGridView_OnItemClick;
				ImageGridView.SelectionChanged -= GridView_SelectionChanged;
				ImageGridView.SelectionChanged += GridView_MultiSelectChanged;
				selectButton.Icon = new SymbolIcon(Symbol.Cancel);
				isMultiSelectEnabled = true;
				ImageGridView.SelectionMode = ListViewSelectionMode.Multiple;
				SearchAppBarButton.Visibility = Visibility.Collapsed;
				SettingsButton.Visibility = Visibility.Collapsed;
				SelectAllButton.Visibility = Visibility.Visible;
				SaveButton.Visibility = Visibility.Visible;
			}
			else
			{
				SaveButton.Visibility = Visibility.Collapsed;
				Debug.WriteLine(ImageGridView.SelectedRanges);
				ImageGridView.SelectedItems.Clear();
				ImageGridView.SelectionChanged -= GridView_MultiSelectChanged;
				ImageGridView.ItemClick += ImageGridView_OnItemClick;
				ImageGridView.SelectionChanged += GridView_SelectionChanged;

				SelectAllButton.Visibility = Visibility.Collapsed;
				SaveButton.Visibility = Visibility.Collapsed;
				selectButton.Icon = new SymbolIcon(Symbol.Bullets);
				isMultiSelectEnabled = false;
				ImageGridView.SelectionMode = ListViewSelectionMode.None;
				SearchAppBarButton.Visibility = Visibility.Visible;
				SettingsButton.Visibility = Visibility.Visible;
			}


		}

		private void SelectAllClicked(object sender, RoutedEventArgs e)
		{
			foreach (var img in ImageGridView.Items)
			{
				ImageGridView.SelectedItems.Add(img);
			}
		}

		private void ImageContextMenuSelectClick(object sender, RoutedEventArgs e)
		{
			var imageViewModel = (sender as FrameworkElement).DataContext as FullImageViewModel;
			
			MultiSelectButtonTapped(SelectButton, new RoutedEventArgs());
			ImageGridView.SelectedItems.Add(imageViewModel);

		}

		private void SaveButton_OnClick(object sender, RoutedEventArgs e)
		{
			ViewModel.SaveSelectedImages.Execute(ImageGridView.SelectedItems);
			
			SelectAllButton.Visibility = Visibility.Collapsed;
			SaveButton.Visibility = Visibility.Collapsed;
			isMultiSelectEnabled = false;
			SelectButton.Icon = new SymbolIcon(Symbol.Bullets);
			ImageGridView.SelectionMode = ListViewSelectionMode.None;
			SearchAppBarButton.Visibility = Visibility.Visible;
			SettingsButton.Visibility = Visibility.Visible;
			//ImageGridView.SelectedItems.Clear();
			SaveButton.IsEnabled = false;
			ImageGridView.SelectionChanged -= GridView_MultiSelectChanged;
			ImageGridView.ItemClick += ImageGridView_OnItemClick;
			ImageGridView.SelectionChanged += GridView_SelectionChanged;
		}
	}
}
