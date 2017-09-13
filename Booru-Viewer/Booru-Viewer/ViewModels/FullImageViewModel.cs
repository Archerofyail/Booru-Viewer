using System.Windows.Input;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Booru_Viewer.ViewModels
{
	public class FullImageViewModel : ViewModelBase
	{
		public string FullImageURL { get; set; }
		public string FullImageWithLoginURL { get; set; }
		public string LargeImageURL { get; set; }
		public string PreviewURL { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		private RelayCommand<string> saveImageCommand;
		public string WebsiteURL { get; set; }
		public FullImageViewModel(string previewURL, string imageURLUrl, string websiteURL, string largeImage = null, int width = 0, int height = 0)
		{
			FullImageURL = imageURLUrl;
			FullImageWithLoginURL = imageURLUrl + "?login=" + BooruAPI.Username + "&api_key=" + BooruAPI.APIKey;
			LargeImageURL = largeImage ?? imageURLUrl;
			PreviewURL = previewURL;
			Width = width;
			Height = height;
			WebsiteURL = websiteURL;
		}

		public ICommand SaveImage => new RelayCommand(SaveImageExec);

		void SaveImageExec()
		{
			saveImageCommand.Execute(LargeImageURL);
		}
	}
}
