using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booru_Viewer.Models
{
	public class SettingsData
	{
		public int PerPage { get; set; }
		public string Username { get; set; }
		public string APIKey { get; set; }
		public bool[] contentChecks { get; set; }
	}
}
