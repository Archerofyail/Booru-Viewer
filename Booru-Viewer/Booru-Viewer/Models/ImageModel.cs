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
		public string Tag_String_Character { get; set; }
		public string Tag_String_Artist { get; set; }
		public string Tag_String_Copyright { get; set; }
		public string Tag_String_General { get; set; }
		public bool Has_Large { get; set; }
		public bool Is_Pending { get; set; }
		public bool Is_Flagged{ get; set; }
		public bool Is_Deleted { get; set; }
		public bool Is_Banned { get; set; }
		public int image_width { get; set; }
		public int image_height { get; set; }
		[JsonIgnore]
		public string[] GeneralTags
		{
			get
			{
				var tags = Tag_String_General.Split(' ');
				for (var i = 0; i < tags.Length; i++)
				{
					
					tags[i] = tags[i].Replace('_', ' ');
				}

				return tags;
			}
		}
		[JsonIgnore]
		public string[] CharacterTags
		{
			get
			{
				if (Tag_String_Character.Length == 0)
				{ return new string[0]; }
				var tags = Tag_String_Character.Split(' ');
				for (var i = 0; i < tags.Length; i++)
				{

					tags[i] = tags[i].Replace('_', ' ');
				}

				return tags;
			}
		}
		[JsonIgnore]
		public string[] ArtistTags
		{
			get
			{
				if (Tag_String_Artist.Length == 0)
				{ return new string[0]; }
				var tags = Tag_String_Artist.Split(' ');
			
				for (var i = 0; i < tags.Length; i++)
				{

					tags[i] = tags[i].Replace('_', ' ');
				}

				return tags;
			}
		}
		[JsonIgnore]
		public string[] CopyrightTags
		{
			get
			{
				if (Tag_String_Copyright.Length == 0)
				{ return new string[0]; }
				var tags = Tag_String_Copyright.Split(' ');
				
				for (var i = 0; i < tags.Length; i++)
				{

					tags[i] = tags[i].Replace('_', ' ');
				}

				return tags;
			}
		}
	}
}
