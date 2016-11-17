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

			if(header.GetAttribute("dat_prij", "")!=String.Empty)
			{
				response.Hlavicka.dat_prij = DateTime.Parse(header.GetAttribute("dat_prij", ""));
				response.Hlavicka.dat_prijSpecified = true;
			}

			if(header.GetAttribute("dat_odmit", "") != String.Empty)
			{
				response.Hlavicka.dat_odmit = DateTime.Parse(header.GetAttribute("dat_odmit", ""));
				response.Hlavicka.dat_odmitSpecified = true;
			}

			var validResponse = nav.SelectSingleNode("eet:Odpoved/eet:Potvrzeni", manager);
			if (validResponse != null)
			{
				//<eet:Potvrzeni fik="9e8e30cd-3d31-4aab-92ad-30ca0912689a-ff" test="true" xmlns:eet="http://fs.mfcr.cz/eet/schema/v3" />

				var odpovedPotvrzeniType = new OdpovedPotvrzeniType();
				odpovedPotvrzeniType.fik = validResponse.GetAttribute("fik", "");
				odpovedPotvrzeniType.test = Convert.ToBoolean(validResponse.GetAttribute("test", ""));
				odpovedPotvrzeniType.testSpecified = true;

				response.Item = odpovedPotvrzeniType;
				
				var warningResponse = nav.Select("eet:Odpoved/eet:Varovani", manager);

				if (warningResponse.Count > 0)
				{
					foreach (XPathNavigator warningNav in warningResponse)
					{
						OdpovedVarovaniType warning = new OdpovedVarovaniType();

						warning.kod_varov = Convert.ToInt16(warningNav.GetAttribute("kod_varov", ""));
						warning.Text = new[] {warningNav.Value};
					}
				}

			}
			else
			{
				var errorResponse = nav.SelectSingleNode("eet:Odpoved/eet:Chyba", manager);
				if (errorResponse != null)
				{
					var error = new OdpovedChybaType();
					error.kod = Convert.ToInt16(errorResponse.GetAttribute("kod", ""));
					error.test = Convert.ToBoolean(header.GetAttribute("test", ""));
					error.testSpecified = true;

					response.Item = error;
				}
			}

			return response;
		}
	}
}