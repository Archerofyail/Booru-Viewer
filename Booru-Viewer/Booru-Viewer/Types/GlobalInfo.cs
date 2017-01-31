using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booru_Viewer.Types
{
	public static class GlobalInfo
	{
		public static ObservableCollection<string> ImageURLs { get; set; } = new ObservableCollection<string>();
		public static ObservableCollection<string> CurrentTags { get; set; } = new ObservableCollection<string>();
	}
}
