using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using ListViewBase = Windows.UI.Xaml.Controls.ListViewBase;


namespace Booru_Viewer
{
	//TODO:Add navigation pane so you can have multiple searches at the same time Going to need to use code to duplicate the views I think

	public sealed partial class MainPage : Page
	{
		private MainPageViewModel ViewModel { get; set; }
		private bool _isMultiSelectEnabled;

		void PageLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			DataContext = new MainPageViewModel();
			ViewModel = DataContext as MainPageViewModel;

			ImageGridView.Loaded += (sender3, args3) =>
			{
				ImageGridView.RightTapped += (o, eventArgs) =>
				{
					eventArgs.Handled = false;
					var elements = VisualTreeHelper.FindElementsInHostCoordinates(eventArgs.GetPosition(o as UIElement), ImageGridView);
					var imageView = elements.First((x) => x.GetType() == typeof(ImageEx)) as ImageEx;
					Debug.WriteLine("imageView is " + imageView);
					ViewModel.ImageContextOpened = imageView.DataContext as FullImageViewModel;
				};
			};

			if (ImageGridView.Items?.Count > 0 && GlobalInfo.SelectedImage > 0 && GlobalInfo.SelectedImage <= ImageGridView.Items.Count - 1)
			{
				ImageGridView.ScrollIntoView(ImageGridView.Items[GlobalInfo.SelectedImage]);
			}

			Debug.WriteLine("Image count in gridview is  " + ImageGridView.Items.Count);
			Debug.WriteLine("Image count in GlobalInfo is  " + GlobalInfo.CurrentSearch.Count);
			Debug.WriteLine("Image count in GlobalInfo ViewModels is  " + GlobalInfo.ImageViewModels.Count);
		}

		public MainPage()
		{
			InitializeComponent();
			Loaded += PageLoaded;
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


		private void GridView_MultiSelectChanged(object sender, SelectionChangedEventArgs e)
		{
			SaveButton.IsEnabled = ImageGridView.SelectedItems.Count > 0;

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
				else if (grid.Name == "SavedForLaterGridView")
				{
					var index = grid.Items.IndexOf(e.ClickedItem);
					Debug.WriteLine("Index is " + index);
					GlobalInfo.SelectedImage = index;
					Frame.Navigate(typeof(SavedForLaterSwipeView));
				}

			}


		}

		private void AddTagClicked(object sender, RoutedEventArgs e)
		{
			TagTextBox.GetBindingExpression(AutoSuggestBox.TextProperty).UpdateSource();
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

			if (e.Key == Windows.System.VirtualKey.Enter && e.KeyStatus.IsKeyReleased && AddTagButton != null)
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

				ImageGridView.ItemClick -= ImageGridView_OnItemClick;
				ImageGridView.SelectionChanged += GridView_MultiSelectChanged;
				selectButton.Icon = new SymbolIcon(Symbol.Cancel);
				_isMultiSelectEnabled = true;
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
				SelectAllButton.Visibility = Visibility.Collapsed;
				SaveButton.Visibility = Visibility.Collapsed;
				selectButton.Icon = new SymbolIcon(Symbol.Bullets);
				_isMultiSelectEnabled = false;
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
			_isMultiSelectEnabled = false;
			SelectButton.Icon = new SymbolIcon(Symbol.Bullets);
			ImageGridView.SelectionMode = ListViewSelectionMode.None;
			SearchAppBarButton.Visibility = Visibility.Visible;
			SettingsButton.Visibility = Visibility.Visible;
			SaveButton.IsEnabled = false;
			ImageGridView.SelectionChanged -= GridView_MultiSelectChanged;
			ImageGridView.ItemClick += ImageGridView_OnItemClick;
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
			await ConfirmAgeDialog.ShowAsync();
		}

		private async void SearchButtonClicked(object sender, RoutedEventArgs e)
		{
			await SearchDialog.ShowAsync();
		}

		private void SearchClicked(object sender, RoutedEventArgs e)
		{
			SearchDialog?.Hide();
		}

		private void CloseSearchDialogClick(object sender, RoutedEventArgs e)
		{
			SearchDialog.Hide();
		}

		private void SavedSearchesListForReal_OnItemClick(object sender, ItemClickEventArgs e)
		{
			if (SavedSearchInvoke != null && SavedSearchesListForReal != null)
			{
				SavedSearchInvoke.Command.Execute(e.ClickedItem);
				MainHub.SelectedIndex = 0;
			}
			else
			{
				Debug.WriteLine("SavedSearchInvoke is null");
			}
		}

		private void PreviewPictureClicked(object sender, ItemClickEventArgs e)
		{
			var list = sender as ListView;
			var parent = list.FindAscendant<ListView>();
			if (parent != null)
			{
				var i = SavedSearchesListForReal.Items.IndexOf(list.DataContext);
				SavedSearchInvoke.Command.Execute(SavedSearchesListForReal.Items[i]);
				MainHub.SelectedIndex = 0;
			}
		}

		private void MainHub_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (MainHub.SelectedIndex != 0)
			{
				SearchAppBarButton.Visibility = Visibility.Collapsed;
				SelectButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				SearchAppBarButton.Visibility = Visibility.Visible;
				SelectButton.Visibility = Visibility.Visible;
			}
			if (MainHub.SelectedIndex == 1)
			{
				FindName("SavedSearchGrid");
			}


		}
	}
}
