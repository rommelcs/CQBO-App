using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Net;

namespace CQBO_App.CME
{
	public class CME_API
	{
		public string CreateLink(int iMode)
		{
			//Product LInk = https://api.refdata.cmegroup.com/v1/products/
			//Instrument LInk = https://api.refdata.cmegroup.com/v1/instruments/
			//TogetInstrument per product = https://api.refdata.cmegroup.com/v1/products/23GTYEWOXUKD/instruments

			string strResult = "";
			string cmeMainApiLink = ConfigurationManager.AppSettings.Get("CMEAPILink"); 
			string mode = "";

			switch (iMode)
			{
				case 1:
					mode = "products";
					break;
				case 2:
					mode = "instruments";
					break;
				default:
					mode = "";
					break;
			}

			strResult = cmeMainApiLink + mode + "/";

			return strResult;
		}
	}
}
