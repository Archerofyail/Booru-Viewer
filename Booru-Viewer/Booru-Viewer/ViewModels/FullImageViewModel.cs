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
