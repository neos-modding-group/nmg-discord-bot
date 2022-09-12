using Discord;

namespace nmgBot
{
	internal static class logWraper
	{
		public static void Log(string msg, LogSeverity severity = LogSeverity.Info, Exception exception = null, string source = "nmgBot")
		{
			Console.WriteLine(new LogMessage(severity, source, msg, exception).ToString());
		}

		public static void Debug(string msg, Exception exception = null, string source = "nmgBot")
		{
			Log(msg, LogSeverity.Debug, exception, source);
		}

		public static void Warning(string msg, Exception exception = null, string source = "nmgBot")
		{
			Log(msg, LogSeverity.Warning, exception, source);
		}

		public static void Error(string msg, Exception exception = null, string source = "nmgBot")
		{
			Log(msg, LogSeverity.Error, exception, source);
		}

		public static void Critical(string msg, Exception exception = null, string source = "nmgBot")
		{
			Log(msg, LogSeverity.Critical, exception, source);
		}

		public static void Verbose(string msg, Exception exception = null, string source = "nmgBot")
		{
			Log(msg, LogSeverity.Verbose, exception, source);
		}
	}
}
