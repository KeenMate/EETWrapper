using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EETWrapper
{
	public class EETData
	{
		public enum SaleRegimes { Normal, Special}
		
		public bool FirstTry { get; set; } = true;

		/// <summary>
		/// Message's UUID - always unique per message
		/// </summary>
		public Guid UUID { get; } = Guid.NewGuid();

		/// <summary>
		/// Date and time of sending
		/// </summary>
		public DateTime CreationDate { get; } = DateTime.Now;

		/// <summary>
		/// Flag of verification sending mode  
		/// </summary>
		public bool TestRun { get; set; } = false;

		public SaleRegimes SaleRegime { get; set; } = SaleRegimes.Normal;

		/// <summary>
		/// Tax identification number
		/// </summary>
		public string TaxID { get; set; }

		/// <summary>
		/// Appointing taxpayer tax identification number
		/// </summary>
		public string AppointingPayerTaxID { get; set; }

		/// <summary>
		/// Business premises ID
		/// </summary>
		public string BusinessPremisesID { get; set; }

		/// <summary>
		/// Cash register ID
		/// </summary>
		public string CashRegisterID { get; set; }

		/// <summary>
		/// Serial number of receipt
		/// </summary>
		public string ReceiptID { get; set; }

		/// <summary>
		/// Total amount of sale
		/// </summary>
		public decimal TotalAmountOfSale { get; set; }
	}

	public class AdditionalData
	{
		/// <summary>
		/// Total amount for performance exempted from VAT, other performance
		/// </summary>
		public decimal TotalAmountExemptedFromVAT { get; set; }

		/// <summary>
		/// Total tax base ‐ basic VAT rate
		/// </summary>
		public decimal TotalTaxBase_BasicVATRate { get; set; }

		/// <summary>
		/// Total VAT ‐ basic VAT rate
		/// </summary>
		public decimal TotalVAT_BasicVATRate { get; set; }
	}

}
