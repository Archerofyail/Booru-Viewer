using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Booru_Viewer.Models;
using Booru_Viewer.ViewModels;

namespace Booru_Viewer.Types
{
	public class GroupInfoList : List<TagViewModel>,INotifyCollectionChanged
	{

		public TagType Key { get; set; }

		public GroupInfoList(IList<TagViewModel> list = null)
		{
			if (list != null)
			{
				foreach (var tag in list)
				{
					Add(tag);
				}
			}
		}

		public new void Sort()
		{
			Sort((x, y)=>x.CompareTo(y));
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new void Add(TagViewModel item)
		{
			base.Add(item);
			CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new void AddRange(IEnumerable<TagViewModel> range)
		{
			base.AddRange(range);
			CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;
	}

}
