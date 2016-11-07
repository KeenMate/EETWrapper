namespace EETWrapper.Data
{
	public enum WarningTypes
	{
		/// <summary>
		/// The taxpayer identification codes (DIČ) in the message and certificate differ
		/// </summary>
		TaxpayersIDDoesNotMatchCertificate = 1,

		/// <summary>
		/// Invalid structure of tax identification number of the appointing taxpayer
		/// </summary>
		InvalidTaxIdentificationNumber = 2,

		/// <summary>
		/// Invalid value of Taxpayer’s signature code (PKP)
		/// </summary>
		InvalidTaxpayersSignature = 3,

		/// <summary>
		/// The date and time of sale is newer than the date and time of data message acceptance
		/// </summary>
		TimeOfSaleIsTooNew = 4,


		/// <summary>
		/// The date and time of sale is far in the past
		/// </summary>
		TimeOfSaleIsFarInPast = 5
	}
}