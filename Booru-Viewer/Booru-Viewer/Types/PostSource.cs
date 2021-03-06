﻿using System;
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

		private bool _useLargerImagePreviews;
		public async Task<IEnumerable<FullImageViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
		{
			if (ApplicationData.Current.RoamingSettings.Values["UseLargerImagesForThumbnails"] != null)
			{
				_useLargerImagePreviews = (bool)ApplicationData.Current.RoamingSettings.Values["UseLargerImagesForThumbnails"];
			}
			var page = BooruApi.Page++;
			if (page == 2 || page == 3)
			{
				await Task.Delay(300 * (page - 1), cancellationToken);
			}
			var result = await BooruApi.SearchPosts(GlobalInfo.CurrentSearchTags.ToArray(), page, pageSize, GlobalInfo.ContentCheck, false);
			if (!result.Item1) throw new Exception(result.Item3);
			var tns = new List<FullImageViewModel>();
			foreach (var image in result.Item2)
			{
				bool shouldUseLargeImage = image.Has_Large; // && !image.Large_File_Url.EndsWith(".webm");
				tns.Add(new FullImageViewModel(image, image.id, _useLargerImagePreviews ? image.File_Url : image.Preview_File_Url,
					shouldUseLargeImage ? image.Large_File_Url : image.File_Url, "https://danbooru.donmai.us/posts/" + image.id, image.ChildrenImages, null,
					image.Large_File_Url));

			}



			return tns;


		}
	}
}
