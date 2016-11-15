using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;
using EETWrapper.ServiceHelpers;

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
			MessageBuffer msgbuf = request.CreateBufferedCopy(int.MaxValue);
			Message tmpMessage = msgbuf.CreateMessage();
			XmlDictionaryReader fullBody = tmpMessage.GetReaderAtBodyContents();

			XmlDocument xdoc = new XmlDocument();

			xdoc.Load(fullBody);

			fullBody.Close();


			var guid = Guid.NewGuid();
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(tmpMessage.ToString());
			

			// Add the required namespaces to the SOAP Envelope element, if I don't do this, the web service I'm calling returns an error
			string soapSecNS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
			string soapEnvNS = "http://schemas.xmlsoap.org/soap/envelope/";
			http://www.w3.org/2000/09/xmldsig#
					 //Get the header element, so that we can add the digital signature to it
			XmlNode headerNode = doc.GetElementsByTagName("Header", soapEnvNS)[0];

			// Set the ID attribute on the body element, so that we can reference it later
			XmlNode bodyNode = doc.GetElementsByTagName("Body", soapEnvNS)[0];

			bodyNode.RemoveAll();
			var importedNode = doc.ImportNode(xdoc.DocumentElement, true);
			bodyNode.AppendChild(importedNode);
			((XmlElement)bodyNode).RemoveAllAttributes();
			((XmlElement)bodyNode).SetAttribute("xmlns:wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
			((XmlElement)bodyNode).SetAttribute("id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", $"Body-{guid:D}");

			XmlWriterSettings settings2 = new XmlWriterSettings();
			settings2.Encoding = new System.Text.UTF8Encoding(false);

			// Load the certificate we want to use for signing
			SignedXmlWithId signedXml = new SignedXmlWithId(doc);

			// Note that this will return a Basic crypto provider, with only SHA-1 support
			var privKey = (RSACryptoServiceProvider)Certificate.PrivateKey;
			// Force use of the Enhanced RSA and AES Cryptographic Provider with openssl-generated SHA256 keys
			var enhCsp = new RSACryptoServiceProvider().CspKeyContainerInfo;
			var cspparams = new CspParameters(enhCsp.ProviderType, enhCsp.ProviderName, privKey.CspKeyContainerInfo.KeyContainerName);

			RSACryptoServiceProvider key = new RSACryptoServiceProvider(cspparams);
		
			signedXml.SigningKey = key;

			//< ds:SignatureMethod Algorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256" />
			signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
			signedXml.Signature.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
			//Populate the KeyInfo element correctly, with the public cert and public key
			Signature sigElement = signedXml.Signature;
			KeyInfoX509Data x509Data = new KeyInfoX509Data(Certificate);

			KeyInfoNode keyNode = new KeyInfoNode();

			//< wsse:BinarySecurityToken EncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary" ValueType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3" wsu: Id = "X509-A79845F15C5549CA0514761283545351" >
			//	MIIEmDCCA4CgAwIBAgIEdHOXJzANBgkqhkiG9w0BAQsFADB3MRIwEAYKCZImiZPyLGQBGRYCQ1oxQzBBBgNVBAoMOsSMZXNrw6EgUmVwdWJsaWthIOKAkyBHZW5lcsOhbG7DrSBmaW5hbsSNbsOtIMWZZWRpdGVsc3R2w60xHDAaBgNVBAMTE0VFVCBDQSAxIFBsYXlncm91bmQwHhcNMTYwOTMwMDkwMzU5WhcNMTkwOTMwMDkwMzU5WjBDMRIwEAYKCZImiZPyLGQBGRYCQ1oxEzARBgNVBAMTCkNaMDAwMDAwMTkxGDAWBgNVBA0TD3ByYXZuaWNrYSBvc29iYTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAJnNUPW8rAlLi2KAwu12W1vqLj02mWIifq / Jp0 / tUjf9B8RpkDAD3GOqDdVuHSfxej92WiEouDy7X8uXzIDdZu4pXA3t3KntxM8rAlu2U6SqtF3kTR + AJCdwfkM53U3z4 / qoyKqdQ8lGuMxJKs7X5uIjcY / UDSXMK9OTmXRhndjYcX1oILr5F2ONf1Z0kWyl / S9wI0cl0gQ1F91mzqgnlH80u2inMmmBp42ndR4TGS1nvjer5D73bkLg07TdeqnUg609WwjUJN96OKZMsKXzBMzt09NbhQcABWnAWbRTSVhsAdDO8vfmWx2C + gXUlkIvtO + 9fbj81GS1xdNoAkpARUcCAwEAAaOCAV4wggFaMAkGA1UdEwQCMAAwHQYDVR0OBBYEFL / 0b0Iw6FY33UT8iJEy1V7nZVR6MB8GA1UdIwQYMBaAFHwwdqzM1ofR7Mkf4nAILONf3gwHMA4GA1UdDwEB / wQEAwIGwDBjBgNVHSAEXDBaMFgGCmCGSAFlAwIBMAEwSjBIBggrBgEFBQcCAjA8DDpUZW50byBjZXJ0aWZpa8OhdCBieWwgdnlkw6FuIHBvdXplIHBybyB0ZXN0b3ZhY8OtIMO6xI1lbHkuMIGXBgNVHR8EgY8wgYwwgYmggYaggYOGKWh0dHA6Ly9jcmwuY2ExLXBnLmVldC5jei9lZXRjYTFwZy9hbGwuY3JshipodHRwOi8vY3JsMi5jYTEtcGcuZWV0LmN6L2VldGNhMXBnL2FsbC5jcmyGKmh0dHA6Ly9jcmwzLmNhMS1wZy5lZXQuY3ovZWV0Y2ExcGcvYWxsLmNybDANBgkqhkiG9w0BAQsFAAOCAQEAvXdWsU + Ibd1VysKnjoy6RCYVcI9 + oRUSSTvQQDJLFjwn5Sm6Hebhci8ERGwAzd2R6uqPdzl1KCjmHOitypZ66e +/ e9wj3BaDqgBKRZYvxZykaVUdtQgG0819JZmiXTbGgOCKiUPIXO80cnP7U1ZPkVNV7WZwh0I2k / fg1VLTI5HA / x4BeD77wiEOExa7eqePJET0jpTVK3LxSW59LLIJROh4 / kfKQbTvDL5Ypw8WagAMVCPvWnGJIcUru + ApLU4pZD9bdHSa1Ib4LpFhtWrkHYM / XqKbj2bNKKjTo5T3sU0Bf2QD3QzkmcjlNVG0V + qAgimwTdPueU / mtExw + 7z1 / A ==
			//</ wsse:BinarySecurityToken >

			//< wsse:SecurityTokenReference xmlns:wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" xmlns: wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd" wsu: Id = "STR-A79845F15C5549CA0514761283545513" >
			// < wsse:Reference URI = "#X509-A79845F15C5549CA0514761283545351" ValueType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3" />
			//</ wsse:SecurityTokenReference >


			var export = Certificate.Export(X509ContentType.Cert);
			var base64 = Convert.ToBase64String(export);

			var binaryTokenKey = $"X509-{Guid.NewGuid():N}";

			XmlElement binaryToken = doc.CreateElement("wsse", "BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
			binaryToken.SetAttribute("EncodingType",
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
			binaryToken.SetAttribute("ValueType",
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
			binaryToken.SetAttribute("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd",
				binaryTokenKey);
			binaryToken.InnerText = base64;

			XmlElement str = doc.CreateElement("wsse", "SecurityTokenReference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			XmlElement r = doc.CreateElement("wsse", "Reference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			r.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
			r.SetAttribute("URI", $"#{binaryTokenKey}");
			str.AppendChild(r);

			keyNode.Value = str;

			sigElement.KeyInfo.AddClause(keyNode);
			//sigElement.KeyInfo.AddClause(x509Data);

			//RSAKeyValue rsaKeyValue = new RSAKeyValue((RSA)Certificate.PublicKey.Key);
			//sigElement.KeyInfo.AddClause(rsaKeyValue);

			// Create a reference to be signed, only sign the body of the SOAP request, which we have given an 
			// ID attribute to, in order to reference it correctly here
			Reference reference = new Reference();
			reference.Uri = $"#Body-{guid:D}";
			// Add an enveloped transformation to the reference.
			XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
			env.Algorithm = "http://www.w3.org/2001/10/xml-exc-c14n#";
			reference.AddTransform(env);
			reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

			// Add the reference to the SignedXml object.
			signedXml.AddReference(reference);

			// Compute the signature.
			signedXml.ComputeSignature();

			// Get the XML representation of the signature and save
			// it to an XmlElement object.
			XmlElement xmlDigitalSignature = signedXml.GetXml();

					//< wsse:Security xmlns:wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" xmlns: wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd" soap: mustUnderstand = "1" >


						 XmlElement security = doc.CreateElement("wsse", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			security.SetAttribute("xmlns:wsu",
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

			security.SetAttribute("mustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/", "1");
			//binaryToken.InnerText = Convert.ToBase64String(Certificate.GetPublicKey(), Base64FormattingOptions.None);

			security.AppendChild(binaryToken);

			XmlElement soapSignature = doc.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
			soapSignature.Prefix = "ds";
			soapSignature.AppendChild(xmlDigitalSignature);

			security.AppendChild(xmlDigitalSignature);

			headerNode.AppendChild(security);

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