namespace Cordial.Models
{
	public class Podcast
	{
		public string Name { get; set; }
		public PodcastStatus Status { get; set; }
		public ulong MB { get; set; }
		public string Location { get; set; }
	}
}
