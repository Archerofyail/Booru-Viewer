using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Web.Http;
using System.Text;
using System.Threading.Tasks;
using Booru_Viewer.ViewModels;
using Newtonsoft.Json;

namespace Booru_Viewer.Types
{
	public static class BooruAPI
	{
		public static string BaseURL { get; private set; } = "https://Danbooru.donmai.us";
		public static string PostsURL = "/posts.json";
		public static string Username { get; private set; } = "";
		public static string APIKey { get; private set; } = "";
		public static int Page { get; set; } = 1;
		private static HttpClient booruClient = new HttpClient();


		//Tags must have a space added to them when they are passed to this function. This returns a null list if failed
		public static async Task<Tuple<bool, List<ImageModel>, HttpStatusCode>> SearchPosts(string[] tags, int page, bool restartSearch = true)
		{
			List<ImageModel> imageLinks = new List<ImageModel>();
			
			string tagsAsOne = "";
			foreach (var tag in tags)
			{
				tagsAsOne += tag.Replace(" ", "%20");
			}
			
			var requestURI =
				new Uri(BaseURL + PostsURL + "?limit=20&page=" + page + "&tags=" + tagsAsOne +
				        (APIKey != "" && Username != "" ? "&login=" + Username + "&api_key=" + APIKey : ""));
			var response = await booruClient.GetAsync(requestURI);
			if (response.StatusCode != HttpStatusCode.Ok)
			{
				return new Tuple<bool, List<ImageModel>, HttpStatusCode>(false, null, response.StatusCode);
			}
			var json = await response.Content.ReadAsStringAsync();
			Debug.WriteLine("Got Json:\n" + json);
			imageLinks = JsonConvert.DeserializeObject<List<ImageModel>>(json);
			if (restartSearch)
			{
				GlobalInfo.CurrentSearch.Clear();
			}
			
			foreach (var img in imageLinks)
			{
				GlobalInfo.CurrentSearch.Add(img);
			}
			return new Tuple<bool, List<ImageModel>, HttpStatusCode>(true, imageLinks, response.StatusCode);
		}

		public static void SetLogin(string username, string APIKey)
		{
			Username = username;
			BooruAPI.APIKey = APIKey;
		}

		public static void ChangeWebsite(string newBaseURL)
		{
			BaseURL = newBaseURL;
		}
	}
}
