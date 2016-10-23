using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using EETWrapper.EETService_v3;
using EETWrapper.ServiceHelpers;

namespace EETWrapper.Mappers
{
	internal static class EETDataMappers
	{
		public static TrzbaHlavickaType GetRequestHeader(this EETData data)
		{
			TrzbaHlavickaType hlavicka = new TrzbaHlavickaType();
			hlavicka.prvni_zaslani = data.FirstTry;
			hlavicka.uuid_zpravy = data.UUID.ToString();
			hlavicka.dat_odesl = data.CreationDate;
			hlavicka.overeni = data.TestRun;

			return hlavicka;
		}

		public static TrzbaDataType GetRequestBody(this EETData data)
		{
			TrzbaDataType body = new TrzbaDataType();

			body.dic_popl = data.TaxID;
			body.dic_poverujiciho = data.AppointingPayerTaxID;
			body.id_provoz = data.BusinessPremisesID;
			body.id_pokl = data.CashRegisterID;
			body.porad_cis = data.ReceiptID;

			body.dat_trzby = data.CreationDate;
			body.rezim = (int)data.SaleRegime;

			body.celk_trzba = data.TotalAmountOfSale;

			// Additional data

			if (data.AdditionalData != null)
			{
				if (data.AdditionalData.TotalAmountExemptedFromVAT.HasValue)
				{
					body.zakl_nepodl_dph = data.AdditionalData.TotalAmountExemptedFromVAT.Value;
					body.zakl_nepodl_dphSpecified = true;
				}

				body.cerp_zuct = 679;
				body.cerp_zuctSpecified = true;
				body.cest_sluz = 5460;
				body.cest_sluzSpecified = true;
				body.dan1 = -172.39M;
				body.dan1Specified = true;
				body.dan2 = -530.73M;
				body.dan2Specified = true;
				body.dan3 = 975.65M;
				body.dan3Specified = true;
				body.dat_trzby = new DateTime(2016, 08, 05, 0, 30, 12, DateTimeKind.Local);

				body.pouzit_zboz1 = 784;
				body.pouzit_zboz1Specified = true;
				body.pouzit_zboz2 = 967;
				body.pouzit_zboz2Specified = true;
				body.pouzit_zboz3 = 189;
				body.pouzit_zboz3Specified = true;


				body.urceno_cerp_zuct = 324;
				body.urceno_cerp_zuctSpecified = true;
				body.zakl_dan1 = -820.92M;
				body.zakl_dan1Specified = true;
				body.zakl_dan2 = -3538.20M;
				body.zakl_dan2Specified = true;
				body.zakl_dan3 = -9756.46M;
				body.zakl_dan3Specified = true;
				body.zakl_nepodl_dph = 3036.00M;
				body.zakl_nepodl_dphSpecified = true;

#warning Finish the mappings
			}

			

			return body;
		}

		public static TrzbaKontrolniKodyType GetRequestCheckCodes(this EETData data, X509Certificate2 taxpayersCertificate)
		{
			TrzbaKontrolniKodyType checkCodes = new TrzbaKontrolniKodyType();

			checkCodes.pkp = new PkpElementType();

			var pkp = generatePKP(taxpayersCertificate, data);

			checkCodes.pkp.Text = new[] { Convert.ToBase64String(pkp, Base64FormattingOptions.None) };

			var bkp = SHA1Hash(pkp);
			bkp = $"{bkp.Substring(0, 8)}-{bkp.Substring(8, 8)}-{bkp.Substring(16, 8)}-{bkp.Substring(24, 8)}-{bkp.Substring(32, 8)}";
			checkCodes.bkp = new BkpElementType();
			checkCodes.bkp.Text = new[] { bkp };

			return checkCodes;
		}

		public static OdeslaniTrzbyRequest GetRequestData(this EETData data, X509Certificate2 taxpayersCertificate)
		{
			return new OdeslaniTrzbyRequest(data.GetRequestHeader(), data.GetRequestBody(), data.GetRequestCheckCodes(taxpayersCertificate));
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

		private static byte[] generatePKP(X509Certificate2 certificate, EETData data)
		{
			//http://stackoverflow.com/questions/7444586/how-can-i-sign-a-file-using-rsa-and-sha256-with-net
			string sign =
					$"{data.TaxID}|{data.BusinessPremisesID}|{data.CashRegisterID}|{data.ReceiptID}|{data.CreationDate.ToString("yyyy-MM-ddTHH:mm:sszzz").Replace("+03:00", "+02:00")}|{data.TotalAmountOfSale.ToString(EETMessage.EETDecimalFormat)}";


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