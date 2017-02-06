using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Booru_Viewer.ViewModels
{
	class FullImageViewModel : ViewModelBase
	{
		public string FullImage { get; set; }

		public FullImageViewModel(string imageUrl)
		{
			FullImage = imageUrl;
		}
	}
}
