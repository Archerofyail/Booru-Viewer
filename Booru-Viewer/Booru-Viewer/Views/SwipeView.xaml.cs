using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Booru_Viewer.Types;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Booru_Viewer.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SwipeView : Page
	{
		public SwipeView()
		{
			this.InitializeComponent();

			FlipView.Loaded += (sender, args) =>
			{
				FlipView.SelectedIndex = GlobalInfo.SelectedImage;
			};
			FlipView.SelectionChanged += (sender, args) =>
			{
				if (FlipView.Items.Count - FlipView.SelectedIndex < 2)
				{

				}
			};

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

		private void ScrollViewDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var view = sender as ScrollViewer;
			e.Handled = true;
			view.HorizontalScrollMode = ScrollMode.Disabled;
			view.VerticalScrollMode = ScrollMode.Disabled;
			if (view.ZoomFactor <= 1)
			{
				Debug.WriteLine(view.ChangeView(view.ScrollableWidth / 2, view.ScrollableHeight / 2, 15, false)
					? "View Changed"
					: "View not changed");
			}

			view.HorizontalScrollMode = ScrollMode.Enabled;
			view.VerticalScrollMode = ScrollMode.Enabled;

		}

		private void ImageFailedToOpen(object sender, ImageExFailedEventArgs e)
		{
			Debug.WriteLine("Image failed to open: " + e.ErrorMessage + ",\nexception: " + e.ErrorException.Message);

		}

		private async void ImageOpened(object sender, ImageExOpenedEventArgs e)
		{
			var img = sender as ImageEx;
			Debug.WriteLine(FlipView.Items.IndexOf((sender as ImageEx).DataContext) + ": Image Width and height is " + img.ActualWidth + "x" + img.ActualHeight);
			await Page.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Page.UpdateLayout());
			img.Width = (img.Source as BitmapImage).PixelWidth;
			img.Height = (img.Source as BitmapImage).PixelHeight;

		}


		private void BitmapImage_OnImageOpened(object sender, RoutedEventArgs e)
		{
			var str = (sender as BitmapImage).UriSource.ToString();
			var img = sender as BitmapImage;

			Debug.WriteLine("Image Opened " + "Pixel width and height are " + img.PixelWidth + "x" + img.PixelHeight);
		}

		private void BitmapImage_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
			Debug.WriteLine("Failed to open image: " + e.ErrorMessage);
		}

		private void BitmapImage_OnDownloadProgress(object sender, DownloadProgressEventArgs e)
		{
			if (sender as BitmapImage == FlipView.ContainerFromIndex(FlipView.SelectedIndex).GetValue(ImageEx.SourceProperty))
			{
				Debug.WriteLine("PRogress added from index");
				if (Progress.Visibility == Visibility.Collapsed)
				{
					Progress.Visibility = Visibility.Visible;

				}
				Progress.IsIndeterminate = false;
				Progress.Value = e.Progress;
				if (e.Progress == 100)
				{
					Progress.Visibility = Visibility.Collapsed;
				}
			}
			else
			{

			}
		}

		private void FlipView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Debug.WriteLine("Changed");
			Debug.WriteLine("Page actual height and width are:" + Page.ActualHeight + "x" + Page.ActualWidth);
				
		}

		private void FlipView_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			AppBar.Visibility = AppBar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		}
	}


}
