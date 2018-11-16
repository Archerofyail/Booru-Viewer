using System;
using Windows.Media.Core;
using Windows.UI.Xaml.Data;

namespace Booru_Viewer.Types
{
	class SourceToStreamConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is string source)
			{
				if (source.EndsWith(".mp4"))
				{
					var video = MediaSource.CreateFromUri(new Uri(source));
					return video;
				}

			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
