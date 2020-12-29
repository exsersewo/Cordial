using System;

namespace Cordial.Models
{
	public class RSSFeed
	{
		public string Name { get; set; }
		public Uri URL { get; set; }

		public RSSFeed() { }

		public RSSFeed(string name, Uri url)
		{
			Name = name;
			URL = url;
		}

		public RSSFeed(string name, string url) : this(name, new Uri(url)) { }
	}
}
