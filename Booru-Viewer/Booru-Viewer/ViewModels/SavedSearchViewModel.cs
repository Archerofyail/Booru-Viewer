using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Booru_Viewer.Types;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Booru_Viewer.ViewModels
{
	public class SavedSearchViewModel : ViewModelBase
	{

		private MainPageViewModel parentVM;
		public SavedSearchViewModel(string[] tags, MainPageViewModel pVM)
		{
			Tags = tags;
			parentVM = pVM;
		}

		public string AllTags
		{
			get
			{
				var text = "";
				if (Tags.Length > 0)
				{
					foreach (var tag in Tags)
					{
						text += tag + ", ";
					}
					text = text.Substring(0, text.Length - 2);
				}
				return text;
			}
		}

		private string[] searchPreview = null;
		public string[] SearchPreview
		{
			get

			{
				if (searchPreview == null)
				{
					GetPreviews();
				}
				//return new []{ "https://danbooru.donmai.us/data/sample/__d_va_overwatch_drawn_by_piao_qi_jiangjun__sample-773ae26de9f2c223dc9cf4e866ce89e6.jpg"};
				return searchPreview;
			}
			set
			{
				searchPreview = value;
				RaisePropertyChanged();
			}
		}
		public string[] Tags { get; set; }

		void DeleteSearchExecute()
		{

			parentVM.DeleteSavedSearch(this);
		}

		void StartSearchExecute()
		{
			parentVM.StartSavedSearch(Tags);
		}

		async void GetPreviews()
		{
			List<string> preppedTags = new List<string>(Tags.Length);
			Debug.WriteLine("Getting previews for Search: " + AllTags);



			var i = 0;
			foreach (var tag in Tags)
			{
				if (!string.IsNullOrEmpty(tag))
				{
					preppedTags.Add(tag.Replace(" ", "_") + " ");

				}
				i++;
			}
			preppedTags.Add("order:score");

			var imageResults = (await BooruAPI.SearchPosts(preppedTags.ToArray(), 1, 5)).Item2.Select(x => x.Preview_File_Url).ToArray();
			SearchPreview = imageResults;
			Debug.WriteLine("Got all images");
			
		}
		public ICommand StartSearch => new RelayCommand(StartSearchExecute);
		public ICommand DeleteSearch => new RelayCommand(DeleteSearchExecute);
	}
}
