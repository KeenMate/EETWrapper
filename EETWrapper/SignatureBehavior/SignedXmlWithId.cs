using System.Security.Cryptography.Xml;
using System.Xml;

namespace EETWrapper.SignatureBehavior
{
	public class SignedXmlWithId : SignedXml
	{
		public SignedXmlWithId(XmlDocument xml)
				: base(xml)
		{
		}

		public SignedXmlWithId(XmlElement xmlElement)
				: base(xmlElement)
		{
		}

		public override XmlElement GetIdElement(XmlDocument doc, string id)
		{
			// check to see if it's a standard ID reference
			XmlElement idElem = base.GetIdElement(doc, id);

			if (idElem == null)
			{
				// I've just hardcoded it for the time being, but should be using an XPath expression here, and the id that is passed in
				idElem = (XmlElement)doc.GetElementsByTagName("Body", "http://schemas.xmlsoap.org/soap/envelope/")[0];
			}

			return idElem;
		}
	}
}