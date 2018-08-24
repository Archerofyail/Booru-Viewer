﻿using System;
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
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Booru_Viewer.ViewModels;
using Booru_Viewer.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.Xaml.Interactions.Core;
using ListViewBase = Windows.UI.Xaml.Controls.ListViewBase;


namespace Booru_Viewer
{
	//TODO:Add navigation pane so you can have multiple searches at the same time Going to need to use code to duplicate the views I think

	public sealed partial class MainPage : Page
	{
		private MainPageViewModel ViewModel { get; set; }
		private GridView ImageGridView;
		private Button SearchButton;
		private Button SearchFavouritesButton;
		private Button AddTagButton;
		private ContentDialog ConfirmAgeDialog;
		private ContentDialog SearchDialog;
		private ListView SavedSearchesList;
		private AutoSuggestBox TagTextBox;
		private ListView SavedSearchesListForReal;

		private Button SavedSearchInvoke;
		//private ListView SavedSearchesList;
		private bool isMultiSelectEnabled = false;

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
				ImageGridView = childrenOfHub.OfType<GridView>().First(x => x.Name == "ImageGridView");

				ConfirmAgeDialog = childrenOfHub.OfType<ContentDialog>().First(x => x.Name == "ConfirmAgeDialog");
				SearchDialog = childrenOfHub.OfType<ContentDialog>().First(x => x.Name == "SearchDialog");
				//SavedSearchCommandInvoker = childrenOfHub.OfType<InvokeCommandAction>().First();

				SavedSearchesHub.Loaded += (o, eventArgs1) =>
				{
					var childs = AllChildren(SavedSearchesHub);
					SavedSearchesListForReal = childs.OfType<ListView>().FirstOrDefault(x => x.Name == "SavedSearchesListForReal");
					SavedSearchInvoke = childs.OfType<Button>().FirstOrDefault(x => x.Name == "SavedSearchInvoke");
				};
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

					if (GlobalInfo.CurrentSearch.Count > 0)
					{
						Debug.WriteLine("ImageCount coming back is: " + GlobalInfo.CurrentSearch.Count);
						int layoutcount = 0;
						ImageGridView.LayoutUpdated += (sender1, o) =>
						{
							if (layoutcount == 0)
							{
								layoutcount++;
								if (ImageGridView.Items.Count > 0)
								{
									var image = ImageGridView.Items[GlobalInfo.SelectedImage];
									ImageGridView.ScrollIntoView(image, ScrollIntoViewAlignment.Leading);
								}
							}

						};
						ImageGridView.UpdateLayout();

					}

				};
				SearchDialog.Opened += (sender4, args4) =>
				{
					var childrenOfDialog = AllChildren(SearchDialog.ContentTemplateRoot);
					AddTagButton = childrenOfDialog.OfType<Button>().First(x => x.Name == "AddTagButton");
					SearchButton = childrenOfDialog.OfType<Button>().First(x => x.Name == "SearchButton");
					SearchFavouritesButton = childrenOfDialog.OfType<Button>().First(x => x.Name == "SearchFavouritesButton");
					SavedSearchesList = childrenOfDialog.OfType<ListView>().First(x => x.Name == "SavedSearchesList");
					SearchButton.Loaded += (sender1, args1) => { SearchButton.CommandParameter = SearchAppBarButton; };
					SearchFavouritesButton.Loaded += (sender2, args2) => { SearchButton.CommandParameter = SearchAppBarButton; };
					TagTextBox = childrenOfDialog.OfType<AutoSuggestBox>().First(x => x.Name == "TagTextBox");

				};
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
			if (sender is GridView grid && grid.SelectionMode == ListViewSelectionMode.Single)
			{

				GlobalInfo.SelectedImage = grid.SelectedIndex;
			}
			Frame.Navigate(typeof(SwipeView));
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
					Frame.Navigate(typeof(SwipeView));
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
			//ViewModel?.RaisePropertyChanged("Thumbnails");


			//ViewModel?.RaisePropertyChanged("FavouriteTags");
			
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
			//ViewModel?.RaisePropertyChanged("SavedForLater");
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
			SaveButton.IsEnabled = false;
			ImageGridView.SelectionChanged -= GridView_MultiSelectChanged;
			ImageGridView.ItemClick += ImageGridView_OnItemClick;
			ImageGridView.SelectionChanged += GridView_SelectionChanged;
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

		private void NoImagestextSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Debug.WriteLine("no image text visibility is " + (sender as TextBlock).Visibility);
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

		public List<Control> AllChildren(DependencyObject parent)
		{
			var _List = new List<Control>();
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var _Child = VisualTreeHelper.GetChild(parent, i);
				if (_Child is Control)
					_List.Add(_Child as Control);
				_List.AddRange(AllChildren(_Child));
			}
			return _List;
		}

		private void SavedSearchesListForReal_OnItemClick(object sender, ItemClickEventArgs e)
		{
			if (SavedSearchInvoke != null || SavedSearchesListForReal != null)
			{
				SavedSearchInvoke.Command.Execute(e.ClickedItem);
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
				var i = SavedSearchesListForReal.Items.IndexOf(list.DataContext);
				SavedSearchInvoke.Command.Execute(SavedSearchesListForReal.Items[i]);
				MainHub.ScrollToSection(SearchResultsSection);
			}
		}

		private void ListViewBase_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
		{
			Debug.WriteLine("Items in saved list changed");
		}
	}
}
