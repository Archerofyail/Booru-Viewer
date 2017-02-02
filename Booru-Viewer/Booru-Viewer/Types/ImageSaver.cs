using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Booru_Viewer.Types
{
	public static class ImageSaver
	{
		public static StorageFolder imageFolder;
		static ImageSaver()
		{
			imageFolder = KnownFolders.PicturesLibrary.GetFolderAsync("Booru-Viewer").GetResults();
			
		}
		public static void SaveImage(string ImageURL)
		{
			
		}
	}
}
