using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace EETWrapper.SignatureBehavior
{
	public class SignMessageWithCertificateInspector : IClientMessageInspector
	{
		public X509Certificate2 Certificate { get; private set; }

		public SignMessageWithCertificateInspector(X509Certificate2 certificate)
		{
			Certificate = certificate;
		}

		public object BeforeSendRequest(ref Message request, IClientChannel channel)
		{
			var guid = Guid.NewGuid();
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(request.ToString());

			// Add the required namespaces to the SOAP Envelope element, if I don't do this, the web service I'm calling returns an error
			string soapSecNS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
			string soapEnvNS = "http://schemas.xmlsoap.org/soap/envelope/";
			http://www.w3.org/2000/09/xmldsig#
			//Get the header element, so that we can add the digital signature to it
			XmlNode headerNode = doc.GetElementsByTagName("Header", soapEnvNS)[0];

			

			// Set the ID attribute on the body element, so that we can reference it later
			XmlNode bodyNode = doc.GetElementsByTagName("Body", soapEnvNS)[0];

			((XmlElement)bodyNode).RemoveAllAttributes();
			((XmlElement)bodyNode).SetAttribute("id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", $"Body-{guid:D}");

			XmlWriterSettings settings2 = new XmlWriterSettings();
			settings2.Encoding = new System.Text.UTF8Encoding(false);

			// Load the certificate we want to use for signing
			SignedXmlWithId signedXml = new SignedXmlWithId(doc);
			
			signedXml.SigningKey = Certificate.PrivateKey;

			//Populate the KeyInfo element correctly, with the public cert and public key
			Signature sigElement = signedXml.Signature;
			KeyInfoX509Data x509Data = new KeyInfoX509Data(Certificate);
			sigElement.KeyInfo.AddClause(x509Data);

			//RSAKeyValue rsaKeyValue = new RSAKeyValue((RSA)Certificate.PublicKey.Key);
			//sigElement.KeyInfo.AddClause(rsaKeyValue);

			// Create a reference to be signed, only sign the body of the SOAP request, which we have given an 
			// ID attribute to, in order to reference it correctly here
			Reference reference = new Reference();
			reference.Uri = $"#Body-{guid:D}";
			
			// Add the reference to the SignedXml object.
			signedXml.AddReference(reference);

			// Compute the signature.
			signedXml.ComputeSignature();

			// Get the XML representation of the signature and save
			// it to an XmlElement object.
			XmlElement xmlDigitalSignature = signedXml.GetXml();

			XmlElement security = doc.CreateElement("wsse", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

			var export = Certificate.Export(X509ContentType.Cert);
			var base64 = Convert.ToBase64String(export);
			XmlElement binaryToken = doc.CreateElement("wsse", "BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
			binaryToken.InnerText = base64;
			//binaryToken.InnerText = Convert.ToBase64String(Certificate.GetPublicKey(), Base64FormattingOptions.None);




			security.AppendChild(binaryToken);

			XmlElement soapSignature = doc.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
			soapSignature.Prefix = "ds";
			soapSignature.AppendChild(xmlDigitalSignature);

			headerNode.AppendChild(security);
			headerNode.AppendChild(xmlDigitalSignature);

			// Make sure the byte order mark doesn't get written out
			XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
			Encoding encoderWithoutBOM = new System.Text.UTF8Encoding(false);

			System.IO.MemoryStream ms = new System.IO.MemoryStream(encoderWithoutBOM.GetBytes(doc.InnerXml));

			XmlDictionaryReader xdr = XmlDictionaryReader.CreateTextReader(ms, encoderWithoutBOM, quotas, null);

			//Create the new message, that has the digital signature in the header
			Message newMessage = Message.CreateMessage(xdr, System.Int32.MaxValue, request.Version);
			request = newMessage;

			return null;
		}

		public void AfterReceiveReply(ref Message reply, object correlationState)
		{
			
		}
	}
}