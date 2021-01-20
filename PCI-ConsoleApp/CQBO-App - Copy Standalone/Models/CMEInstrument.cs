using System;
using System.Collections.Generic;
using System.Text;

namespace CQBO_App.Models
{
	class CMEInstrument
	{

		public class InstrumentMain
		{
			public _Embedded _embedded { get; set; }
			public _Links1 _links { get; set; }
			public _Metadata _metadata { get; set; }
		}

		public class _Embedded
		{
			public Instrument[] instruments { get; set; }
		}

		public class Instrument
		{
			public string guid { get; set; }
			public long guidInt { get; set; }
			public long productGuidInt { get; set; }
			public string instrumentName { get; set; }
			public string globexSecurityId { get; set; }
			public string globexSymbol { get; set; }
			public string positionRemovalDate { get; set; }
			public DateTime lastUpdated { get; set; }
			public string lastTradeDate { get; set; }
			public object lastNoticeDate { get; set; }
			public object lastDeliveryDate { get; set; }
			public string globexLastTradeDate { get; set; }
			public string globexFirstTradeDate { get; set; }
			public string firstTradeDate { get; set; }
			public object firstNoticeDate { get; set; }
			public object firstDeliveryDate { get; set; }
			public string finalSettlementDate { get; set; }
			public object initialInventoryDueDate { get; set; }
			public object lastInventoryDueDate { get; set; }
			public object lastEfpDate { get; set; }
			public string strikePxCcy { get; set; }
			public string putCallIndicator { get; set; }
			public string priceBandDl { get; set; }
			public string priceBand { get; set; }
			public string zeroPriceEligible { get; set; }
			public object originalContractSize { get; set; }
			public string lowLimit { get; set; }
			public string highLimit { get; set; }
			public string contractMonth { get; set; }
			public string flexIndicator { get; set; }
			public string cfiCode { get; set; }
			public string valuationMethod { get; set; }
			public string strikePx { get; set; }
			public float? tradeTick { get; set; }
			public float? settlementTick { get; set; }
			public object nonConsecutiveMonthSpreadTick { get; set; }
			public object spreadTick { get; set; }
			public float? vttLowTick { get; set; }
			public float? vttHighTick { get; set; }
			public float? vttPriceThreshold { get; set; }
			public object couponRate { get; set; }
			public bool isUserDefined { get; set; }
			public string tccAlias { get; set; }
			public string itcAlias { get; set; }
			public string gbxAlias { get; set; }
			public string clrAlias { get; set; }
			public object lastIntDate { get; set; }
			public object fnlInvDate { get; set; }
			public object firstPositionDate { get; set; }
			public object pxUomCcy { get; set; }
			public string uomCcy { get; set; }
			public string isTasProduct { get; set; }
			public string isBticProduct { get; set; }
			public string isTamProduct { get; set; }
			public string isSyntheticInstrument { get; set; }
			public string exchangeClearing { get; set; }
			public _Links _links { get; set; }
		}

		public class _Links
		{
			public Self self { get; set; }
			public Underlyinginstruments underlyingInstruments { get; set; }
			public Product product { get; set; }
		}

		public class Self
		{
			public string href { get; set; }
		}

		public class Underlyinginstruments
		{
			public string href { get; set; }
		}

		public class Product
		{
			public string href { get; set; }
		}

		public class _Links1
		{
			public Next next { get; set; }
		}

		public class Next
		{
			public string href { get; set; }
		}

		public class _Metadata
		{
			public int totalElements { get; set; }
			public string scrollId { get; set; }
			public string type { get; set; }
			public int totalPages { get; set; }
		}

	}
}
