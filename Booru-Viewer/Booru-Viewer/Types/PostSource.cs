using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Booru_Viewer.ViewModels;
using Microsoft.Toolkit.Uwp;

namespace Booru_Viewer.Types
{
	public class PostSource : IIncrementalSource<ThumbnailViewModel>
	{
		async Task<IEnumerable<ThumbnailViewModel>> IIncrementalSource<ThumbnailViewModel>.GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
		{
			pageIndex++;
			BooruAPI.Page++;
			var result = await BooruAPI.SearchPosts(GlobalInfo.CurrentSearchTags.ToArray(), BooruAPI.Page, pageSize, GlobalInfo.ContentCheck, false);
			if (!result.Item1) throw new Exception(result.Item3);
			var tns = new List<ThumbnailViewModel>();
			foreach (var image in result.Item2)
			{
				tns.Add(new ThumbnailViewModel(image.Preview_File_Url, image.Has_Large ? image.Large_File_Url : image.File_Url));
			}
			return tns;
		}
	}
}
