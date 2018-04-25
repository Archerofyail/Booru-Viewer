using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Booru_Viewer.Types
{
	class SourceToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string source = value as string;
			bool? isPictureElement = bool.Parse(parameter.ToString().ToLower());
			if (isPictureElement != null)
			{
				if (source == null)
				{
					return Visibility.Collapsed;
				}

				if ((source.EndsWith("webm") || source.EndsWith("mp4")) && (bool)!isPictureElement)
				{
					return Visibility.Visible;
				}

				if ((source.EndsWith("webm") || source.EndsWith("mp4")) && (bool)isPictureElement)
				{
					return Visibility.Collapsed;
				}

				if ((!source.EndsWith("webm")  && !source.EndsWith("mp4")) && (bool)!isPictureElement)
				{
					return Visibility.Collapsed;
				}


			}
			return Visibility.Visible;

		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
