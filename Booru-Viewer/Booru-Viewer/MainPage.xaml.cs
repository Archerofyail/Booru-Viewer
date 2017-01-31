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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Booru_Viewer
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		private void ImageClicked(object sender, ItemClickEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void SearchClicked(object sender, RoutedEventArgs e)
		{
			SearchFlyout.Hide();
		}

		private void AddTagClicked(object sender, RoutedEventArgs e)
		{
			
		}
	}
}
