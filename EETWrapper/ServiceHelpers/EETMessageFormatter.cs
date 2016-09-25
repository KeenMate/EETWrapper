using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

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

		public object DeserializeReply(Message message, object[] parameters)
		{
			return this.formatter.DeserializeReply(message, parameters);
		}
	}
}