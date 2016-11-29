using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using EETTester.Helpers;
using EETWrapper;
using EETWrapper.Data;
using EETWrapper.EETService_v311;
using EETWrapper.ServiceHelpers;

namespace EETTester
{
	class Program
	{
		static void Main(string[] args)
		{
			System.Net.ServicePointManager.ServerCertificateValidationCallback +=
					(se, cert, chain, sslerror) =>
					{
						return true;
					};

			sendThroughProvider();

		}

		static void sendThroughProvider()
		{
			EETProvider provider = new EETProvider("CZ26791706");

			provider.OnLogChange += Provider_OnLogChange;

			EETData data = new EETData();
			data.TestRun = true;
			data.TaxID = "CZ26791706";
			//data.AppointingPayerTaxID = "CZ683555118";
			data.BusinessPremisesID = 11;
			data.CashRegisterID = "B1/1";
			data.ReceiptID = "B1/1/1";

			data.TotalAmountOfSale = 1M;

			//data.AdditionalData = new AdditionalData();

			//data.AdditionalData.TotalAmountOfPaymentsSubsequentlyDrawOrSettled = 679;

			//data.AdditionalData.TotalAmountVATForTravelService = 5460;

			//data.AdditionalData.TotalAmountVATForSaleUsedGoods_BasicVATRate = 784;
			//data.AdditionalData.TotalAmountVATForSaleUsedGoods_FirstReducedVATRate = 967;
			//data.AdditionalData.TotalAmountVATForSaleUsedGoods_SecondReducedVATRate = 189;

			//data.AdditionalData.TotalAmountOfPaymentsForSubsequentDrawingOrSettlement = 324;

			//data.AdditionalData.TotalTaxBase_BasicVATRate = -820.92M;
			//data.AdditionalData.TotalVAT_BasicVATRate = -172.39M;

			//data.AdditionalData.TotalTaxBase_FirstReducedVATRate = -3538.20M;
			//data.AdditionalData.TotalVAT_FirstReducedVATRate = -530.73M;

			//data.AdditionalData.TotalTaxBase_SecondReducedVATRate = 9756.46M;
			//data.AdditionalData.TotalVAT_SecondReducedVATRate = 975.65M;

			//data.AdditionalData.TotalAmountExemptedFromVAT = 3036.00M;

			var response = provider.SendData(data);

		}

		private static void Provider_OnLogChange(object sender, EETWrapper.Data.LogEventArgs e)
		{
			string header = $"{e.Created:s}: {e.LogLevel.ToString().ToUpper()} - ";

			switch (e.LogLevel)
			{
				case LogEventArgs.LogLevels.Info:
					Console.Write(header);
					Console.WriteLine(e.Message);
					break;
				case LogEventArgs.LogLevels.Warn:
					Console.Write(header);
					ConsoleHelper.WriteLineColoredText(ConsoleColor.Yellow, e.Message);
					break;
				case LogEventArgs.LogLevels.Error:
					Console.Write(header);
					ConsoleHelper.WriteLineColoredText(ConsoleColor.Red, e.Message);
					ConsoleHelper.WriteLineColoredText(ConsoleColor.Red, e.Exception.ToString());
					break;
				case LogEventArgs.LogLevels.Trace:
					Console.Write(header);
					ConsoleHelper.WriteLineColoredText(ConsoleColor.DarkGray, e.Message);

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static void plainAndSimple()
		{

			{
				X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
				store.Open(OpenFlags.ReadOnly);
				var allCertificates = store.Certificates;

				var posCert =
						store.Certificates.Cast<X509Certificate2>().FirstOrDefault(x => x.FriendlyName == "CZ00000019");

				var siteCert =
						store.Certificates.Cast<X509Certificate2>().FirstOrDefault(x => x.Subject == "CN=www.eet.cz, OU=Elektronická evidence tržeb, O=Generální finanční ředitelství, L=Praha, S=Praha, C=CZ, SERIALNUMBER=72080043, OID.2.5.4.15=Government Entity, OID.1.3.6.1.4.1.311.60.2.1.3=CZ");


				X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName,
						"CZ00000019", false);
				if (cers.Count > 0)
				{
					var cer = cers[0];
				}
					;
				store.Close();

				EETClient client = new EETClient();

				//client.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySubjectName, "CZ00000019");

				//var endpointBinding = client.Endpoint.Binding;

				//EETService service = new EETService();
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				//System.Net.ServicePointManager.ServerCertificateValidationCallback +=
				//		(se, cert, chain, sslerror) =>
				//		{
				//			return true;
				//		};

				//service.ClientCertificates.Add(orDefault);

				TrzbaHlavickaType hlavicka = new TrzbaHlavickaType();
				hlavicka.prvni_zaslani = true;
				hlavicka.uuid_zpravy = Guid.NewGuid().ToString();
				hlavicka.dat_odesl = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour,
					DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Local);
				
				hlavicka.overeni = true;

				TrzbaDataType data = new TrzbaDataType();

				data.dic_popl = "CZ00000019";
				data.dic_poverujiciho = "CZ683555118";
				data.id_provoz = 273;
				data.id_pokl = "/5546/RO24";
				data.porad_cis = "0/6460/ZQ42"; //data.id_pokl + "/" + DateTime.Now.Second;

				data.dat_trzby = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Local);
				data.rezim = 0;

				data.celk_trzba = 34113.00M;
				Console.ForegroundColor = ConsoleColor.Black;
				data.cerp_zuct = 679;
				data.cerp_zuctSpecified = true;
				data.cest_sluz = 5460;
				data.cest_sluzSpecified = true;
				data.dan1 = -172.39M;
				data.dan1Specified = true;
				data.dan2 = -530.73M;
				data.dan2Specified = true;
				data.dan3 = 975.65M;
				data.dan3Specified = true;
				data.dat_trzby = new DateTime(2016, 08, 05, 0, 30, 12, DateTimeKind.Local);

				data.pouzit_zboz1 = 784;
				data.pouzit_zboz1Specified = true;
				data.pouzit_zboz2 = 967;
				data.pouzit_zboz2Specified = true;
				data.pouzit_zboz3 = 189;
				data.pouzit_zboz3Specified = true;


				data.urceno_cerp_zuct = 324;
				data.urceno_cerp_zuctSpecified = true;
				data.zakl_dan1 = -820.92M;
				data.zakl_dan1Specified = true;
				data.zakl_dan2 = -3538.20M;
				data.zakl_dan2Specified = true;
				data.zakl_dan3 = -9756.46M;
				data.zakl_dan3Specified = true;
				data.zakl_nepodl_dph = 3036.00M;
				data.zakl_nepodl_dphSpecified = true;

				//TimeZoneInfo tzi = TimeZoneInfo.CreateCustomTimeZone("+2", TimeSpan.FromHours(2),"+2:00", "+2:00" );

				//var convertTimeFromUtc = TimeZoneInfo.ConvertTimeFromUtc(data.dat_trzby, tzi);

				//DateTimeOffset dto = new DateTimeOffset(data.dat_trzby, TimeSpan.FromHours(2));

				//TrzbaKontrolniKodyType kontrolniKody = new TrzbaKontrolniKodyType();

				//kontrolniKody.pkp = new PkpElementType();


				//var pkp = generatePKP(posCert, data);

				//kontrolniKody.pkp.Text = new[] { Convert.ToBase64String(pkp, Base64FormattingOptions.None) };

				//var bkp = SHA1Hash(pkp);
				//bkp = $"{bkp.Substring(0, 8)}-{bkp.Substring(8, 8)}-{bkp.Substring(16, 8)}-{bkp.Substring(24, 8)}-{bkp.Substring(32, 8)}";
				//kontrolniKody.bkp = new BkpElementType();
				//kontrolniKody.bkp.Text = new[] { bkp };

				//var rsaCryptoServiceProvider = posCert.PrivateKey as RSACryptoServiceProvider;

				////var privateKey = rsaCryptoServiceProvider.ExportParameters(true);

				////var computeHash = rsaCryptoServiceProvider.SignData(Encoding.UTF8.GetBytes(sign), new SHA256CryptoServiceProvider());


				////rsaCryptoServiceProvider.SignData()

				////using (var sha256 = new SHA256CryptoServiceProvider())
				////{


				////    var computeHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(sign));
				////    kontrolniKody.pk
				////}

				//object response;
				//OdpovedVarovaniType[] warnings;
				//client.OdeslaniTrzby(hlavicka, data, kontrolniKody, out response, out warnings);


			}
		}

	}
}
