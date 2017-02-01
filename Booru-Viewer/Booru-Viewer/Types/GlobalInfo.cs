using Booru_Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booru_Viewer.Types
{
	public static class GlobalInfo
	{
		public static ObservableCollection<ImageModel> CurrentSearch { get; set; } = new ObservableCollection<ImageModel>();
		public static ObservableCollection<TagViewModel> CurrentTags { get; set; } = new ObservableCollection<TagViewModel>();

		public static void RemoveTag(TagViewModel tag)
		{
			CurrentTags.Remove(tag);
		}
	}
}
