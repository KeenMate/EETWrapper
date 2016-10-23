using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EETWrapper.Assets;
using EETWrapper.Data;
using EETWrapper.EETService_v3;
using EETWrapper.Interfaces;
using EETWrapper.Mappers;

namespace EETWrapper
{
	public class EETProvider
	{
		private X509Certificate2 taxpayersCertificate;
		private readonly bool withLogging;

		public event EventHandler<LogEventArgs> OnLogChange;

		static EETProvider()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}

		public EETProvider(string certName)
		{
			taxpayersCertificate = fetchCertificate(certName);

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
			EETClient client = new EETClient();

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

		private void logError(string message)
		{
			if (OnLogChange != null)
				OnLogChange.Invoke(this, new LogEventArgs(message, LogEventArgs.LogLevels.Error));
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
			var client = prepareClient();

			object response;
			OdpovedVarovaniType[] warnings;

			client.OdeslaniTrzby(data.GetRequestHeader(), data.GetRequestBody(), data.GetRequestCheckCodes(taxpayersCertificate), out response, out warnings);

			return new EETResponse();
		}

		public async Task<EETResponse> SendDataAsync(EETData data)
		{
			var client = prepareClient();
			var response = await client.OdeslaniTrzbyAsync(data.GetRequestData(taxpayersCertificate));
			
			return new EETResponse();
		}
	}
}
