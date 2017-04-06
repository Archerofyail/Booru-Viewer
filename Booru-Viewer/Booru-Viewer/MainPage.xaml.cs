using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Booru_Viewer.Types;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;
using Booru_Viewer.ViewModels;
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
		private MainPageViewModel ViewModel { get; set; }
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

			SavedSearchesList.Loaded += (sender, args) =>
			{
				SavedSearchesList.GetBindingExpression(ListView.ItemsSourceProperty).UpdateSource();
			};
			ViewModel = DataContext as MainPageViewModel;
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

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{

			ImageGridView.GetBindingExpression(GridView.ItemsSourceProperty).UpdateSource();

			base.OnNavigatedTo(e);
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
			
			SavedSearchCommandInvoker.CommandParameter = e.ClickedItem;
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
	}
}
