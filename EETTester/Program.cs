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
using EETWrapper.Extensions;
using EETWrapper.ServiceHelpers;
using NLog;

namespace EETTester
{
	class Program
	{
		private static ILogger logger = LogManager.GetCurrentClassLogger();

		static void Main(string[] args)
		{
			logger.Info("### Starting a new run of EETTester");

			System.Net.ServicePointManager.ServerCertificateValidationCallback +=
					(se, cert, chain, sslerror) =>
					{
						return true;
					};

			try
			{
				sendThroughProvider();
				logger.Info("### Test run ended succesfully");
			}
			catch (Exception ex)
			{
				logger.Error(ex, "### Test run failed");
			}
		}

		static void sendThroughProvider()
		{
			var correlationId = Guid.NewGuid();

			EETProvider provider = new EETProvider(correlationId, new NlogLogger(), Configuration.CertificateName);

			EETData data = new EETData();
			data.TestRun = false;
			data.TaxID = Configuration.CertificateName;
			data.BusinessPremisesID = Configuration.BusinessPremisesId;
			data.CashRegisterID = Configuration.CashRegisterId;
			data.ReceiptID = Configuration.ReceiptIdFormat.Fill(Configuration.CashRegisterId, 1);
			data.CreationDate = DateTime.Now; //new DateTime(2018, 8, 18, 15, 12, 7);
			data.TotalAmountOfSale = 0;

			//data.AdditionalData = new AdditionalData();

			//data.AdditionalData.TotalAmountOfPaymentsSubsequentlyDrawOrSettled = 679;

			//data.AdditionalData.TotalAmountVATForTravelService = 5460;

			//data.AdditionalData.TotalAmountVATForSaleUsedGoods_BasicVATRate = 784;
			//data.AdditionalData.TotalAmountVATForSaleUsedGoods_FirstReducedVATRate = 967;
			//data.AdditionalData.TotalAmountVATForSaleUsedGoods_SecondReducedVATRate = 189;

			//data.AdditionalData.TotalAmountOfPaymentsForSubsequentDrawingOrSettlement = 324;

			//data.AdditionalData.TotalTaxBase_BasicVATRate = 343.50M;
			//data.AdditionalData.TotalVAT_BasicVATRate = 51.50M;

			//data.AdditionalData.TotalTaxBase_FirstReducedVATRate = -3538.20M;
			//data.AdditionalData.TotalVAT_FirstReducedVATRate = -530.73M;

			//data.AdditionalData.TotalTaxBase_SecondReducedVATRate = 9756.46M;
			//data.AdditionalData.TotalVAT_SecondReducedVATRate = 975.65M;

			//data.AdditionalData.TotalAmountExemptedFromVAT = 3036.00M;

			var response = provider.SendData(data);
		}

	}
}
