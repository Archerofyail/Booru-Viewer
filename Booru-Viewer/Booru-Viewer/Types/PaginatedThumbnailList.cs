using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Booru_Viewer.ViewModels;

namespace Booru_Viewer.Types
{
	public class PaginatedThumbnailList : ObservableCollection<ThumbnailViewModel>, ISupportIncrementalLoading
	{

		public delegate void LoadMoreDelegate();

		public LoadMoreDelegate LoadMoreItemsDelegate;
		public Func<uint, Task<IEnumerable<ImageModel>>> load;
		private bool _busy;
		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
		{
			if (_busy)
			{
				return AsyncInfo.Run(async c => new LoadMoreItemsResult() { Count = 0 });
			}
			_busy = true;
			return AsyncInfo.Run(x => LoadItems());
		}

		async Task<LoadMoreItemsResult> LoadItems()
		{

			uint count = 0;
			var items = await load(0);
			foreach (var item in items)
			{
				Add(new ThumbnailViewModel(item.Preview_File_Url, item.Large_File_Url));
			}

			Debug.WriteLine("Finished loading images");
			_busy = false;
			count = Convert.ToUInt32(items.Count());
			return new LoadMoreItemsResult { Count = count };
		}

		public bool HasMoreItems { get; set; } = true;
	}
}
