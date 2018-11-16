using System.Collections.Generic;
using Booru_Viewer.Models;
using Newtonsoft.Json;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Booru_Viewer.ViewModels
{
	public class ImageModel
	{
		public string id { get; set; }
		public string Preview_File_Url { get; set; }
		public string File_Url { get; set; }
		public string Large_File_Url { get; set; }
		public string Tag_String { get; set; }
		public string Tag_String_Character { get; set; }
		public string Tag_String_Artist { get; set; }
		public string Tag_String_Copyright { get; set; }
		public string Tag_String_General { get; set; }
		public string Tag_String_Meta { get; set; }
		public bool Has_Large { get; set; }
		public bool Is_Pending { get; set; }
		public bool Is_Flagged { get; set; }
		public bool Is_Deleted { get; set; }
		public bool Is_Banned { get; set; }
		public int image_width { get; set; }
		public int image_height { get; set; }
		public bool Has_Children { get; set; }
		public string Parent_Id { get; set; }

		[JsonIgnore] public List<ImageModel> ChildrenImages = new List<ImageModel>();

		public string rating { get; set; }
		[JsonIgnore]
		public string Rating
		{
			get
			{
				switch (rating)
				{
					case "s":
					return "Safe";
					case "q":
					return "Questionable";
					case "e":
					return "Explicit";
					default:
					return "Safe";
				}
			}

		}

		[JsonIgnore] private List<Tag> generalTags = new List<Tag>();
		[JsonIgnore]
		public List<Tag> GeneralTags
		{
			get
			{
				if (Tag_String_General.Length == 0)
				{ return new List<Tag>(); }

				if (generalTags.Count == 0)
				{
					var tags = Tag_String_General.Split(' ');
					for (var i = 0; i < tags.Length; i++)
					{
						tags[i] = tags[i].Replace('_', ' ');
						generalTags.Add(new Tag(tags[i]));
					}
					
				}
				return generalTags;
			}
		}
		[JsonIgnore] private List<Tag> characterTags = new List<Tag>();
		[JsonIgnore]
		public List<Tag> CharacterTags
		{
			get
			{
				if (Tag_String_Character.Length == 0)
				{ return new List<Tag>(); }

				if (characterTags.Count == 0)
				{
					var tags = Tag_String_Character.Split(' ');
					for (var i = 0; i < tags.Length; i++)
					{
						tags[i] = tags[i].Replace('_', ' ');
						characterTags.Add(new Tag(tags[i]));
					}
					
				}

				return characterTags;
			}
		}
		[JsonIgnore] private List<Tag> artistTags = new List<Tag>();
		[JsonIgnore]
		public List<Tag> ArtistTags
		{
			get
			{
				if (Tag_String_Artist.Length == 0)
				{ return new List<Tag>(); }

				if (artistTags.Count == 0)
				{
					var tags = Tag_String_Artist.Split(' ');
					for (var i = 0; i < tags.Length; i++)
					{
						tags[i] = tags[i].Replace('_', ' ');
						artistTags.Add(new Tag(tags[i]));
					}
					
				}

				return artistTags;
			}
		}
		[JsonIgnore] private List<Tag> copyrightTags = new List<Tag>();
		[JsonIgnore]
		public List<Tag> CopyrightTags
		{
			get
			{
				if (Tag_String_Copyright.Length == 0)
				{ return new List<Tag>(); }

				if (copyrightTags.Count == 0)
				{
					var tags = Tag_String_Copyright.Split(' ');
					for (var i = 0; i < tags.Length; i++)
					{
						tags[i] = tags[i].Replace('_', ' ');
						copyrightTags.Add(new Tag(tags[i]));
					}
					
				}

				return copyrightTags;
			}
		}
		[JsonIgnore] private List<Tag> metaTags = new List<Tag>();
		[JsonIgnore]
		public List<Tag> MetaTags
		{
			get
			{
				if (Tag_String_Meta.Length == 0)
				{ return new List<Tag>(); }

				if (metaTags.Count == 0)
				{
					var tags = Tag_String_Meta.Split(' ');
					for (var i = 0; i < tags.Length; i++)
					{
						tags[i] = tags[i].Replace('_', ' ');
						metaTags.Add(new Tag(tags[i]));
					}
					
				}

				return metaTags;
			}
		}
	}
}
