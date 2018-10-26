using System;

namespace EETWrapper.Interfaces
{
	public interface ILogger
	{
		void Info(string message);
		void Debug(string message);
		void Trace(string message);
		void Warn(string message);
		void Warn(Exception ex, string message);
		void Error(string message);
		void Error(Exception ex, string message);
	}
}