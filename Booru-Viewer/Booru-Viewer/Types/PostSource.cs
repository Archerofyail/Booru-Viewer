using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Booru_Viewer.ViewModels;
using Microsoft.Toolkit.Collections;

namespace Booru_Viewer.Types
{
	public class PostSource : IIncrementalSource<FullImageViewModel>
	{
		public async Task<IEnumerable<FullImageViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
		{

			var page = BooruAPI.Page++;
			var result = await BooruAPI.SearchPosts(GlobalInfo.CurrentSearchTags.ToArray(), page, pageSize, GlobalInfo.ContentCheck, false);
			if (!result.Item1) throw new Exception(result.Item3);
			var tns = new List<FullImageViewModel>();
			foreach (var image in result.Item2)
			{
				tns.Add(new FullImageViewModel(image.Preview_File_Url, image.Has_Large ? image.Large_File_Url : image.File_Url, "https://danbooru.donmai.us/posts/" + image.id, image.Large_File_Url, image.image_width, image.image_height));
			}


			return tns;


		}
	}
}
