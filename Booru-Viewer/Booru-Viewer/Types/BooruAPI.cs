using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Web.Http;
using System.Threading.Tasks;
using Booru_Viewer.ViewModels;
using Newtonsoft.Json;
using Booru_Viewer.Models;
using Microsoft.HockeyApp;

namespace Booru_Viewer.Types
{
	public static class BooruApi
	{
		public static string BaseUrl { get; private set; } = "https://Danbooru.donmai.us";
		public static string PostsUrl = "/posts.json";
		public static string TagsUrl = "/tags.json";
		public static string Username { get; private set; } = "";
		public static string ApiKey { get; private set; } = "";
		public static int Page { get; set; } = 1;
		private static HttpClient _booruClient = new HttpClient();
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

			if (ApiKey != "" && Username != "")
			{
				variables.Add(new KeyValuePair<string, string>("login", Username));
				variables.Add(new KeyValuePair<string, string>("api_key", ApiKey));
			}
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(variables);
			var requestUri =
				new Uri(BaseUrl + PostsUrl + "?" + content.ToString());
			try
			{


				response = await _booruClient.GetAsync(requestUri);
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

			imageLinks = JsonConvert.DeserializeObject<List<ImageModel>>(json, new JsonSerializerSettings
			{
				Error =
				(sender, args) =>
				{
					Debug.WriteLine("Error while deserializing image search results: " + args.ErrorContext.Error.Message);
					args.ErrorContext.Handled = true;
					HockeyClient.Current.TrackException(args.ErrorContext.Error);
				}
			});

			var signinStuff = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("login", Username),
				new KeyValuePair<string, string>("api_key", ApiKey)
			};

			HttpFormUrlEncodedContent signinDetails = new HttpFormUrlEncodedContent(signinStuff);
			try
			{
				var toRemove = new List<ImageModel>();
				for (var i = 0; i < imageLinks.Count; i++)
				{

					var img = imageLinks[i];
					if (img.File_Url == null && img.Large_File_Url == null)
					{
						toRemove.Add(img);
						continue;
					}

					if (!img.File_Url.EndsWith("mp4") &&
						!img.File_Url.EndsWith("webm") &&
						!img.File_Url.EndsWith("zip") &&
						!img.File_Url.EndsWith("swf") &&
						!img.Large_File_Url.EndsWith("mp4") &&
						!img.Large_File_Url.EndsWith("webm") &&
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
									while (img.id == imageLinks[j].Parent_Id || img.Parent_Id == imageLinks[j].id || parentsSameAndNotNull)
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

			var noDupes = imageLinks.GroupBy(x => x.id).Where(x => x.Count() == 1).Select(x => x.First(y => !string.IsNullOrEmpty(y.id))).ToList();
			Debug.WriteLine("Page is: " + page + ". URL: " + requestUri);
			return new Tuple<bool, List<ImageModel>, string>(true, noDupes, response.StatusCode.ToString());

		}

		public static async Task<Tuple<bool, List<Tag>, string>> SearchTags(string search, int limit = -1, bool isExact = false)
		{
			var tags = new List<Tag>();
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("search[name" + (isExact ? "]" : "_matches]"), search.ToLower() + (isExact ? "" : "*")),
				new KeyValuePair<string, string>("search[order]", "name"),  });
			var requestUri = BaseUrl + TagsUrl + "?" + content.ToString();

			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				response = await _booruClient.GetAsync(new Uri(requestUri));
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
			List<Tag> allTags = JsonConvert.DeserializeObject<List<Tag>>(json, new JsonSerializerSettings
			{
				Error =
				(sender, args) =>
				{
					Debug.WriteLine("Error while deserializing list of tags: " + args.ErrorContext.Error.Message);
					args.ErrorContext.Handled = true;
					HockeyClient.Current.TrackException(args.ErrorContext.Error);
				}
			});

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
			TagSearchCompletedHandler?.Invoke(typeof(BooruApi), new Tuple<bool, List<Tag>, string>(true, tags, response.ReasonPhrase));
			return new Tuple<bool, List<Tag>, string>(true, tags, response.ReasonPhrase);
		}

		public static async Task<UserModel> GetUser()
		{
			if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(ApiKey))
			{
				return UserModel;
			}

			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[]
				{
				new KeyValuePair<string, string>("search[name]", Username), new KeyValuePair<string, string>("login", Username),
				new KeyValuePair<string, string>("api_key", ApiKey),
			});
			var requestUri = BaseUrl + "/users.json" + "?" + content.ToString();
			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				response = await _booruClient.GetAsync(new Uri(requestUri));
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
			UserModel user = JsonConvert.DeserializeObject<List<UserModel>>(json, new JsonSerializerSettings
			{
				Error =
				(sender, args) =>
				{
					Debug.WriteLine("Error while deserializing user: " + args.ErrorContext.Error.Message);
					args.ErrorContext.Handled = true;
					HockeyClient.Current.TrackException(args.ErrorContext.Error);
				}
			})[0];
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
			var requestUri = BaseUrl + "/users/" + UserModel.id + ".json?" + content.ToString();
			return false;
		}

		public static async Task<bool> FavouriteImage(ImageModel im)
		{
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[]{
																			new KeyValuePair<string, string>("post_id", im.id),
																			new KeyValuePair<string, string>("login", Username),
																			new KeyValuePair<string, string>("api_key", ApiKey),});
			HttpResponseMessage response = new HttpResponseMessage();
			var requestUri = BaseUrl + "/favorites.json?" + content.ToString();
			try
			{
				response = await _booruClient.PostAsync(new Uri(requestUri), null);
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

			var responseData = await response.Content.ReadAsStringAsync();
			if (responseData != null)
			{
				var jobj = JsonConvert.DeserializeObject<ImageModel>(responseData, new JsonSerializerSettings
				{
					Error =
					(sender, args) =>
					{
						Debug.WriteLine("Error while deserializing image: " + args.ErrorContext.Error.Message);
						args.ErrorContext.Handled = true;
						HockeyClient.Current.TrackException(args.ErrorContext.Error);
					}
				});
				var isSuccess = jobj.id == im.id;

				if (isSuccess)
				{
					return true;
				}
			}

			return false;
		}

		public static async Task<Tag> GetTagInfo(string tagName)
		{
			HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("search[name]",tagName.Replace(" ", "_")),
			});
			var requestUri = BaseUrl + "/tags.json?" + content.ToString();
			var response = await (_booruClient.GetAsync(new Uri(requestUri)));
			if (response == null)
			{
				return null;
			}
			var json = await response.Content.ReadAsStringAsync();
			var tag = JsonConvert.DeserializeObject<List<Tag>>(json, new JsonSerializerSettings
			{
				Error =
				(sender, args) =>
				{
					Debug.WriteLine("Error while deserializing tag: " + args.ErrorContext.Error.Message);
					args.ErrorContext.Handled = true;
					HockeyClient.Current.TrackException(args.ErrorContext.Error);
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
				new KeyValuePair<string, string>("api_key", ApiKey),
			});
			var requestUri = BaseUrl + "/favorites/" + im.id + ".json?" + content.ToString();
			var response = await _booruClient.DeleteAsync(new Uri(requestUri));
			if (response == null)
			{
				return false;
			}

			var json = await response.Content.ReadAsStringAsync();
			if (json != null)
			{
				return true;
			}
			return false;
		}

		private static async Task<string> Get(string endpoint, HttpFormUrlEncodedContent parameters)
		{
			var loginInfo = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("login", Username), new KeyValuePair<string, string>("api_key", ApiKey) });
			var uri = BaseUrl + endpoint + "?" + parameters.ToString() + loginInfo.ToString();
			var response = await _booruClient.GetAsync(new Uri(uri));
			if (response == null)
			{
				return null;
			}

			return await response.Content.ReadAsStringAsync();
		}

		public static void SetLogin(string username, string apiKey)
		{
			Username = username;
			BooruApi.ApiKey = apiKey;
		}

		public static void ChangeWebsite(string newBaseUrl)
		{
			BaseUrl = newBaseUrl;
		}

		public static async Task<List<ImageModel>> GetUserFavourites()
		{
			List<ImageModel> favourites = new List<ImageModel>();
			int index = 1;
			int imageReturnCount = 20;
			bool caughtUp = false;
			Debug.WriteLine("favourite images count before adding is " + GlobalInfo.FavouriteImages.Count);
			do
			{
				var images = (await SearchPosts(new[] { "ordfav:archerofyail" }, index++, 100)).Item2;
				imageReturnCount = images.Count;
				foreach (var img in images)
				{
					if (GlobalInfo.FavouriteImages.Contains(img.id))
					{
						caughtUp = true;
						break;
					}
					favourites.Add(img);
				}

				Debug.WriteLine("Downloaded page " + index + "of favourites");

			} while ((index - 2) * 100 < UserModel.favorite_count && !caughtUp);


			return favourites;
		}

	}
}
