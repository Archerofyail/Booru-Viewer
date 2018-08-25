namespace Booru_Viewer.Models
{
	public class SettingsData
	{
		public int PerPage { get; set; }
		public string Username { get; set; }
		public string ApiKey { get; set; }
		public bool[] ContentChecks { get; set; }
	}
}
