using EETWrapper.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace EETWrapper
{
	public class EETResponse
	{
		private readonly string message;
		public ResultTypes Type { get; }

		public Guid UUID { get; }

		public string Message { get; set; }

		public List<EETWarning> Warnings { get; } = new List<EETWarning>(5);

		public bool TestRun { get; set; }

		public string Fik { get; set; }

		public string Bkp { get; set; }

		public DateTime ResponseTime { get; set; }

		public EETResponse()
		{
		}

		/// <summary>
		/// Used only for failures
		/// </summary>
		/// <param name="type">Most likely a client failure</param>
		/// <param name="warnings">Message for the user</param>
		public EETResponse(ResultTypes type, List<EETWarning> warnings)
		{
			Type = type;
			Warnings = warnings;
		}

		public EETResponse(ResultTypes type, Guid uuid)
		{
			Type = type;
			UUID = uuid;
		}

		public EETResponse(ResultTypes type, string message)
		{
			this.message = message;
			Type = type;
		}
	}

	public class EETWarning
	{
		public bool IsError { get; }
		public int Code { get; }
		public string Text { get; }

		public WarningTypes WarningType => (WarningTypes) Code;

		public ErrorTypes ErrorType => (ErrorTypes) Code;

		public EETWarning(int code, string text) : this(false, code, text)
		{
		}

		public EETWarning(bool isError, int code, string text)
		{
			IsError = isError;
			Code = code;
			Text = text;
		}
	}
}