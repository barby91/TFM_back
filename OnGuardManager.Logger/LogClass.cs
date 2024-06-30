
using NLog;
using NLog.Web;

namespace onGuardManager.Logger
{
	public static class LogClass
	{
		private static readonly NLog.Logger logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

		public static void WriteLog(ErrorWrite level, string message)
		{
			switch(level)
			{
				case ErrorWrite.Debug:
					logger.Debug(message);
					break;
				case ErrorWrite.Error:
					logger.Error(message);
					break;
				case ErrorWrite.Trace:
					logger.Trace(message);
					break;
				case ErrorWrite.Info:
					logger.Info(message);
					break;
				default:
					logger.Debug(message);
					break;
			}
		}

		public static void FlushNLog()
		{
			NLog.LogManager.Shutdown();
		}
	}
}