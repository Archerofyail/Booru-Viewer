using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Booru_Viewer.Types;
using Windows.Storage;
using Windows.System.Profile;
using System.Diagnostics;
using Booru_Viewer.Views;

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
			//SavedSearches.Loaded += (sender, args) =>
			//{
			//	SavedSearches.GetBindingExpression(ListView.ItemsSourceProperty).UpdateSource();
			//};
		}


		private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var grid = sender as GridView;
			if (grid != null)
			{

				GlobalInfo.SelectedImage = grid.SelectedIndex;
			}
			Frame.Navigate(typeof(SwipeView));
		}

		private void SearchClicked(object sender, RoutedEventArgs e)
		{
			SearchFlyout.Hide();
		}

		private void AddTagClicked(object sender, RoutedEventArgs e)
		{
			//AddTagButton.Focus(FocusState.Programmatic);
			TagTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void SaveLoginDataButtonTapped(object sender, TappedRoutedEventArgs e)
		{
			UsernameTextBox.Focus(FocusState.Programmatic);
			APIKeyTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void TagTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
	}
}
