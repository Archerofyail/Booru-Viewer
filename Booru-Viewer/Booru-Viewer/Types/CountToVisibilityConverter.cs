using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Booru_Viewer.Types
{
	class CountToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			int count = 0;
			if (value is int)
			{
				count = (int) value;
			}
			return count == 0 ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}
