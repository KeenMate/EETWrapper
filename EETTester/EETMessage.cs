

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace EETTester
{
	public class EETMessage : Message
	{
		private string bodyKey = $"Body-{Guid.NewGuid():D}";
		
		private readonly Message message;

		public EETMessage(Message message)
		{
			this.message = message;
		}
		public override MessageHeaders Headers
		{
			get { return this.message.Headers; }
		}
		public override MessageProperties Properties
		{
			get { return this.message.Properties; }
		}
		public override MessageVersion Version
		{
			get { return this.message.Version; }
		}

		protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
		{
			base.OnWriteStartHeaders(writer);
		}

		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Body", "http://schemas.xmlsoap.org/soap/envelope/");
			writer.WriteAttributeString("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", bodyKey);
		}
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			//< Trzba xmlns = "http://fs.mfcr.cz/eet/schema/v3" xmlns: xsi = "http://www.w3.org/2001/XMLSchema-instance" xmlns: xsd = "http://www.w3.org/2001/XMLSchema" >
			//  < Hlavicka uuid_zpravy = "b11a6061-6f8a-4f51-86a4-0e164ffa5a37" dat_odesl = "2016-09-15T22:01:51+02:00" prvni_zaslani = "true" />
			//	< Data dic_popl = "CZ00000019" dic_poverujiciho = "CZ683555118" id_provoz = "273" id_pokl = "/5546/RO24" porad_cis = "0/6460/ZQ42" dat_trzby = "2016-08-05T00:30:12+02:00" celk_trzba = "34113.00" urceno_cerp_zuct = "324" rezim = "0" />
			//	< KontrolniKody >
			//		< pkp digest = "SHA256" cipher = "RSA2048" encoding = "base64" > lspk8ii8PoVdgjvsGlCFly4wYnaM8jKS4CgVIf2vfnRwU4g0FsrPHdF3 / Bzu / EaWpMUUx3Gi7pc0MAZ + YYtFaw9DtYd9IcBzCXf79A4gPpujYJpdtI / qxGt1OSz7r / u + Sdah5ymUOO4p7YQVLcibIkLw7DX3kp6fjDeef6dtJAHHWGG32pwowR02uQIecIiq15IamJ8nikjZL2OtGX / rpOzux0ZytyTSmFWqx / KGGhPh + Cgj2UBMdgb / SVT7gvsME2yDqwgd6xyN1AtuX473IwZN5pgfkRG1 + zWhUTUgZWFdnSo3mpEidsxVat8t5l + RbgZY32W8rYT3EPlM2UaJzg ==</ pkp >
			//		< bkp digest = "SHA1" encoding = "base16" > 00a8afed - 5d07e87a - fa2acdaa - 230f97f2 - 4d13bfc9 </ bkp >
			//	</ KontrolniKody >
			//</ Trzba >

			message.WriteBodyContents(writer);
			writer.WriteElement("Trzba", (t =>
			{
				t.WriteElement("Hlavicka", (h) =>
				{
					h.WriteAttributeString("uuid_zpravy", "b11a6061-6f8a-4f51-86a4-0e164ffa5a37");
					h.WriteAttributeString("dat_odesl", "b11a6061-6f8a-4f51-86a4-0e164ffa5a37");
					h.WriteAttributeString("prvni_zaslani", "true");
				});

				t.WriteElement("Data", d =>
				{
					d.WriteAttributeString("dic_popl", "CZ00000019");
					d.WriteAttributeString("dic_poverujiciho", "CZ683555118");
					d.WriteAttributeString("id_provoz", "273");
					d.WriteAttributeString("id_pokl", "/5546/RO24");
					d.WriteAttributeString("porad_cis", "CZ00000019");
					d.WriteAttributeString("dat_trzby", "CZ00000019");
					d.WriteAttributeString("celk_trzba", "CZ00000019");
					d.WriteAttributeString("urceno_cerp_zuct", "CZ00000019");
				});
			}), "http://fs.mfcr.cz/eet/schema/v3");
			
		}
		protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
		{
			base.OnWriteStartEnvelope(writer);
		}

		protected override void OnWriteMessage(XmlDictionaryWriter writer)
		{
			base.OnWriteMessage(writer);
		}

	}

	public static class XmlChainHelper
	{
		public static void WriteElement(this XmlDictionaryWriter writer, string name, Action<XmlDictionaryWriter> insideElement, string ns = "")
		{
			writer.WriteStartElement(name, ns);
			insideElement(writer);
			writer.WriteEndElement();
		}
	}


	public class EETBodyWriter : BodyWriter
	{
		public EETBodyWriter(bool isBuffered) : base(isBuffered)
		{
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			throw new NotImplementedException();
		}
	}

	public class EETMessageFormatter : IClientMessageFormatter
	{
		private readonly IClientMessageFormatter formatter;

		public EETMessageFormatter(IClientMessageFormatter formatter)
		{
			this.formatter = formatter;
		}

		public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
		{
			Message.CreateMessage(MessageVersion.Soap12,)

			var message = this.formatter.SerializeRequest(messageVersion, parameters);
			return new EETMessage(message);
		}

		public object DeserializeReply(Message message, object[] parameters)
		{
			return this.formatter.DeserializeReply(message, parameters);
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class EETFormatMessageAttribute : Attribute, IOperationBehavior
	{
		public void AddBindingParameters(OperationDescription operationDescription,
				BindingParameterCollection bindingParameters)
		{ }

		public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
		{
			var serializerBehavior = operationDescription.Behaviors.Find<XmlSerializerOperationBehavior>();

			if (clientOperation.Formatter == null)
				((IOperationBehavior)serializerBehavior).ApplyClientBehavior(operationDescription, clientOperation);

			IClientMessageFormatter innerClientFormatter = clientOperation.Formatter;
			
			clientOperation.Formatter = new EETMessageFormatter(innerClientFormatter);
		}

		public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
		{ }

		public void Validate(OperationDescription operationDescription) { }
	}
}