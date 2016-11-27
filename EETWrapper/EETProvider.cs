using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EETWrapper.Assets;
using EETWrapper.Data;
using EETWrapper.EETService_v311;
using EETWrapper.Extensions;
using EETWrapper.Interfaces;
using EETWrapper.Mappers;
using EETWrapper.SignatureBehavior;

namespace EETWrapper
{
	public class EETProvider
	{
		private X509Certificate2 taxpayersCertificate;
		private readonly string customEndPointName;

		public event EventHandler<LogEventArgs> OnLogChange;

		static EETProvider()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}

		public EETProvider(string certName)
		{
			taxpayersCertificate = fetchCertificate(certName);
		}

		public EETProvider(X509Certificate2 taxpayersCertificate, string customEndPointName)
		{
			this.taxpayersCertificate = taxpayersCertificate;
			this.customEndPointName = customEndPointName;
		}


		/// <summary>
		/// X509 taxpayer's certificate
		/// </summary>
		/// <param name="taxpayersCertificate"></param>
		public EETProvider(X509Certificate2 taxpayersCertificate)
		{
			if (taxpayersCertificate == null)
				throw new ArgumentOutOfRangeException(nameof(taxpayersCertificate), Exceptions.CertificateCannotNotBeNull);
			this.taxpayersCertificate = taxpayersCertificate;
		}

		#region Private

		private X509Certificate2 fetchCertificate(string certName)
		{
			logInfo(Messages.OpeningPersonalCurrentUserCertStore);

			X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			try
			{
				store.Open(OpenFlags.ReadOnly);

				logInfo(Messages.SearchingForCertByName);
				X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName,
					certName, false);

				logTrace($"Certificates found: {cers.Count}");

				switch (cers.Count)
				{
					case 0:
						throw new ArgumentOutOfRangeException(nameof(certName), string.Format(Exceptions.CouldNotFindTaxpayersCertificate, certName));
					case 1:
						return cers[0];
					default:
						throw new ArgumentOutOfRangeException(nameof(certName), string.Format(Exceptions.MoreThanOneTaxpayersCertificate, certName));
				}
			}
			finally
			{
				store.Close();
			}
		}

		private EETClient prepareClient()
		{
			logTrace("Creating EETClient object " + (string.IsNullOrEmpty(customEndPointName) ? "with default": $"with custom endpoint name: {customEndPointName}" ));
			EETClient client = string.IsNullOrEmpty(customEndPointName)? new EETClient(): new EETClient(customEndPointName);

			client.Endpoint.Behaviors.Add(new SignMessageWithCertificateBehavior(taxpayersCertificate));

			return client;
		}

		#region Logging

		private void logInfo(string message)
		{
			if(OnLogChange!=null)
				OnLogChange.Invoke(this, new LogEventArgs(message, LogEventArgs.LogLevels.Info));
		}

		private void logWarn(string message)
		{
			if (OnLogChange != null)
				OnLogChange.Invoke(this, new LogEventArgs(message, LogEventArgs.LogLevels.Warn));
		}

		private void logError(string message, Exception ex)
		{
			if (OnLogChange != null)
				OnLogChange.Invoke(this, new LogEventArgs(message, LogEventArgs.LogLevels.Error, ex));
		}

		private void logTrace(string message)
		{
			if (OnLogChange != null)
				OnLogChange.Invoke(this, new LogEventArgs(message, LogEventArgs.LogLevels.Trace));
		}

		#endregion

		#endregion

		public EETResponse SendData(EETData data)
		{
			logInfo(Messages.SendingEETData);
			EETResponse eetResponse;
			try
			{
				logTrace("Preparing WCF client");
				var client = prepareClient();

				object response;
				OdpovedVarovaniType[] warnings;

				OdpovedHlavickaType odpovedHlavickaType = client.OdeslaniTrzby(data.GetRequestHeader(), data.GetRequestBody(), data.GetRequestCheckCodes(taxpayersCertificate), out response, out warnings);

				if(response is OdpovedChybaType)
				{
					OdpovedChybaType o = (OdpovedChybaType) response;
					logWarn(Messages.ReceivedError.Fill($"({o.kod}) {o.Text[0]}"));
					eetResponse = new EETResponse(ResultTypes.Error, new List<EETWarning>() {new EETWarning(o.kod, o.Text[0])});
					eetResponse.ResponseTime = odpovedHlavickaType.dat_odmit;
				}
				else
				{
					OdpovedPotvrzeniType o = (OdpovedPotvrzeniType)response;
					logInfo(Messages.ReceivedSuccess.Fill(o.fik, odpovedHlavickaType.bkp));
					eetResponse = new EETResponse(ResultTypes.Success, new Guid(odpovedHlavickaType.uuid_zpravy));
					eetResponse.ResponseTime = odpovedHlavickaType.dat_prij;
					eetResponse.Fik = o.fik;
					eetResponse.Bkp = odpovedHlavickaType.bkp;
					eetResponse.TestRun = o.test;
				}

				return eetResponse;
			}
			catch (Exception ex)
			{
				logError("Could not call the service.", ex);
				eetResponse = new EETResponse(ResultTypes.ClientFailure, "An error has occured while calling the server. Please, check the log for more information.");
				return eetResponse;
			}
		}

		public async Task<EETResponse> SendDataAsync(EETData data)
		{
			EETResponse eetResponse;
			try
			{
				var client = prepareClient();
				var response = await client.OdeslaniTrzbyAsync(data.GetRequestData(taxpayersCertificate));
			
				return new EETResponse(ResultTypes.Success, response.Hlavicka.uuid_zpravy);
			}
			catch (Exception ex)
			{
				logError("Could not call the service.", ex);
				eetResponse = new EETResponse(ResultTypes.ClientFailure, "An error has occured while calling the server. Please, check the log for more information.");
				return eetResponse;
			}
		}
	}
}
