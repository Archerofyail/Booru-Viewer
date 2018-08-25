using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Booru_Viewer.Views
{

	public sealed partial class SavedForLaterSwipeView : Page
	{
		public SavedForLaterSwipeView()
		{
			this.InitializeComponent();


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
		}

		private void FlipView_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			
		}

		private void FlipView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void ImageEx_OnImageExFailed(object sender, ImageExFailedEventArgs e)
		{
			Debug.WriteLine("Failed to open image: " + e.ErrorMessage);
			if (sender is ImageEx img)
			{
				if (img.Source.ToString().EndsWith(".webm") || img.Source.ToString().EndsWith(".mp4"))
				{
					return;
				}

				var source = img.Source;

				img.Source = "";
				img.Source = source;
			}
		}

		private async void ImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			e.Handled = true;
			var scrollViewer = sender as ScrollViewer;
			var doubleTapPoint = e.GetPosition(scrollViewer);

			if (scrollViewer.ZoomFactor != 1f)
			{
				scrollViewer.ChangeView(null, null, 1);
			}
			else if (scrollViewer.ZoomFactor == 1f)
			{
				scrollViewer.ChangeView(null, null, 2);

				var dispatcher = Window.Current.CoreWindow.Dispatcher;
				await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
				{
					scrollViewer.ChangeView(doubleTapPoint.X, doubleTapPoint.Y, null);
				});
			}
		}

		private void ImageSelectClick(object sender, RoutedEventArgs e)
		{
			
		}

		private void ImageTapped(object sender, TappedRoutedEventArgs e)
		{
			AppBar.Visibility = AppBar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		}
	}


}
