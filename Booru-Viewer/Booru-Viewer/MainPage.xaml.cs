using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Booru_Viewer.Types;
using System.Diagnostics;
using System.Linq;
using Booru_Viewer.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Booru_Viewer
{
	//TODO:Add navigation pane so you can have multiple searches at the same time Going to need to use code to duplicate the views I think
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
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
			this.NavigationCacheMode = NavigationCacheMode.Enabled;
			SearchButton.Loaded += (sender, args) => { SearchButton.CommandParameter = SearchAppBarButton; };
			SearchFavouritesButton.Loaded += (sender, args) => { SearchButton.CommandParameter = SearchAppBarButton; };

			//SavedSearches.Loaded += (sender, args) =>
			//{
			//	SavedSearches.GetBindingExpression(ListView.ItemsSourceProperty).UpdateSource();
			//};
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
			UsernameTextBox.Focus(FocusState.Programmatic);
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

		private void GridView_Tapped(object sender, TappedRoutedEventArgs e)
		{
			Debug.Write("blah");
		}

		private void MultiSelectCommandButtonClicked(object sender, RoutedEventArgs e)
		{

		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{

			ImageGridView.GetBindingExpression(GridView.ItemsSourceProperty).UpdateSource();

			base.OnNavigatedTo(e);
		}


		private void SavedSearchTapped(object sender, TappedRoutedEventArgs e)
		{
			var savedSearch = sender as TextBlock;
			if (savedSearch != null)
			{

			}
		}

		private void SavedSearchSelection(object sender, SelectionChangedEventArgs e)
		{
			var list = sender as ListView;

		}

		private void SettingsTapped(object sender, TappedRoutedEventArgs e)
		{
			Debug.WriteLine("ActualHeight of Grid is: " + RootGrid.ActualHeight + " and page is :" + Page.ActualHeight);
			Debug.Write("Max for slider is " + ImageHeightSlider.Maximum);

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
			
			SavedSearchCommandInvoker.CommandParameter = e.ClickedItem;
		}
	}
}
