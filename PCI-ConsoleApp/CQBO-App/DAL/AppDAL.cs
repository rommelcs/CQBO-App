using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using CQBO_App.Models;
using Newtonsoft.Json;
using CQBO_App.CME;
using System.Net;

namespace CQBO_App.DAL
{
	public class AppDAL
	{
		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
		
		public void DtExecNonQuerySP(DataTable dt, string spName, string typeTblName)
		{
			conn = new SqlConnection(connString);
			if (dt.Rows.Count > 0)
			{
				SqlCommand cmd = new SqlCommand(spName, conn);

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue(typeTblName, dt);
				cmd.Parameters.Add("@oReturnCode", SqlDbType.VarChar, 10);
				cmd.Parameters["@oReturnCode"].Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@oReturnMsg", SqlDbType.VarChar, 2000);
				cmd.Parameters["@oReturnMsg"].Direction = ParameterDirection.Output;
				conn.Open();
				cmd.ExecuteNonQuery();
				conn.Close();
			}
		}

		public void SetProductDataToSave(CMEProduct.ProductsMain prodobjTmp)
		{
			DataTable dt = typeProductTable();

			foreach (var item in prodobjTmp._embedded.products)
			{
				string prodGuid = item.productGuid.ToString();
				
				dt.Rows.Add(
					item.productGuid,
					item.productGuidInt,
					item.productName,
					item.securityType,
					item.clearingSymbol,
					item.masterSymbol,
					item.exchangeClearing,
					item.exchangeGlobex,
					item.assetClass,
					item.assetSubClass,
					item.sector,
					item.subSector,
					item.dailyFlag,
					item.settlePxCcy,
					item.minGlobexOrdQty,
					item.maxGlobexOrdQty,
					item.tradePxCcy,
					item.globexGroupCode,
					item.itcCode,
					item.pxQuoteMethod, //codeMethod in Table
					item.lastUpdated
					);

				CME_API cmeAPI = new CME_API();
				string cmeLink = cmeAPI.CreateLink(1);  // Get instrument link
				string prodInstrLnk = cmeLink + prodGuid + "/instruments";
				int pageSizeInstr = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CMEAPI_PageSize"));
				string paramInstrLnk = "?page=0&size="+pageSizeInstr.ToString();
				var jsonProdInst = new WebClient().DownloadString(prodInstrLnk + paramInstrLnk);

				CMEInstrument.InstrumentMain instrobjTmp = JsonConvert.DeserializeObject<CMEInstrument.InstrumentMain>(jsonProdInst);
				
				DataTable dtInst = typeInstrumentTable();

				Console.Write("Collecting product instrument for " + prodGuid);
				for (int iCnt = 0; iCnt < instrobjTmp._metadata.totalPages; iCnt++)
				{
					Console.WriteLine("Collecting Instrument information from source." + instrobjTmp._metadata.totalElements.ToString() +" - " +iCnt.ToString() + " of " + instrobjTmp._metadata.totalPages.ToString());
					if (iCnt>0)
					{
						paramInstrLnk = "?page="+iCnt.ToString()+"&size=" + pageSizeInstr.ToString();
						jsonProdInst = new WebClient().DownloadString(prodInstrLnk + paramInstrLnk);

						instrobjTmp = JsonConvert.DeserializeObject<CMEInstrument.InstrumentMain>(jsonProdInst);
					}					
					if (instrobjTmp._embedded != null)
					{
						foreach (var inst in instrobjTmp._embedded.instruments)
						{
							dtInst.Rows.Add(
								inst.guid,
								inst.guidInt,
								inst.productGuidInt,
								inst.instrumentName,
								inst.globexSecurityId,
								inst.globexSymbol,
								inst.positionRemovalDate,
								inst.lastUpdated,
								inst.lastTradeDate,
								inst.lastNoticeDate,
								inst.lastDeliveryDate,
								inst.globexLastTradeDate,
								inst.globexFirstTradeDate,
								inst.firstTradeDate,
								inst.firstNoticeDate,
								inst.firstDeliveryDate,
								inst.finalSettlementDate,
								inst.initialInventoryDueDate,
								inst.lastInventoryDueDate,
								inst.contractMonth,
								inst.valuationMethod,
								inst.strikePx,
								inst.couponRate,
								inst.tccAlias,
								inst.itcAlias, // iccalias in DataTable name
								inst.gbxAlias,
								inst.clrAlias,
								inst.firstPositionDate,
								inst.exchangeClearing
								);
						}
						DtExecNonQuerySP(dtInst, "USP_CME_InstrumentInsert", "@tblInstrument");
						Console.WriteLine("Instrument " + prodGuid + " saved....");
					}

				}

				Console.WriteLine("Product " + prodGuid );
			}

			DtExecNonQuerySP(dt, "USP_CME_ProductInsert", "@tblProducts");
		}

		public DataTable typeProductTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.AddRange(new DataColumn[21] { 
					new DataColumn("ProductGuid", typeof(string)),
					new DataColumn("ProductGuidInt", typeof(long)),
					new DataColumn("ProductName",typeof(string)),
					new DataColumn("SecurityType",typeof(string)),
					new DataColumn("ClearingSymbol",typeof(string)),
					new DataColumn("MasterSymbol",typeof(string)),
					new DataColumn("ExchangeClearing",typeof(string)),
					new DataColumn("ExchangeGlobex",typeof(string)),
					new DataColumn("AssetClass",typeof(string)),
					new DataColumn("AssetSubClass",typeof(string)),
					new DataColumn("Sector",typeof(string)),
					new DataColumn("SubSector",typeof(string)),
					new DataColumn("DailyFlag",typeof(string)),
					new DataColumn("SettlePxCcy",typeof(string)),
					new DataColumn("MinGlobexOrdQty",typeof(string)),
					new DataColumn("MaxGlobexOrdQty",typeof(string)),
					new DataColumn("TradePxCcy",typeof(string)),
					new DataColumn("GlobexGrpCode",typeof(string)),
					new DataColumn("ITCCode",typeof(string)),
					new DataColumn("PxCodeMethod",typeof(string)),
					new DataColumn("LastUpdated",typeof(DateTime))
				});

			return dt;
		}

		public DataTable typeInstrumentTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.AddRange(new DataColumn[29] {
					new DataColumn("Guid", typeof(string)),
					new DataColumn("GuidInt", typeof(long)),
					new DataColumn("ProductGuidInt", typeof(long)),
					new DataColumn("InstrumentName",typeof(string)),
					new DataColumn("GlobexSecurityId",typeof(string)),
					new DataColumn("GlobexSymbol",typeof(string)),
					new DataColumn("PostitionRemovalDate",typeof(DateTime)),
					new DataColumn("LastUpdated",typeof(DateTime)),
					new DataColumn("LastTradeDate",typeof(DateTime)),
					new DataColumn("LastNoticeDate",typeof(DateTime)),
					new DataColumn("LastDeliveryDate",typeof(DateTime)),
					new DataColumn("GlobexLastTradeDate",typeof(DateTime)),
					new DataColumn("GlobexFirstTradeDate",typeof(DateTime)),
					new DataColumn("FirstTradeDate",typeof(DateTime)),
					new DataColumn("FirstNoticeDate",typeof(DateTime)),
					new DataColumn("FirstDeliveryDate",typeof(DateTime)),
					new DataColumn("FinalSettlementDate",typeof(DateTime)),
					new DataColumn("InitialInventoryDueDate",typeof(DateTime)),
					new DataColumn("LastInventoryDueDate",typeof(string)),
					new DataColumn("ContractMonth",typeof(string)),
					new DataColumn("ValuationMethod",typeof(string)),
					new DataColumn("StrikePrice",typeof(string)),
					new DataColumn("CouponRate",typeof(string)),
					new DataColumn("TccAlias",typeof(string)),
					new DataColumn("IccAlias",typeof(string)),
					new DataColumn("GbxAlias",typeof(string)),
					new DataColumn("ClrAlias",typeof(string)),
					new DataColumn("FirstPositionDate",typeof(string)),
					new DataColumn("ExchangeClearing",typeof(string))
				});

			return dt;
		}
	}
}
