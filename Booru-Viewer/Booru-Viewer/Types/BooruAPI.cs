using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Web.Http;
using System.Threading.Tasks;
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
		public static async Task<Tuple<bool, List<ImageModel>, string>> SearchPosts(string[] tags, int page, int limit, bool[] ratingChecks = null, bool restartSearch = true)
		{


			if (restartSearch)
			{
				GlobalInfo.CurrentSearch.Clear();
				Page = page;
			}
			List<ImageModel> imageLinks = new List<ImageModel>();
			HttpResponseMessage response = null;
			string tagsAsOne = "";
			foreach (var tag in tags)
			{
				tagsAsOne += tag;
			}
			tagsAsOne += GlobalInfo.CurrentOrdering;
			var contentTags = new[] { "rating:s", "rating:q", "rating:e" };
			if (ratingChecks != null)
			{
				//if all three are checked, no tags. If 2 are checked, minus one. if one is checked add that one
				int trueCount = 0;
				foreach (var b in ratingChecks)
				{
					if (b)
					{
						trueCount++;
					}
				}
				switch (trueCount)
				{
					case 1:
					{
						for (int i = 0; i < ratingChecks.Length; i++)
						{
							if (ratingChecks[i])
							{
								tagsAsOne += contentTags[i];
							}
						}
					}
					break;

					case 2:
					{
						for (int i = 0; i < ratingChecks.Length; i++)
						{
							if (!ratingChecks[i])
							{
								tagsAsOne += "-" + contentTags[i];
							}
						}
					}
					break;
				}
			}
			var variables = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("limit", limit.ToString()),
				new KeyValuePair<string, string>("page", page.ToString()),
				new KeyValuePair<string, string>("tags", tagsAsOne)
			};

			if (APIKey != "" && Username != "")
			{
				variables.Add(new KeyValuePair<string, string>("login", Username));
				variables.Add(new KeyValuePair<string, string>("api_key", APIKey));
			}
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(variables);
			var requestURI =
				new Uri(BaseURL + PostsURL + "?" + content.ToString());
			try
			{


				response = await booruClient.GetAsync(requestURI);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
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

			var signinStuff = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("login", Username),
				new KeyValuePair<string, string>("api_key", APIKey)
			};

			HttpFormUrlEncodedContent signinDetails = new HttpFormUrlEncodedContent(signinStuff);
			int index = 0;
			foreach (var img in imageLinks)
			{
				
				if (img.File_Url == null && img.Preview_File_Url == null && img.Large_File_Url == null)
				{ continue; }
				
				img.File_Url = img.File_Url?.Insert(0, BaseURL);
				img.Preview_File_Url = img.Preview_File_Url?.Insert(0, BaseURL);
				img.Large_File_Url = img.Large_File_Url?.Insert(0, BaseURL);
				GlobalInfo.CurrentSearch.Add(img);
				await ImageSaver.SaveImage(img.File_Url);
				Debug.WriteLine("Image" + index + " is: " + (img.Is_Flagged ? "flagged, " :"") + (img.Is_Pending ? " Pending, " : "") + (img.Is_Deleted ? "Deleted, " : "") + (img.Is_Banned ? "Banned" : ""));
				index++;
			}

			Debug.WriteLine("Finished Adding posts to global search list");

			return new Tuple<bool, List<ImageModel>, string>(true, imageLinks, response.StatusCode.ToString());

		}

		public static async Task<Tuple<bool, List<Tag>, string>> SearchTags(string search, int limit = -1)
		{
			var tags = new List<Tag>();
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("search[name_matches]", search + "*") });
			var requestURI = BaseURL + TagsURL + "?" + content.ToString();
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
			List<Tag> allTags = JsonConvert.DeserializeObject<List<Tag>>(json);

			if (limit != -1)
			{
				for (int i = 0; i < limit && i < allTags.Count; i++)
				{
					allTags[i].Name = allTags[i].Name.Replace("_", " ");
					tags.Add(allTags[i]);
				}
			}
			else
			{
				tags = allTags;
			}
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
