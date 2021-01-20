using System;
using System.Collections.Generic;
using System.Text;

namespace CQBO_App.Models
{
	public class CMEProduct
	{
		public class ProductsMain
		{
			public _Embedded _embedded { get; set; }
			public _Links1 _links { get; set; }
			public _Metadata _metadata { get; set; }
		}

		public class _Embedded
		{
			public Product[] products { get; set; }
		}

		public class Product
		{
			public string productGuid { get; set; }
			public long productGuidInt { get; set; }
			public string productName { get; set; }
			public string securityType { get; set; }
			public string clearingSymbol { get; set; }
			public string masterSymbol { get; set; }
			public string exchangeClearing { get; set; }
			public string exchangeGlobex { get; set; }
			public string assetClass { get; set; }
			public string assetSubClass { get; set; }
			public string sector { get; set; }
			public string subSector { get; set; }
			public string isTasProduct { get; set; }
			public string isBticProduct { get; set; }
			public string isTamProduct { get; set; }
			public string rfqCrossEligible { get; set; }
			public string massQuoteEligible { get; set; }
			public string dailyFlag { get; set; }
			public string settlePxCcy { get; set; }
			public string efrEligible { get; set; }
			public object floorPutSymbol { get; set; }
			public string blockTradeEligible { get; set; }
			public string efpEligible { get; set; }
			public string flexEligible { get; set; }
			public int pxUnitOfMeasureQty { get; set; }
			public string negativeStrikeEligible { get; set; }
			public string minGlobexOrdQty { get; set; }
			public string maxGlobexOrdQty { get; set; }
			public string negativePxEligible { get; set; }
			public string pxUnitOfMeasure { get; set; }
			public string tradePxCcy { get; set; }
			public object floorCallSymbol { get; set; }
			public string ebfEligible { get; set; }
			public string fractional { get; set; }
			public string globexGroupCode { get; set; }
			public string itcCode { get; set; }
			public string priceBand { get; set; }
			public string otcEligible { get; set; }
			public string globexGtEligible { get; set; }
			public string variableTickTable { get; set; }
			public string pxQuoteMethod { get; set; }
			public DateTime lastUpdated { get; set; }
			public string marketSegmentId { get; set; }
			public string globexMatchAlgo { get; set; }
			public string ilinkEligible { get; set; }
			public string clearportEligible { get; set; }
			public string unitOfMeasure { get; set; }
			public string globexEligible { get; set; }
			public string floorEligible { get; set; }
			public string variableQtyFlag { get; set; }
			public string strategyType { get; set; }
			public string assignmentMethod { get; set; }
			public float priceMultiplier { get; set; }
			public int mainFraction { get; set; }
			public float unitOfMeasureQty { get; set; }
			public string globexProductCode { get; set; }
			public string defaultMinTick { get; set; }
			public object reducedTickNotes { get; set; }
			public object minQtrlySerialTick { get; set; }
			public object minOutrightTick { get; set; }
			public object minimumTickNote { get; set; }
			public object minClearPortTick { get; set; }
			public object minCabinetTickRules { get; set; }
			public object minimumHalfTick { get; set; }
			public object globexMinTick { get; set; }
			public object minClearPortFloorTick { get; set; }
			public object midcurveTickRules { get; set; }
			public object calendarTickRules { get; set; }
			public string floorSchedule { get; set; }
			public string stdTradingHours { get; set; }
			public string globexSchedule { get; set; }
			public string clearportSchedule { get; set; }
			public object totLtd { get; set; }
			public object totMidcurve { get; set; }
			public object totQuarterly { get; set; }
			public object totSerial { get; set; }
			public object totClearport { get; set; }
			public object totGlobex { get; set; }
			public string totDefault { get; set; }
			public object totFloor { get; set; }
			public object serialListingRules { get; set; }
			public object regularListingRules { get; set; }
			public object quarterlyListingRules { get; set; }
			public object floorListingRules { get; set; }
			public object midcurveOptionsRules { get; set; }
			public string defaultListingRules { get; set; }
			public object globexListingRules { get; set; }
			public string lastDeliveryRules { get; set; }
			public string commodityStandards { get; set; }
			public string settleMethod { get; set; }
			public string markerStlmtRules { get; set; }
			public string limitRules { get; set; }
			public object daysOrHours { get; set; }
			public object reportablePositions { get; set; }
			public string priceQuotation { get; set; }
			public string settlementProcedure { get; set; }
			public string exerciseStyle { get; set; }
			public object settlementAtExpiration { get; set; }
			public string strikePriceInterval { get; set; }
			public string mdp3Channel { get; set; }
			public string globexDisplayFactor { get; set; }
			public string varCabPxHigh { get; set; }
			public string varCabPxLow { get; set; }
			public string clearingCabPx { get; set; }
			public string globexCabPx { get; set; }
			public string isSyntheticProduct { get; set; }
			public string itmOtm { get; set; }
			public string contraryInstructionsAllowed { get; set; }
			public string settleUsingFixingPx { get; set; }
			public string settlementType { get; set; }
			public string optStyle { get; set; }
			public string tradingCutOffTime { get; set; }
			public string isTacoProduct { get; set; }
			public string exerciseStyleAmericanEuropean { get; set; }
			public object subfraction { get; set; }
			public _Links _links { get; set; }
		}

		public class _Links
		{
			public Self self { get; set; }
			public Underlyingproducts underlyingProducts { get; set; }
			public Instruments instruments { get; set; }
			public Overlyingproducts overlyingProducts { get; set; }
			public Bticunderlying bticUnderlying { get; set; }
		}

		public class Self
		{
			public string href { get; set; }
		}

		public class Underlyingproducts
		{
			public string href { get; set; }
		}

		public class Instruments
		{
			public string href { get; set; }
		}

		public class Overlyingproducts
		{
			public string href { get; set; }
		}

		public class Bticunderlying
		{
			public string href { get; set; }
		}

		public class _Links1
		{
			public First first { get; set; }
			public Self1 self { get; set; }
			public Next next { get; set; }
			public Last last { get; set; }
		}

		public class First
		{
			public string href { get; set; }
		}

		public class Self1
		{
			public string href { get; set; }
		}

		public class Next
		{
			public string href { get; set; }
		}

		public class Last
		{
			public string href { get; set; }
		}

		public class _Metadata
		{
			public int size { get; set; }
			public int totalElements { get; set; }
			public int totalPages { get; set; }
			public int number { get; set; }
			public string type { get; set; }
		}

	}


}
