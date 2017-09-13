using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Booru_Viewer.Types
{
	public class CountToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			int count = 0;
			if (value is int i)
			{
				count = i;
			}
			return count == 0 ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}
