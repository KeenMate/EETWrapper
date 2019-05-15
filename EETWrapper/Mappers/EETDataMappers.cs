using EETWrapper.EETService_v311;
using EETWrapper.Interfaces;
using EETWrapper.ServiceHelpers;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EETWrapper.Mappers
{
	internal class EETDataMappers
	{
		private readonly Guid correlationId;
		private readonly ILogger logger;
		private readonly X509Certificate2 taxpayersCertificate;
		private readonly EETData data;

		public EETDataMappers(Guid correlationId, ILogger logger, X509Certificate2 taxpayersCertificate, EETData data)
		{
			this.correlationId = correlationId;
			this.logger = logger;
			this.taxpayersCertificate = taxpayersCertificate;
			this.data = data;
		}


		public TrzbaHlavickaType GetRequestHeader()
		{
			TrzbaHlavickaType hlavicka = new TrzbaHlavickaType();
			hlavicka.prvni_zaslani = data.FirstTry;
			hlavicka.uuid_zpravy = data.UUID.ToString();
			hlavicka.dat_odesl = DateTime.Now;
			hlavicka.overeni = data.TestRun;
			hlavicka.overeniSpecified = true;

			return hlavicka;
		}

		public TrzbaDataType GetRequestBody()
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

				if (data.AdditionalData.TotalTaxBase_BasicVATRate.HasValue)
				{
					body.zakl_dan1 = data.AdditionalData.TotalTaxBase_BasicVATRate.Value;
					body.zakl_dan1Specified = true;
				}

				if (data.AdditionalData.TotalVAT_BasicVATRate.HasValue)
				{
					body.dan1 = data.AdditionalData.TotalVAT_BasicVATRate.Value;
					body.dan1Specified = true;
				}

				if (data.AdditionalData.TotalTaxBase_FirstReducedVATRate.HasValue)
				{
					body.zakl_dan2 = data.AdditionalData.TotalTaxBase_FirstReducedVATRate.Value;
					body.zakl_dan2Specified = true;
				}

				if (data.AdditionalData.TotalVAT_FirstReducedVATRate.HasValue)
				{
					body.dan2 = data.AdditionalData.TotalVAT_FirstReducedVATRate.Value;
					body.dan2Specified = true;
				}

				if (data.AdditionalData.TotalTaxBase_SecondReducedVATRate.HasValue)
				{
					body.zakl_dan3 = data.AdditionalData.TotalTaxBase_SecondReducedVATRate.Value;
					body.zakl_dan3Specified = true;
				}

				if (data.AdditionalData.TotalVAT_SecondReducedVATRate.HasValue)
				{
					body.dan3 = data.AdditionalData.TotalVAT_SecondReducedVATRate.Value;
					body.dan3Specified = true;
				}

				if (data.AdditionalData.TotalAmountVATForTravelService.HasValue)
				{
					body.cest_sluz = data.AdditionalData.TotalAmountVATForTravelService.Value;
					body.cest_sluzSpecified = true;
				}

				if (data.AdditionalData.TotalAmountVATForSaleUsedGoods_BasicVATRate.HasValue)
				{
					body.pouzit_zboz1 = data.AdditionalData.TotalAmountVATForSaleUsedGoods_BasicVATRate.Value;
					body.pouzit_zboz1Specified = true;
				}

				if (data.AdditionalData.TotalAmountVATForSaleUsedGoods_FirstReducedVATRate.HasValue)
				{
					body.pouzit_zboz2 = data.AdditionalData.TotalAmountVATForSaleUsedGoods_FirstReducedVATRate.Value;
					body.pouzit_zboz2Specified = true;
				}

				if (data.AdditionalData.TotalAmountVATForSaleUsedGoods_SecondReducedVATRate.HasValue)
				{
					body.pouzit_zboz3 = data.AdditionalData.TotalAmountVATForSaleUsedGoods_SecondReducedVATRate.Value;
					body.pouzit_zboz3Specified = true;
				}

				if (data.AdditionalData.TotalAmountOfPaymentsForSubsequentDrawingOrSettlement.HasValue)
				{
					body.urceno_cerp_zuct = data.AdditionalData.TotalAmountOfPaymentsForSubsequentDrawingOrSettlement.Value;
					body.urceno_cerp_zuctSpecified = true;
				}

				if (data.AdditionalData.TotalAmountOfPaymentsSubsequentlyDrawOrSettled.HasValue)
				{
					body.cerp_zuct = data.AdditionalData.TotalAmountOfPaymentsSubsequentlyDrawOrSettled.Value;
					body.cerp_zuctSpecified = true;
				}
			}

			return body;
		}

		public TrzbaKontrolniKodyType GetRequestCheckCodes()
		{
			TrzbaKontrolniKodyType checkCodes = new TrzbaKontrolniKodyType();

			checkCodes.pkp = new PkpElementType();

			var pkp = generatePKP(taxpayersCertificate);

			checkCodes.pkp.Text = new[] { Convert.ToBase64String(pkp, Base64FormattingOptions.None) };

			var bkp = SHA1Hash(pkp);
			bkp =
				$"{bkp.Substring(0, 8)}-{bkp.Substring(8, 8)}-{bkp.Substring(16, 8)}-{bkp.Substring(24, 8)}-{bkp.Substring(32, 8)}";
			checkCodes.bkp = new BkpElementType();
			checkCodes.bkp.Text = new[] { bkp };

			return checkCodes;
		}

		public OdeslaniTrzbyRequest GetRequestData()
		{
			return new OdeslaniTrzbyRequest(GetRequestHeader(), GetRequestBody(),
				GetRequestCheckCodes());
		}

		public EETResponse GetResponse(OdpovedHlavickaType response)
		{
			return new EETResponse();
		}

		public EETResponse GetResponseFromAsync(OdeslaniTrzbyResponse response)
		{
			return new EETResponse();
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
			{
				hex.AppendFormat("{0:x2}", b);
			}

			return hex.ToString();
		}

		private byte[] generatePKP(X509Certificate2 certificate)
		{
			// 2019-05-07T19:11:31+02:00
			var date = $@"{data.CreationDate:yyyy-MM-ddTHH:mm:ssK}";

			//http://stackoverflow.com/questions/7444586/how-can-i-sign-a-file-using-rsa-and-sha256-with-net
			string sign =
				$"{data.TaxID}|{data.BusinessPremisesID}|{data.CashRegisterID}|{data.ReceiptID}|{date}|{string.Format(EETMessage.EETDecimalFormat, "{0:N}", data.TotalAmountOfSale)}";

			logger.Debug($"{correlationId} - Calculating PKP form string {sign}");

			// Note that this will return a Basic crypto provider, with only SHA-1 support
			var privKey = (RSACryptoServiceProvider)certificate.PrivateKey;
			// Force use of the Enhanced RSA and AES Cryptographic Provider with openssl-generated SHA256 keys
			var enhCsp = new RSACryptoServiceProvider().CspKeyContainerInfo;
			var cspparams = new CspParameters(enhCsp.ProviderType, enhCsp.ProviderName, privKey.CspKeyContainerInfo.KeyContainerName);

			using (RSACryptoServiceProvider key = new RSACryptoServiceProvider(cspparams))
			{
				//Sign the data
				byte[] sig = key.SignData(Encoding.UTF8.GetBytes(sign), CryptoConfig.MapNameToOID("SHA256"));

				return sig;
			}
		}
	}
}