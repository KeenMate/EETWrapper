using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace EETWrapper.ServiceHelpers
{
	[AttributeUsage(AttributeTargets.Method)]
	internal class EETFormatMessageAttribute : Attribute, IOperationBehavior
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