﻿using System;

namespace EETWrapper.Data
{
	public class LogEventArgs : EventArgs
	{
		public enum LogLevels { Info, Warn, Error,
			Trace
		}

		public DateTime Created { get; } = DateTime.Now;

		public string Message { get; private set; }

		public LogLevels LogLevel { get; private set; }

		public Exception Exception { get; private set; }

		public LogEventArgs(string message, LogLevels logLevel)
		{
			LogLevel = logLevel;
			Message = message;
		}

		public LogEventArgs(string message, LogLevels logLevel, Exception exception) : this(message, logLevel)
		{
			Exception = exception;
		}
	}
}