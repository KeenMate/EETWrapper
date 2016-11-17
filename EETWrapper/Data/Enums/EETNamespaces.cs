namespace EETWrapper.Data
{
	static class EETNamespaces
	{
		/// <summary>
		/// http://schemas.xmlsoap.org/soap/envelope/
		/// </summary>
		public const string SOAP11Envelope = "http://schemas.xmlsoap.org/soap/envelope/";

		/// <summary>
		/// http://fs.mfcr.cz/eet/schema/v3
		/// </summary>
		public const string EETSchemaV3 = "http://fs.mfcr.cz/eet/schema/v3";

		#region WSSecurity and signatures

		/// <summary>
		/// http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd
		/// </summary>
		public const string WSSecurityUtility =
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

		/// <summary>
		/// http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd
		/// </summary>
		public const string WSSecurityExtensions =
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

		/// <summary>
		/// http://www.w3.org/2000/09/xmldsig#
		/// </summary>
		public const string Signature = "http://www.w3.org/2000/09/xmldsig#";

		/// <summary>
		/// http://www.w3.org/2001/04/xmldsig-more#rsa-sha256
		/// </summary>
		public const string Signature_RSASHA256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

		/// <summary>
		/// http://www.w3.org/2001/10/xml-exc-c14n#
		/// </summary>
		public const string CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";

		/// <summary>
		/// http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary
		/// </summary>
		public const string Base64BinaryEncoding =
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";

		/// <summary>
		/// http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3
		/// </summary>
		public const string x509v3ValueType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
		#endregion

		#region XML Schema

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema
		/// </summary>
		public const string XMLSchema = "http://www.w3.org/2001/XMLSchema";

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema-instance
		/// </summary>
		public const string XMLSchemaInstance = "http://www.w3.org/2001/XMLSchema-instance";
		

		#endregion
	}
}
