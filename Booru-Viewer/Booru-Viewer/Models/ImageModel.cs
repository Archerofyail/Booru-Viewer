using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Booru_Viewer.ViewModels
{
	public class ImageModel
	{
		public int id { get; set; }
		public string Preview_File_Url	{ get; set; }
		public string File_Url { get; set; }
		public string Large_File_Url { get; set; }
		public string Tag_String { get; set; }
		[JsonIgnore]
		public string[] Tags
		{
			get
			{
				var tags = Tag_String.Split(' ');
				for (var i = 0; i < tags.Length; i++)
				{
					
					tags[i] = tags[i].Replace('_', ' ');
				}

				return tags;
			}
		}
	}
}
