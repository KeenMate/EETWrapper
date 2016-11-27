using EETWrapper.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace EETWrapper
{
	public class EETResponse
	{
		private readonly string message;
		public ResultTypes Type { get; private set; }

		public Guid UUID { get; private set; }

		public string Message { get; private set; }

		public List<EETWarning> Warnings { get; private set; }

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
		public int Code { get; private set; }
		public string Text { get; private set; }

		public WarningTypes Type => (WarningTypes) Code;

		public EETWarning(int code, string text)
		{
			Code = code;
			Text = text;
		}
	}
}