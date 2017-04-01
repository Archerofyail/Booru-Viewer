using GalaSoft.MvvmLight;

namespace Booru_Viewer.ViewModels
{
	class FullImageViewModel : ViewModelBase
	{
		public string FullImage { get; set; }

		public string LargeImage { get; set; }

		public FullImageViewModel(string imageUrl, string largeImage = null)
		{
			FullImage = imageUrl;
			LargeImage = largeImage ?? imageUrl;
		}
	}
}
