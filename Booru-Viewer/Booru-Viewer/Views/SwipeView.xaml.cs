﻿using System;
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

	

		

		private void FlipView_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			AppBar.Visibility = AppBar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		}

		private void SwipeView_OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			
		}

		private void FlipView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			
		}
	}


}
