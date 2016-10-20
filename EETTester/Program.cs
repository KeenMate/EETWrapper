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
using EETWrapper.EETService_v3;
using EETWrapper.ServiceHelpers;

namespace EETTester
{
	class Program
	{
		static void Main(string[] args)
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

			var endpointBinding = client.Endpoint.Binding;

			//EETService service = new EETService();
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			System.Net.ServicePointManager.ServerCertificateValidationCallback +=
					(se, cert, chain, sslerror) =>
					{
						return true;
					};

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

			TrzbaKontrolniKodyType kontrolniKody = new TrzbaKontrolniKodyType();

			kontrolniKody.pkp = new PkpElementType();


			var pkp = generatePKP(posCert, data);

			kontrolniKody.pkp.Text = new[] { Convert.ToBase64String(pkp, Base64FormattingOptions.None) };

			var bkp = SHA1Hash(pkp);
			bkp = $"{bkp.Substring(0,8)}-{bkp.Substring(8,8)}-{bkp.Substring(16,8)}-{bkp.Substring(24,8)}-{bkp.Substring(32,8)}";
			kontrolniKody.bkp = new BkpElementType();
			kontrolniKody.bkp.Text = new[] {bkp};

			var rsaCryptoServiceProvider = posCert.PrivateKey as RSACryptoServiceProvider;

			//var privateKey = rsaCryptoServiceProvider.ExportParameters(true);

			//var computeHash = rsaCryptoServiceProvider.SignData(Encoding.UTF8.GetBytes(sign), new SHA256CryptoServiceProvider());


			//rsaCryptoServiceProvider.SignData()

			//using (var sha256 = new SHA256CryptoServiceProvider())
			//{


			//    var computeHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(sign));
			//    kontrolniKody.pk
			//}

			object response;
			OdpovedVarovaniType[] warnings;
			client.OdeslaniTrzby(hlavicka, data, kontrolniKody, out response, out warnings);


		}

		static string SHA1Hash(byte[] input)
		{
			var hash = (new SHA1Managed()).ComputeHash(input);
			return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
		}

		public static string ByteArrayToString(byte[] ba)
		{
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}

		private static byte[] generatePKP(X509Certificate2 certificate, TrzbaDataType data)
		{
			//http://stackoverflow.com/questions/7444586/how-can-i-sign-a-file-using-rsa-and-sha256-with-net
			string sign =
					$"{data.dic_popl}|{data.id_provoz}|{data.id_pokl}|{data.porad_cis}|{data.dat_trzby.ToString("yyyy-MM-ddTHH:mm:sszzz").Replace("+03:00", "+02:00")}|{data.celk_trzba.ToString(EETMessage.EETDecimalFormat)}";


			// Note that this will return a Basic crypto provider, with only SHA-1 support
			var privKey = (RSACryptoServiceProvider)certificate.PrivateKey;
			// Force use of the Enhanced RSA and AES Cryptographic Provider with openssl-generated SHA256 keys
			var enhCsp = new RSACryptoServiceProvider().CspKeyContainerInfo;
			var cspparams = new CspParameters(enhCsp.ProviderType, enhCsp.ProviderName, privKey.CspKeyContainerInfo.KeyContainerName);

			using (RSACryptoServiceProvider key = new RSACryptoServiceProvider(cspparams))
			{
				//Sign the data
				byte[] sig = key.SignData(UTF8Encoding.UTF8.GetBytes(sign), CryptoConfig.MapNameToOID("SHA256"));

				return sig;
			}
		}
	}
}
