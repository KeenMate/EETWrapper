using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using EETWrapper.Data;
using EETWrapper.EETService_v311;

namespace EETWrapper.ServiceHelpers
{
	internal class EETMessageFormatter : IClientMessageFormatter
	{
		private readonly IClientMessageFormatter formatter;

		public EETMessageFormatter(IClientMessageFormatter formatter)
		{
			this.formatter = formatter;
		}

		public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
		{
			var message = this.formatter.SerializeRequest(messageVersion, parameters);
			
			return new EETMessage(message, parameters);
		}


		/// <summary>
		/// Parsing according to http://www.thefrankes.com/wp/?p=2343
		/// </summary>
		/// <param name="message"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public object DeserializeReply(Message message, object[] parameters)
		{
			OdeslaniTrzbyResponse response = new OdeslaniTrzbyResponse();

			XPathDocument doc = new XPathDocument(message.GetReaderAtBodyContents());

			var nav = doc.CreateNavigator();

			var manager = new XmlNamespaceManager(new NameTable());
			manager.AddNamespace("eet", EETNamespaces.EETSchemaV3);


			var header = nav.SelectSingleNode("eet:Odpoved/eet:Hlavicka", manager);

			response.Hlavicka = new OdpovedHlavickaType();
			response.Hlavicka.uuid_zpravy = header.GetAttribute("uuid_zpravy", "");
			response.Hlavicka.bkp = header.GetAttribute("bkp", "");
			response.Hlavicka.dat_prij = DateTime.Parse(header.GetAttribute("dat_prij", ""));

			var validResponse = nav.SelectSingleNode("eet:Odpoved/eet:Potvrzeni", manager);
			if (validResponse != null)
				response.Item = new OdpovedPotvrzeniType();


			//xdoc.ImportNode(nav.SelectSingleNode("Odpoved").MoveToFirstChild(), false);
			XmlSerializer ser = new XmlSerializer(typeof(OdeslaniTrzbyResponse));
			
			var deserialize = ser.Deserialize(nav.ReadSubtree());

			return this.formatter.DeserializeReply(message, parameters);
		}
	}
}