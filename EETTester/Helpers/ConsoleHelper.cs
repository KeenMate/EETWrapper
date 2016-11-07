using System;
using System.Diagnostics.Eventing;

namespace EETTester.Helpers
{
	public static class ConsoleHelper
	{
		public static void WriteColoredText(ConsoleColor color, string text)
		{
			Console.ForegroundColor = color;
			Console.Write(text);
			Console.ResetColor();
		}

		public static void WriteLineColoredText(ConsoleColor color, string text)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ResetColor();
		}

	}
}