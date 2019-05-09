using EETWrapper.Assets;
using EETWrapper.Data;
using EETWrapper.EETService_v311;
using EETWrapper.Extensions;
using EETWrapper.Interfaces;
using EETWrapper.SignatureBehavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EETWrapper.Mappers;

namespace EETWrapper
{
	public class EETProvider
	{
		private readonly Guid correlationId;
		private readonly ILogger logger;
		private X509Certificate2 taxpayersCertificate;
		private readonly string customEndPointName;

		//public event EventHandler<LogEventArgs> OnLogChange;

		static EETProvider()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}

		public EETProvider(Guid correlationId, ILogger logger)
		{
			this.correlationId = correlationId;
			this.logger = logger;
		}

		public EETProvider(Guid correlationId, ILogger logger, string certName) : this(correlationId, logger)
		{
			SetCertificate(certName);
		}

		public EETProvider(Guid correlationId, ILogger logger, X509Certificate2 taxpayersCertificate, string customEndPointName) : this(correlationId, logger)
		{
			this.taxpayersCertificate = taxpayersCertificate;
			this.customEndPointName = customEndPointName;
		}


		/// <summary>
		/// X509 taxpayer's certificate
		/// </summary>
		/// <param name="taxpayersCertificate"></param>
		public EETProvider(Guid correlationId, ILogger logger, X509Certificate2 taxpayersCertificate) : this(correlationId, logger)
		{
			if (taxpayersCertificate == null)
				throw new ArgumentOutOfRangeException(nameof(taxpayersCertificate), Exceptions.CertificateCannotNotBeNull);
			this.taxpayersCertificate = taxpayersCertificate;
		}

		protected void SetCertificate(string certName)
		{
			logger.Info(Messages.CertificateLookup.Fill(certName));
			taxpayersCertificate = fetchCertificate(certName);
		}

		#region Private

		private X509Certificate2 fetchCertificate(string certName)
		{
			logger.Info(Messages.OpeningPersonalCurrentUserCertStore);

			X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			try
			{
				store.Open(OpenFlags.ReadOnly);

				logger.Info(Messages.SearchingForCertByName);
				X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName,
					certName, false);

				logger.Trace($"{correlationId} - Certificates found: {cers.Count}");

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
			logger.Trace($"{correlationId} - Creating EETClient object " + (string.IsNullOrEmpty(customEndPointName) ? "with default" : $"with custom endpoint name: {customEndPointName}"));
			EETClient client = string.IsNullOrEmpty(customEndPointName) ? new EETClient() : new EETClient(customEndPointName);

			client.Endpoint.Behaviors.Add(new SignMessageWithCertificateBehavior(taxpayersCertificate));

			return client;
		}

		#endregion

		public EETResponse SendData(EETData data)
		{
			logger.Info(Messages.SendingEETData);
			EETResponse eetResponse;
			try
			{
				logger.Trace($"{correlationId} - Preparing WCF client");
				var client = prepareClient();

				object response;
				OdpovedVarovaniType[] warnings;

				var dataMapper = new EETDataMappers(correlationId, logger, taxpayersCertificate, data);

				var odpoved = client.OdeslaniTrzby(dataMapper.GetRequestHeader(), dataMapper.GetRequestBody(), dataMapper.GetRequestCheckCodes(), out response, out warnings);

				if (response is OdpovedChybaType)
				{
					OdpovedChybaType o = (OdpovedChybaType)response;
					var errorMessage = Messages.ReceivedError.Fill($"({o.kod}) {o.Text[0]}");
					logger.Warn($"{correlationId} - {errorMessage}");

					eetResponse = new EETResponse(ResultTypes.Error, new List<EETWarning>() { new EETWarning(o.kod, o.Text[0]) });
					eetResponse.ResponseTime = odpoved.dat_odmit;
					eetResponse.Warnings.AddRange(mapWarnings(warnings, true));
					eetResponse.Message = mapTextResponse(o.Text);
				}
				else
				{
					OdpovedPotvrzeniType o = (OdpovedPotvrzeniType)response;
					logger.Info(Messages.ReceivedSuccess.Fill(o.fik, odpoved.bkp));
					eetResponse = new EETResponse(warnings?.Length>0?ResultTypes.SuccessWithWarnings:ResultTypes.Success, new Guid(odpoved.uuid_zpravy));
					eetResponse.Warnings.AddRange(mapWarnings(warnings));
					eetResponse.ResponseTime = odpoved.dat_prij;
					eetResponse.Fik = o.fik;
					eetResponse.Bkp = odpoved.bkp;
					eetResponse.TestRun = o.test;

					eetResponse.Warnings.ForEach(x=> logger.Warn($"{correlationId} - Call's warning: {x.Code} - {x.Text}"));
				}

				return eetResponse;
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"{correlationId} - Could not call the service.");
				eetResponse = new EETResponse(ResultTypes.ClientFailure, "An error has occured while calling the server. Please, check the log for more information.");
				eetResponse.TestRun = data.TestRun;
				return eetResponse;
			}
		}

		private static string mapTextResponse(string[] o)
		{
			return o.Aggregate((f, s) => $"{f}; {s}");
		}

		private static IEnumerable<EETWarning> mapWarnings(OdpovedVarovaniType[] varovani, bool isError = false)
		{
			return varovani?.Select(warning =>
				new EETWarning(isError, warning.kod_varov, warning.Text?.Aggregate((f, s) => $"{f}; {s}")));
		}

		public async Task<EETResponse> SendDataAsync(EETData data)
		{
			EETResponse eetResponse;
			try
			{
				var client = prepareClient();

				var dataMapper = new EETDataMappers(correlationId, logger, taxpayersCertificate, data);

				var response = await client.OdeslaniTrzbyAsync(dataMapper.GetRequestData());

				return new EETResponse(ResultTypes.Success, response.Hlavicka.uuid_zpravy);
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"{correlationId} - Could not call the service.");
				eetResponse = new EETResponse(ResultTypes.ClientFailure, "An error has occured while calling the server. Please, check the log for more information.");
				return eetResponse;
			}
		}
	}
}
