using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booru_Viewer.ViewModels
{
	public class ImageModel
	{
		public int id { get; set; }
		public string Preview_File_Url	{ get; set; }
		public string File_Url { get; set; }
		public string Large_File_Url { get; set; }
	}
}
