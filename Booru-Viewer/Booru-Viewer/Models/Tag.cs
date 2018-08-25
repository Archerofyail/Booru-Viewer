using Booru_Viewer.Types;
using Newtonsoft.Json;

namespace Booru_Viewer.Models
{
	public class Tag
	{
		public string Name { get; set; }
		public TagType category = TagType.Unknown;

		[JsonIgnore]
		public TagType Category
		{
			get
			{
				if (category == TagType.Unknown || category == TagType.General)
				{
					GetTagCategory();
				}
				return category;
			}
			set => category = value;
		}
		public int PostCount { get; set; }
		public Tag(string name, TagType type = TagType.Unknown)
		{
			Name = name;
			category = type;
		}
		async void GetTagCategory()
		{
			var taginf = (await BooruApi.GetTagInfo(Name));
			if (taginf != null)
			{
				category = taginf.category;
			}
			else
			{
				category = TagType.Unknown;
			}
		}
	}



	public enum TagType
	{
		General = 0,
		Artist = 1,
		Copyright = 3,
		Character = 4,
		Unknown = 5,
	}
}
