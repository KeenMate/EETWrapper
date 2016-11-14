using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace EETWrapper.SignatureBehavior
{
	public class SignMessageWithCertificateBehavior : IEndpointBehavior
	{
		public X509Certificate2 Certificate { get; private set; }

		public SignMessageWithCertificateBehavior(X509Certificate2 certificate)
		{
			Certificate = certificate;
		}

		public void Validate(ServiceEndpoint endpoint)
		{
			
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			SignMessageWithCertificateInspector inspector = new SignMessageWithCertificateInspector(Certificate);
			clientRuntime.MessageInspectors.Add(inspector);
		}
	}
}