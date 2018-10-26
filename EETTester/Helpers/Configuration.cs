using System;
using System.Configuration;

namespace EETTester.Helpers
{
	public class Configuration
	{
		public static string CertificateName => ConfigurationManager.AppSettings["CertificateName"];
		public static int BusinessPremisesId => Convert.ToInt16(ConfigurationManager.AppSettings["BusinessPremisesId"]);
		public static string CashRegisterId => ConfigurationManager.AppSettings["CashRegisterId"];
		public static string ReceiptIdFormat => ConfigurationManager.AppSettings["ReceiptIdFormat"];
		public static string CancellationIdFormat => ConfigurationManager.AppSettings["CancellationIdFormat"];
		public static bool TestRun => Convert.ToBoolean(ConfigurationManager.AppSettings["TestRun"]);
		public static int Timeout => Convert.ToInt32(ConfigurationManager.AppSettings["Timeout"]);
	}
}