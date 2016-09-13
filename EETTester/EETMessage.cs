

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
			this.message.WriteBodyContents(writer);
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

	public class EETMessageFormatter : IClientMessageFormatter
	{
		private readonly IClientMessageFormatter formatter;

		public EETMessageFormatter(IClientMessageFormatter formatter)
		{
			this.formatter = formatter;
		}

		public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
		{
			Message.CreateMessage()
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