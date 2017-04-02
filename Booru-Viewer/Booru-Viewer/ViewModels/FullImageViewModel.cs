using GalaSoft.MvvmLight;

namespace Booru_Viewer.ViewModels
{
	class FullImageViewModel : ViewModelBase
	{
		public string FullImage { get; set; }

		public string LargeImage { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public FullImageViewModel(string imageUrl, string largeImage = null, int width = 0, int height = 0)
		{
			FullImage = imageUrl;
			LargeImage = largeImage ?? imageUrl;
			Width = width;
			Height = height;
		}
	}
}
