using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Booru_Viewer.ViewModels;
using Microsoft.Toolkit.Collections;

namespace Booru_Viewer.Types
{
	public class PostSource : IIncrementalSource<FullImageViewModel>
	{

		private bool useLargerImagePreviews;
		public async Task<IEnumerable<FullImageViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
		{
			if (ApplicationData.Current.RoamingSettings.Values["UseLargerImagesForThumbnails"] != null)
			{
				useLargerImagePreviews = (bool)ApplicationData.Current.RoamingSettings.Values["UseLargerImagesForThumbnails"];
			}
			var page = BooruAPI.Page++;
			if (page == 2 || page == 3)
			{
				await Task.Delay(100 * (page - 1), cancellationToken);
			}
			var result = await BooruAPI.SearchPosts(GlobalInfo.CurrentSearchTags.ToArray(), page, pageSize, GlobalInfo.ContentCheck, false);
			if (!result.Item1) throw new Exception(result.Item3);
			var tns = new List<FullImageViewModel>();
			foreach (var image in result.Item2)
			{
				bool shouldUseLargeImage = image.Has_Large;// && !image.Large_File_Url.EndsWith(".webm");
				tns.Add(new FullImageViewModel(useLargerImagePreviews ? image.File_Url : image.Preview_File_Url,
					shouldUseLargeImage ? image.Large_File_Url : image.File_Url, "https://danbooru.donmai.us/posts/" + image.id,
					image.Large_File_Url, image.image_width, image.image_height));
			}
			


			return tns;


		}
	}
}
