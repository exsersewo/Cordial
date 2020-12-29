using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Cordial
{
	public static class Log
	{
		readonly static string CurrentLogFileName =
			DateTime.UtcNow.ToString(
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture
			) + ".log";

		static RichTextBox RichTextBox;
		static StreamWriter LogFile;

		public static void Configure(RichTextBox rtb)
		{
			RichTextBox = rtb;

			if (!Directory.Exists(AppProps.BaseDirectory))
			{
				Directory.CreateDirectory(AppProps.BaseDirectory);
			}

			if (!Directory.Exists(AppProps.LogDirectory))
			{
				Directory.CreateDirectory(AppProps.LogDirectory);
			}

			LogFile = new StreamWriter(
				File.Open(
					Path.Combine(AppProps.LogDirectory, CurrentLogFileName),
					FileMode.Append,
					FileAccess.Write,
					FileShare.Read
				)
			)
			{
				AutoFlush = true,
				NewLine = Environment.NewLine
			};
		}

		public static void Info(string message)
		{
			var log = GetMessage("INF", message, null);

			if (RichTextBox != null)
			{
				RichTextBox.AppendText(log);
				RichTextBox.AppendText(Environment.NewLine);
			}
			if (LogFile != null)
			{
				LogFile.WriteLine(log);
			}

			Console.WriteLine(log);
		}
		public static void Warning(string message, Exception exception)
		{
			var log = GetMessage("WAR", message, exception);

			if (RichTextBox != null)
			{
				RichTextBox.AppendText(log);
				RichTextBox.AppendText(Environment.NewLine);
			}
			if (LogFile != null)
			{
				LogFile.WriteLine(log);
			}

			Console.WriteLine(log);
		}
		public static void Error(string message, Exception exception)
		{
			var log = GetMessage("ERR", message, exception);

			if (RichTextBox != null)
			{
				RichTextBox.AppendText(log);
				RichTextBox.AppendText(Environment.NewLine);
			}
			if (LogFile != null)
			{
				LogFile.WriteLine(log);
			}

			Console.WriteLine(log);
		}

		static string GetMessage(string level, string message, Exception exception)
		{
			string logContent = $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] [{level}] {message}";

			if (exception != null)
			{
				logContent += $"\n{exception}";
			}

			return logContent;
		}
	}
}
