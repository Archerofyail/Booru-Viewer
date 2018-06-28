using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Web.Http;
using System.Threading.Tasks;
using Booru_Viewer.ViewModels;
using Newtonsoft.Json;
using Booru_Viewer.Models;
using Newtonsoft.Json.Linq;

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
		public static EventHandler UserLookupEvent;


		public static UserModel UserModel;
		//GeneralTags must have a space added to them when they are passed to this function. This returns a null list if failed
		public static async Task<Tuple<bool, List<ImageModel>, string>> SearchPosts(string[] tags, int page, int limit,
			bool[] ratingChecks = null, bool restartSearch = true)
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
			tagsAsOne += GlobalInfo.CurrentOrdering + " ";
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
				return new Tuple<bool, List<ImageModel>, string>(false, null, "Request Failed, check your connection");
			}
			if (response.StatusCode != HttpStatusCode.Ok)
			{

				return new Tuple<bool, List<ImageModel>, string>(false, null, response.StatusCode.ToString());
			}

			var json = await response.Content.ReadAsStringAsync();

			imageLinks = JsonConvert.DeserializeObject<List<ImageModel>>(json);

			var signinStuff = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("login", Username),
				new KeyValuePair<string, string>("api_key", APIKey)
			};

			HttpFormUrlEncodedContent signinDetails = new HttpFormUrlEncodedContent(signinStuff);
			try
			{
				var toRemove = new List<ImageModel>();
				for (var i = 0; i < imageLinks.Count; i++)
				{
					var img = imageLinks[i];
					if (!img.File_Url.EndsWith("mp4") && 
					    !img.File_Url.EndsWith("webm") && 
					    !img.File_Url.EndsWith("zip") &&
						!img.File_Url.EndsWith("swf") &&
					    !img.Large_File_Url.EndsWith("mp4") &&
						!img.Large_File_Url.EndsWith("webm")&& 
					    !img.Large_File_Url.EndsWith("zip") &&
						!img.Large_File_Url.EndsWith("swf"))
					{
						if (i < imageLinks.Count - 2)
						{

							var j = i + 1;
							if (j < imageLinks.Count - 1)
							{
								try
								{
									//if img has same parent id as j's parent id, and both aren't null check
									//if img has same parent id as j's id check
									//if img has same id as j's parent id check
									var parentsSameAndNotNull = (imageLinks[j].Parent_Id == img.Parent_Id && img.Parent_Id != null &&
									                             imageLinks[j].Parent_Id != null);
									while (img.id.ToString() == imageLinks[j].Parent_Id || img.Parent_Id == imageLinks[j].id.ToString() || parentsSameAndNotNull)
									{
										img.ChildrenImages.Add(imageLinks[j]);
										imageLinks.Remove(imageLinks[j]);
										if (j >= imageLinks.Count)
										{
											break;
										}
										parentsSameAndNotNull = (imageLinks[j].Parent_Id == img.Parent_Id && img.Parent_Id != null &&
										                             imageLinks[j].Parent_Id != null);

									}
								}
								catch (Exception e)
								{
									Console.WriteLine(e);
									throw;
								}

							}

						}
						GlobalInfo.CurrentSearch.Add(img);
					}
					else
					{
						toRemove.Add(img);
					}
					
				}
				foreach (var img in toRemove)
				{
					imageLinks.Remove(img);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}



			

			var noDupes = imageLinks.GroupBy(x => x.id).Where(x => x.Count() == 1).Select(x => x.First(y => y.id > 0)).ToList();
			Debug.WriteLine("Page is: " + page + ". URL: " + requestURI);
			return new Tuple<bool, List<ImageModel>, string>(true, noDupes, response.StatusCode.ToString());

		}

		public static async Task<Tuple<bool, List<Tag>, string>> SearchTags(string search, int limit = -1, bool isExact = false)
		{
			var tags = new List<Tag>();
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("search[name" + (isExact ? "]" : "_matches]"), search.ToLower() + (isExact ? "" : "*")),
				new KeyValuePair<string, string>("search[order]", "name"),  });
			var requestURI = BaseURL + TagsURL + "?" + content.ToString();

			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				response = await booruClient.GetAsync(new Uri(requestURI));
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
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

		public static async Task<UserModel> GetUser()
		{
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("search[name]", Username), new KeyValuePair<string, string>("login", Username),
				new KeyValuePair<string, string>("api_key", APIKey),
			});
			var requestURI = BaseURL + "/users.json" + "?" + content.ToString();
			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				response = await booruClient.GetAsync(new Uri(requestURI));
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);

			}
			if (response?.StatusCode != HttpStatusCode.Ok)
			{
				return null;
			}
			var json = await response.Content.ReadAsStringAsync();
			UserModel user = JsonConvert.DeserializeObject<List<UserModel>>(json)[0];
			UserModel = user;
			if (UserLookupEvent != null)
			{
				UserLookupEvent.Invoke(user, null);
			}
			return user;
		}

		public static async Task<bool> UpdateBlacklistedTags(string blacklistedTags)
		{
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("user[blacklisted_tags]", blacklistedTags), });
			var requestURI = BaseURL + "/users/" + UserModel.id + ".json?" + content.ToString();
			return false;
		}

		public static async Task<bool> FavouriteImage(ImageModel im)
		{
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[]{
																			new KeyValuePair<string, string>("post_id", im.id.ToString()),
																			new KeyValuePair<string, string>("login", Username),
																			new KeyValuePair<string, string>("api_key", APIKey),});
			HttpResponseMessage response = new HttpResponseMessage();
			var requestURI = BaseURL + "/favorites.json?" + content.ToString();
			try
			{
				response = await booruClient.PostAsync(new Uri(requestURI), null);
				Debug.WriteLine(await response.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{

				Debug.WriteLine(e.Message);

			}
			if (response == null)
			{
				return false;
			}
			var jobj = JObject.Parse(await response.Content.ReadAsStringAsync());
			var isSuccess = jobj["success"].Value<bool>();
			if (isSuccess)
			{
				return true;
			}

			return false;
		}

		public static async Task<Tag> GetTagInfo(string tagName)
		{
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("search[name]",tagName.Replace(" ", "_")),
			});
			var requestURI = BaseURL + "/tags.json?" + content.ToString();
			var response = await (booruClient.GetAsync(new Uri(requestURI)));
			if (response == null)
			{
				return null;
			}
			var json = await response.Content.ReadAsStringAsync();
			var tag = JsonConvert.DeserializeObject<List<Tag>>(json, new JsonSerializerSettings
			{
				Error = (sender, args) =>
{
	Debug.WriteLine(args.ErrorContext.Error.Message);
	args.ErrorContext.Handled = true;
}
			});
			if (tag.Count >= 1)
			{
				return tag[0];
			}
			return null;
		}

		public static async Task<bool> UnfavouriteImage(ImageModel im)
		{
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("login", Username),
				new KeyValuePair<string, string>("api_key", APIKey),
			});
			var requestURI = BaseURL + "/favorites/" + im.id + ".json?" + content.ToString();
			var response = await booruClient.DeleteAsync(new Uri(requestURI));
			if (response == null)
			{
				return false;
			}
			var jobj = JObject.Parse(await response.Content.ReadAsStringAsync());
			var isSuccess = jobj["success"].Value<bool>();
			if (isSuccess)
			{
				return true;
			}
			return false;
		}

		private static async Task<string> Get(string endpoint, HttpFormUrlEncodedContent parameters)
		{
			var loginInfo = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("login", Username), new KeyValuePair<string, string>("api_key", APIKey) });
			var uri = BaseURL + endpoint + "?" + parameters.ToString() + loginInfo.ToString();
			var response = await booruClient.GetAsync(new Uri(uri));
			if (response == null)
			{
				return null;
			}

			return await response.Content.ReadAsStringAsync();
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
