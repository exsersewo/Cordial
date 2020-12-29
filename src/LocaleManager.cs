using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Cordial.Globalization
{
	public class LocaleManager
	{
		private static Dictionary<string, ResourceManager> locales = new Dictionary<string, ResourceManager>();
		private static Dictionary<string, string> localehumannames = new Dictionary<string, string>();
		public Dictionary<string, ResourceManager> Locales { get => locales; }
		public Dictionary<string, string> LocaleHumanNames { get => localehumannames; }
		public const string DefaultLocale = "en-gb";

		public ResourceManager GetDefaultLocale()
		{
			if (locales.TryGetValue(DefaultLocale, out var resMan))
			{
				return resMan;
			}

			throw new System.NullReferenceException("Couldn't get default locale, is it registered?");
		}

		public static LocaleManager InitialiseLocales()
		{
			LocaleManager localeinstance = new LocaleManager();

			if (!locales.Any())
			{
				string[] list = Directory.GetFiles(Path.Combine(Application.StartupPath, "Resources", "Languages"), "*.resx");

				Assembly currentAssembly = Assembly.GetExecutingAssembly();

				foreach (string str in list)
				{
					Regex regex = new Regex("[a-zA-Z-]*.resx");

					string language = regex.Match(str).ToString().Replace(".resx", string.Empty);

					if (string.IsNullOrEmpty(language)) continue;

					if (!locales.ContainsKey(language))
					{
						var niceKey = language.Replace("-", "_");

						var languageClass = currentAssembly.GetType($"Cordial.Resources.Languages.{niceKey}");

						var instance = FormatterServices.GetUninitializedObject(languageClass.GetType());

						var resProp = languageClass.GetRuntimeProperties().FirstOrDefault(x => x.Name == "ResourceManager");

						var resMan = resProp.GetValue(instance) as ResourceManager;

						locales.Add(language, resMan);
						localehumannames.Add(resMan.GetString("human_name"), language);

						Log.Info($"Registered \"{resMan.GetString("human_name")}\" into locale database");
					}
				}

				Log.Info($"Registered: {locales.Count}/{list.Length} languages into locale database");
			}

			return localeinstance;
		}

		public ResourceManager GetLocale(string id)
		{
			var locale = locales.FirstOrDefault(x => x.Key == id);
			if (locale.Value != null)
				return locale.Value;
			else
				return null;
		}
	}
}