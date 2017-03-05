﻿using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Booru_Viewer.Types;

namespace Booru_Viewer.ViewModels
{
	public class ThumbnailViewModel
	{

		public ThumbnailViewModel(string prevUrl, string fullUrl)
		{
			PreviewURL = prevUrl;
			FullURL = fullUrl;
		}
		public string PreviewURL { get; set; }
		public string FullURL { get; set; }
		void SaveImageExecute()
		{
			ImageSaver.SaveImage(FullURL);
		}
		

		void EnableMultiSelectExecute()
		{
			//parentVM.ImageSelectionMode = ListViewSelectionMode.Multiple;
		}
		

		public ICommand SaveImage { get { return new RelayCommand(SaveImageExecute); } }
		public ICommand EnableMultiSelect { get { return new RelayCommand(EnableMultiSelectExecute); } }
	}
}
