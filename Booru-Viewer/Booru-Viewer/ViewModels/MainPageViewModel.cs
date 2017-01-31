using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Booru_Viewer.Types;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace Booru_Viewer.ViewModels
{
	class MainPageViewModel
	{
		public ObservableCollection<string> ImageLinks { get { return GlobalInfo.ImageURLs; } }
	}
}
