using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Booru_Viewer.Types
{
	public static class BooruAPI
	{
		public static string BaseURL { get; set; } = "https://Danbooru.donmai.us";
		public static string PostsURL = "/posts.json";
		public static string Username = "";
		public static string APIKey = "";
		private static HttpClient booruClient = new HttpClient();


		//Tags must have a space added to them when they are passed to this function. This returns a null list if failed
		public static async Task<Tuple<bool, List<string>>> GetImageLinksList(string[] tags, int page)
		{
			
			List<string> imageLinks = new List<string>();
			string tagsAsOne = string.Concat(tags);
			var response = await booruClient.GetAsync(new Uri(BaseURL + PostsURL + "?limit=20&page=" + page + "&tags=" + tagsAsOne));
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return new Tuple<bool, List<string>>(false, null);
			}
			var json = await response.Content.ReadAsStringAsync();
			return new Tuple<bool, List<string>>(true, imageLinks);
		}
	}
}
