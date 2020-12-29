using Cordial.Globalization;
using Cordial.Models;
using Octokit;
using OPMLCore.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;
using Configuration = Cordial.Models.Configuration;

namespace Cordial
{
	public partial class Form1 : Form
	{
		List<RSSFeed> Feeds;
		string downloadDestination;
		List<Podcast> Subscriptions;
		LocaleManager locale;
		Configuration currentConfiguration;
		GitHubClient githubClient;
#if PRERELEASE
		bool isPreRelease = true;
#else
		bool isPreRelease = false;
#endif

		public Form1()
		{
			if (!Directory.Exists(AppProps.DefaultPodcastDownloadDir))
			{
				Directory.CreateDirectory(AppProps.DefaultPodcastDownloadDir);
			}

			InitializeComponent();

			Log.Configure(logRTB);

			currentConfiguration = Configuration.Load();

			Feeds = currentConfiguration.Podcasts;
			downloadDestination = currentConfiguration.Destination;

			currentConfiguration.Language = currentConfiguration.Language;

			locale = LocaleManager.InitialiseLocales();

			foreach (var localeEntry in locale.Locales)
			{
				ToolStripMenuItem item = new ToolStripMenuItem(localeEntry.Value.GetString("human_name"), image: null, onClick: (e, sender) =>
				{
					currentConfiguration.Language = localeEntry.Key;

					currentConfiguration.Save();

					Log.Info($"Using \"{localeEntry.Value.GetString("human_name")}\" as current language");

					UpdateLanguage();
				})
				{
					Checked = true,
					CheckState = currentConfiguration.Language == localeEntry.Key ? CheckState.Checked : CheckState.Unchecked,
				};

				selectLanguageToolStripMenuItem.DropDownItems.Add(item);

				if (currentConfiguration.Language == localeEntry.Key)
				{
					Log.Info($"Using \"{localeEntry.Value.GetString("human_name")}\" as current language");
				}
			}

			showWindowNotificationMenu.Visible = false;
			showWindowNotificationMenu.Click += ShowWindowNotificationMenu_Click;
			closeNotificationTray.Click += CloseNotificationTray_Click;
			closeNotificationTray.Checked = currentConfiguration.CloseNotifTray;

			UpdateLanguage();

			UpdateSubscriptions();
		}

		private void ShowWindowNotificationMenu_Click(object sender, EventArgs e)
		{
			Visible = true;
			showWindowNotificationMenu.Visible = false;
		}

		void UpdateSubscriptions()
		{
			if (Subscriptions == null)
			{
				Subscriptions = new List<Podcast>();
			}

			Subscriptions.Clear();

			foreach (var feed in Feeds)
			{
				ulong mb = 0;
				string podcastPath = Path.Combine(downloadDestination, feed.Name);
				if (Directory.Exists(podcastPath))
				{
					mb = (ulong)new DirectoryInfo(podcastPath).GetFiles().Sum(z => z.Length) / 1000000;
				}

				Subscriptions.Add(new Podcast
				{
					Name = feed.Name,
					Status = PodcastStatus.NewlySubscribed,
					MB = mb,
					Location = feed.URL.OriginalString
				});
			}

			podcastBindingSource.DataSource = Subscriptions;
		}

		string GetLanguageString(string key, ResourceManager targetResource, ResourceManager defaultResource = null)
		{
			var value = key;

			if (locale != null)
			{
				if (defaultResource == null)
				{
					defaultResource = locale.GetDefaultLocale();
				}

				value = targetResource.GetString(key);

				if (string.IsNullOrEmpty(value))
				{
					value = defaultResource.GetString(key);

					if (string.IsNullOrEmpty(value))
					{
						value = key;
					}
				}
			}

			return value;
		}

		void UpdateLanguage()
		{
			var currentLocale = locale != null ? locale.GetLocale(currentConfiguration.Language) : null;
			var defaultLocale = locale != null ? locale.GetDefaultLocale() : null;

			foreach (var item in selectLanguageToolStripMenuItem.DropDownItems)
			{
				if (item is ToolStripMenuItem tsmi)
				{
					if (tsmi.Text == currentLocale.GetString("human_name"))
					{
						tsmi.CheckState = CheckState.Checked;
					}
					else
					{
						tsmi.CheckState = CheckState.Unchecked;
					}
				}
			}

			//statustray
			liveDownloadCounter.Text = GetLanguageString("generic_livedownloads", currentLocale, defaultLocale).Replace("{0}", "0");
			downloadSpeed.Text = GetLanguageString("generic_downloadspeed", currentLocale, defaultLocale).Replace("{0}", "0");

			//toolstrip
			fileToolStripMenuItem.Text = GetLanguageString("generic_file", currentLocale, defaultLocale);
			editToolStripMenuItem.Text = GetLanguageString("generic_edit", currentLocale, defaultLocale);
			viewToolStripMenuItem.Text = GetLanguageString("generic_view", currentLocale, defaultLocale);
			toolsToolStripMenuItem.Text = GetLanguageString("generic_tools", currentLocale, defaultLocale);
			helpToolStripMenuItem.Text = GetLanguageString("generic_help", currentLocale, defaultLocale);

			//pages
			downloadsPage.Text = GetLanguageString("generic_downloads", currentLocale, defaultLocale);
			subscriptionsPage.Text = GetLanguageString("generic_subscriptions", currentLocale, defaultLocale);
			cleanupPage.Text = GetLanguageString("generic_cleanup", currentLocale, defaultLocale);
			logPage.Text = GetLanguageString("generic_log", currentLocale, defaultLocale);

			//file
			importFeedsFromOpmlToolStripMenuItem.Text = GetLanguageString("file_importopml", currentLocale, defaultLocale);
			exportFeedsAsOpmlToolStripMenuItem.Text = GetLanguageString("file_exportopml", currentLocale, defaultLocale);
			preferencesToolStripMenuItem.Text = GetLanguageString("file_preferences", currentLocale, defaultLocale);
			closeWindowToolStripMenuItem.Text = GetLanguageString("file_close", currentLocale, defaultLocale);
			closeNotificationTray.Text = GetLanguageString("file_closetotray", currentLocale, defaultLocale);
			quitToolStripMenuItem.Text = GetLanguageString("file_quit", currentLocale, defaultLocale);

			//edit
			selectAllToolStripMenuItem.Text = GetLanguageString("edit_selectall", currentLocale, defaultLocale);

			//view


			//tools
			checkAllToolStripMenuItem.Text = GetLanguageString("tools_checkall", currentLocale, defaultLocale);
			catchupToolStripMenuItem.Text = GetLanguageString("tools_catchup", currentLocale, defaultLocale);
			checkSelectedToolStripMenuItem.Text = GetLanguageString("tools_checkselected", currentLocale, defaultLocale);
			addAFeedToolStripMenuItem.Text = GetLanguageString("tools_addfeed", currentLocale, defaultLocale);
			removeFeedToolStripMenuItem.Text = GetLanguageString("tools_removefeed", currentLocale, defaultLocale);
			feedPropertiesToolStripMenuItem.Text = GetLanguageString("tools_feedprops", currentLocale, defaultLocale);
			schedulerToolStripMenuItem.Text = GetLanguageString("tools_scheduler", currentLocale, defaultLocale);
			selectLanguageToolStripMenuItem.Text = GetLanguageString("tools_selectlang", currentLocale, defaultLocale);

			//help
			checkForUpdateToolStripMenuItem.Text = GetLanguageString("help_checkupdates", currentLocale, defaultLocale);
			reportAProblemToolStripMenuItem.Text = GetLanguageString("help_reportproblem", currentLocale, defaultLocale);
			aboutToolStripMenuItem.Text = string.Format(GetLanguageString("help_about", currentLocale, defaultLocale), AppProps.AppName);

			//notification context menu
			showWindowNotificationMenu.Text = GetLanguageString("file_showwindow", currentLocale, defaultLocale);
			runCheckNotificationMenu.Text = GetLanguageString("generic_runcheck", currentLocale, defaultLocale);
			quitNotificationMenu.Text = quitToolStripMenuItem.Text;
		}

		bool closeOverride = false;
		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			closeOverride = true;
			this.Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (closeNotificationTray.CheckState == CheckState.Checked && !closeOverride)
			{
				Visible = false;
				showWindowNotificationMenu.Visible = true;
			}
			else
			{
				base.OnClosing(e);
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			if (closeNotificationTray.CheckState == CheckState.Checked && !closeOverride)
			{

			}
			else
			{
				base.OnClosed(e);
			}
		}

		private void importFeedsFromOpmlToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (opmlFileOpen.ShowDialog() == DialogResult.OK)
			{
				if (opmlFileOpen.Multiselect)
				{
					foreach (var filePath in opmlFileOpen.FileNames)
					{
						ImportOPML(filePath);
					}
				}
				else
				{
					ImportOPML(opmlFileOpen.FileName);
				}
			}
		}

		private void exportFeedsAsOpmlToolStripMenuItem_Click(object sender, EventArgs e)
		{
			opmlFileSave.ShowDialog();
		}

		void ImportOPML(string filePath)
		{
			Opml opmldoc = new Opml(filePath);

			Log.Info($"Opened: {filePath}");

			foreach (var rssOutline in opmldoc.Body.Outlines[0].Outlines)
			{
				Feeds.Add(new RSSFeed(rssOutline.Title, rssOutline.XMLUrl));

				Log.Info($"Added: {rssOutline.Title}");
			}

			Configuration.Save(Configuration.GenerateConfiguration(downloadDestination, Feeds));

			UpdateSubscriptions();
		}

		private void notificationIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Visible = true;
			showWindowNotificationMenu.Visible = false;
		}

		private void closeWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (closeNotificationTray.CheckState == CheckState.Checked && !closeOverride)
			{
				Visible = false;
				showWindowNotificationMenu.Visible = true;
			}
			else
			{
				this.Close();
			}
		}

		private void CloseNotificationTray_Click(object sender, EventArgs e)
		{
			if (sender is ToolStripMenuItem tsmi)
			{
				tsmi.Checked = currentConfiguration.CloseNotifTray = !currentConfiguration.CloseNotifTray;
				currentConfiguration.Save();
			}
		}

		private void reportAProblemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(Path.Combine(AppProps.RepoUrl, "issues", "new", "choose"));
		}

		private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (githubClient == null)
			{
				githubClient = new GitHubClient(new ProductHeaderValue(AppProps.AppName, Assembly.GetExecutingAssembly().GetName().Version.ToString()));
			}

			Task.Run(async () =>
			{
#if DEBUG
				var releases = await githubClient.Repository.Release.GetAll("xercsis", $"Cordial-Test");
#else
				var releases = await githubClient.Repository.Release.GetAll(AppProps.CreatorName, AppProps.AppName);
#endif

				if (releases.Any())
				{
					var assName = Assembly.GetExecutingAssembly().GetName();

					Release latestRelease = null;

					if (isPreRelease)
					{
						latestRelease = releases.FirstOrDefault(rel => rel.Prerelease);
					}
					else
					{
						latestRelease = releases.FirstOrDefault(rel => !rel.Prerelease);
					}

					if (latestRelease != null)
					{
						var tagVersion = new Version(latestRelease.TagName);

						if (tagVersion > assName.Version)
						{
							showUpdateBox();
						}
					}
				}
			});
		}

		[STAThread]
		void showUpdateBox()
		{
			var currentLocale = locale != null ? locale.GetLocale(currentConfiguration.Language) : null;
			var defaultLocale = locale != null ? locale.GetDefaultLocale() : null;

			MessageBoxManager.Yes = GetLanguageString("generic_yes", currentLocale, defaultLocale);
			MessageBoxManager.No = GetLanguageString("generic_no", currentLocale, defaultLocale);

			MessageBoxManager.Register();

			var dialogueResult = MessageBox.Show(
					GetLanguageString("update_maintext", currentLocale, defaultLocale),
					GetLanguageString("update_caption", currentLocale, defaultLocale),
					MessageBoxButtons.YesNo
				);

			MessageBoxManager.Unregister();

			if (dialogueResult == DialogResult.Yes)
			{

			}
		}
	}
}
