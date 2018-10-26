using System;
using NLog;
using ILogger = EETWrapper.Interfaces.ILogger;

namespace EETTester.Helpers
{
	public class NlogLogger : ILogger
	{
		private NLog.ILogger logger = LogManager.GetCurrentClassLogger(); // not ideal, we probably lose point of log origin

		private void logMessage(LogLevel level, string message, Exception ex = null)
		{
			if (level == LogLevel.Debug)
				logger.Debug(message);
			else if (level == LogLevel.Error)
			{
				logger.Error(ex, message);
			}
			else if (level == LogLevel.Fatal)
			{
				logger.Fatal(ex, message);
			}
			else if (level == LogLevel.Info)
			{
				logger.Info(ex, message);
			}
			else if (level == LogLevel.Off)
			{

			}
			else if (level == LogLevel.Trace)
			{
				logger.Trace(ex, message);
			}
			else if (level == LogLevel.Warn)
			{
				logger.Warn(ex, message);
			}
		}

		public void Trace(string message)
		{
			logMessage(LogLevel.Trace, message);
		}

		public void Debug(string message)
		{
			logMessage(LogLevel.Debug, message);
		}

		public void Info(string message)
		{
			logMessage(LogLevel.Info, message);
		}

		public void Warn(string message)
		{
			logMessage(LogLevel.Warn, message);
		}

		public void Warn(Exception ex, string message)
		{
			logMessage(LogLevel.Debug, message, ex);
		}

		public void Error(string message)
		{
			logMessage(LogLevel.Error, message);
		}

		public void Error(Exception ex, string message)
		{
			logMessage(LogLevel.Error, message, ex);
		}

		public void Fatal(string message)
		{
			logMessage(LogLevel.Fatal, message);
		}

		public void Fatal(Exception ex, string message)
		{
			logMessage(LogLevel.Fatal, message, ex);
		}
	}
}