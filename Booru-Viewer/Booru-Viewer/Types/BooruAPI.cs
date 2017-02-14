using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Web.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments.DataProvider;
using Booru_Viewer.ViewModels;
using Newtonsoft.Json;
using Booru_Viewer.Models;

namespace Booru_Viewer.Types
{
	public static class BooruAPI
	{
		public static string BaseURL { get; private set; } = "https://Danbooru.donmai.us";
		public static string PostsURL = "/posts.json";
		public static string TagsURL = "/tags.json";
		public static string Username { get; private set; } = "";
		public static string APIKey { get; private set; } = "";
		public static int Page { get; set; } = 1;
		private static HttpClient booruClient = new HttpClient();
		public static EventHandler<Tuple<bool, List<Tag>, string>> TagSearchCompletedHandler;

		//Tags must have a space added to them when they are passed to this function. This returns a null list if failed
		public static async Task<Tuple<bool, List<ImageModel>, string>> SearchPosts(string[] tags, int page, bool restartSearch = true)
		{
			List<ImageModel> imageLinks = new List<ImageModel>();
			HttpResponseMessage response = null;
			string tagsAsOne = "";
			foreach (var tag in tags)
			{
				tagsAsOne += tag.Replace(" ", "%20");
			}

			var requestURI =
				new Uri(BaseURL + PostsURL + "?limit=20&page=" + page + "&tags=" + tagsAsOne +
						(APIKey != "" && Username != "" ? "&login=" + Username + "&api_key=" + APIKey : ""));
			try
			{


				response = await booruClient.GetAsync(requestURI);
			}
			catch (Exception e)
			{

			}
			if (response == null)
			{
				return new Tuple<bool, List<ImageModel>, string>(false, null, "Request Failed");
			}
			if (response.StatusCode != HttpStatusCode.Ok)
			{
				
				return new Tuple<bool, List<ImageModel>, string>(false, null, response.StatusCode.ToString());
			}

			var json = await response.Content.ReadAsStringAsync();

			Debug.WriteLine("Got Json:\n" + json);
			imageLinks = JsonConvert.DeserializeObject<List<ImageModel>>(json);
			if (restartSearch)
			{
				GlobalInfo.CurrentSearch.Clear();
				Page = 1;
			}

			foreach (var img in imageLinks)
			{
				GlobalInfo.CurrentSearch.Add(img);
			}
			return new Tuple<bool, List<ImageModel>, string>(true, imageLinks, response.StatusCode.ToString());
		}

		public static async Task<Tuple<bool, List<Tag>, string>>  SearchTags(string search)
		{
			var tags = new List<Tag>();
			var requestURI = BaseURL + TagsURL + "?search[name_matches]=" + search + "*";
			HttpResponseMessage response;
			try
			{
				response = await booruClient.GetAsync(new Uri(requestURI));
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				throw;
			}

			if (response == null)
			{
				return new Tuple<bool, List<Tag>, string>(false, null, "Request Failed");
			}
			if (response.StatusCode != HttpStatusCode.Ok)
			{
				var data = new Tuple<bool, List<Tag>, string>(false, null, response.ReasonPhrase);
				return data;
			}
			var json = await response.Content.ReadAsStringAsync();
			tags = JsonConvert.DeserializeObject<List<Tag>>(json);

			TagSearchCompletedHandler?.Invoke(typeof(BooruAPI), new Tuple<bool, List<Tag>, string>(true, tags, response.ReasonPhrase));
			return new Tuple<bool, List<Tag>, string>(true, tags, response.ReasonPhrase);
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
