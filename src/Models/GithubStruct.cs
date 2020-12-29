using System;

namespace Cordial.Models
{
	public class GithubStruct
	{
		public string Author { get; internal set; }
		public Uri RepoUrl { get; internal set; }

		public GithubStruct(string author, Uri repo)
		{
			Author = author;
			RepoUrl = repo;
		}

		public GithubStruct(string author, string repo) : this(author, new Uri(repo))
		{

		}
	}
}
