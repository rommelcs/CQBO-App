using System;
using System.Collections.Generic;
using System.Text;

namespace ExtractCME_EODTrades.Model
{
	public class CQAPIModels
	{
		public class CMEComparisonModel
		{
			public string ExchangeCode { get; set; }
			public Int32 CQVol_All { get; set; }
			public Int32 ECQVol_NoMOS { get; set; }
			public Int32 ExchangeVol { get; set; }

		}

	}
}
