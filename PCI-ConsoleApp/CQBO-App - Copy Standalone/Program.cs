using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

using CQBO_App.Models;
using CQBO_App.CME;
using System.Configuration;
using CQBO_App.DAL;

namespace CQBO_App
{
	class Program
	{

		static HttpClient client = new HttpClient();
		static CME_API cmeAPI = new CME_API();
		static void Main(string[] args)
		{
			Process();
		}

		static void Process()
		{
			int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CMEAPI_PageSize"));
			string cmeLink = cmeAPI.CreateLink(1);  // Get Products link
			int totalPages = 0;

			Console.WriteLine("Preparing connection....");
			var jsonProd = new WebClient().DownloadString(cmeLink+ "?page=0&size=" + pageSize.ToString());
			CMEProduct.ProductsMain prodobj = JsonConvert.DeserializeObject<CMEProduct.ProductsMain>(jsonProd);
			//CMEInstrument.InstrumentMain instobj = JsonConvert.DeserializeObject<CMEInstrument.InstrumentMain>(jsonInst);
			totalPages = prodobj._metadata.totalPages;

			ProcessProduct(cmeLink, totalPages, pageSize);
			
		}

		static void ProcessProduct(string cmeLink, int totalPages, int pageSize)
		{
			
			AppDAL appDal = new AppDAL();

			Console.WriteLine("Processing Data...");
			for (int i = 0; i < totalPages; i++)
			{
				string linkParam = "?page=" + i.ToString() + "&size=" + pageSize.ToString();
				Console.WriteLine("Collecting Product information from source." + i.ToString() + " of " + totalPages.ToString());
				var jsonProdTmp = new WebClient().DownloadString(cmeLink + linkParam);

				Console.WriteLine("Processing collected data to temp table...");
				CMEProduct.ProductsMain prodobjTmp = JsonConvert.DeserializeObject<CMEProduct.ProductsMain>(jsonProdTmp);

				appDal.SetProductDataToSave(prodobjTmp);
				
			}
		}
	}
}
