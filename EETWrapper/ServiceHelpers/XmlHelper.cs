using EETWrapper.EETService_v311;
using System;
using System.Xml;


namespace EETWrapper.ServiceHelpers
{
	internal static class XmlHelper
	{
		public static void WriteElement(this XmlDictionaryWriter writer, string name, Action<XmlDictionaryWriter> insideElement, string ns = "")
		{
			if (string.IsNullOrEmpty(ns))
			{
				writer.WriteStartElement(name);
			}
			else
			{
				writer.WriteStartElement(name, ns);
			}
			insideElement(writer);
			writer.WriteEndElement();
		}

		public static void WriteEETAttribute(this XmlDictionaryWriter writer, string key, object value, Func<bool> isSpecifiedFunc = null)
		{
			if (isSpecifiedFunc != null && !isSpecifiedFunc())
			{
				return;
			}

			string v = "";

			if (value is string)
			{
				v = (string)value;
			}
			else if (value is DateTime)
			{
				// 2019-05-07T19:11:31+02:00
				var data = ((DateTime)value);

				var date = $@"{data.ToUniversalTime():yyyy-MM-ddTHH:mm:ss}{data:zzz}";
				v = date;
			}
			else if (value is decimal)
			{
				v = ((decimal)value).ToString("F2", EETMessage.EETDecimalFormat);
			}
			else if (value is PkpElementType || value is BkpElementType)
			{
				v = ((PkpElementType)value).Text[0];
			}
			else
			{
				v = value.ToString().ToLower();
			}

			writer.WriteAttributeString(key, v);
		}
	}
}