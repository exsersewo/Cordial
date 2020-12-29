using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Cordial.Models
{
	public class Configuration
	{
		public string Language { get; set; } = "en-gb";
		public bool CloseNotifTray { get; set; } = false;
		public string Destination { get; set; } = AppProps.DefaultPodcastDownloadDir;
		public List<RSSFeed> Podcasts { get; set; } = new List<RSSFeed>();

		public static Configuration GenerateConfiguration(string dest, IEnumerable<RSSFeed> podcasts)
			=> new Configuration
			{
				Destination = dest,
				Podcasts = podcasts.ToList()
			};

		public static Configuration Default
			=> new Configuration();

		public void Save()
			=> Save(this);

		public static string Save(Configuration configuration, string location = null)
		{
			if (string.IsNullOrEmpty(location))
			{
				if (!Directory.Exists(AppProps.ConfigurationDirectory))
				{
					Directory.CreateDirectory(AppProps.ConfigurationDirectory);
				}

				location = Path.Combine(AppProps.ConfigurationDirectory, "default.json");
			}

			File.WriteAllText(location, JsonSerializer.Serialize(configuration, new JsonSerializerOptions
			{
				WriteIndented = true
			}));

			Log.Info($"Written configuration to: {location}");

			return location;
		}

		public static Configuration Load(string location = null)
		{
			if (string.IsNullOrEmpty(location))
			{
				if (!Directory.Exists(AppProps.ConfigurationDirectory))
				{
					Directory.CreateDirectory(AppProps.ConfigurationDirectory);
				}

				if (!File.Exists(Path.Combine(AppProps.ConfigurationDirectory, "default.json")))
				{
					location = Save(Default);
				}
				else
				{
					location = Path.Combine(AppProps.ConfigurationDirectory, "default.json");
				}
			}

			var data = File.ReadAllText(location);

			var config = JsonSerializer.Deserialize<Configuration>(data, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			Log.Info($"Loaded configuration from: {location}");

			return config;
		}
	}
}
