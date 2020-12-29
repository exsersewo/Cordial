using Cordial.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cordial
{
	public class AppProps
	{
		public static string AppName = "Cordial";
		public static string CreatorName = "exsersewo";
		public static string AppFullName = Path.Combine(CreatorName, AppName).Replace("\\", "/");
		public static readonly string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFullName);
		public static readonly string LogDirectory = Path.Combine(BaseDirectory, "logs");
		public static readonly string ConfigurationDirectory = Path.Combine(BaseDirectory, "config");
		public static readonly string DefaultPodcastDownloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Podcasts");
		public static readonly string RepoUrl = Path.Combine("https://github.com", AppFullName);
		public static string[] Contributors = new[] { CreatorName };
		public static List<GithubStruct> OpenSourceProjects = new List<GithubStruct>
		{
			new GithubStruct("fnya", "https://github.com/fnya/OPMLCore.NET"),
			new GithubStruct("Argotic team", "https://github.com/argotic-syndication-framework/Argotic"),
			new GithubStruct("GitHub", "https://github.com/octokit/octokit.net")
		};
	}
}
