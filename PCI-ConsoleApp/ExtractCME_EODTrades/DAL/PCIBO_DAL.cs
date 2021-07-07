using PCI.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ExtractCME_EODTrades.DAL
{
	public class PCIBO_DAL
	{

		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
		//readonly string CQconnString = ConfigurationManager.ConnectionStrings["CQGlobalUS"].ConnectionString;

		public void InsertCME_EODFIXML(LogHelper log, string tradeDate)
		{
			//DataTable dt = new DataTable();
			log.Log("Inserting CME FIXML Data to PCI DB...  Please wait");

			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_CME_EODFIXMLTrade_Insert", conn);
			cmd.Parameters.AddWithValue("@dateFile", tradeDate); //"20210604"

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = 3000;
			conn.Open();

			SqlDataReader rdr = cmd.ExecuteReader();
			//dt.Load(rdr);

			conn.Close();

			//return dt;
		}

		public DataTable GetCME_EODTrades(LogHelper log, string tradeDate)
		{
			log.Log("Exctracting CME Data for Volume comparison ...  Please wait");

			DataTable dt = new DataTable();

			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_CME_EODFIXMLTrades_Create", conn);
			cmd.Parameters.AddWithValue("@dateFile", tradeDate); //"20210604"

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = 3000;
			conn.Open();

			SqlDataReader rdr = cmd.ExecuteReader();
			dt.Load(rdr);

			conn.Close();

			return dt;
		}

		public void UpdateCQVol_DB(LogHelper log, DataTable tbl)
		{
			//DataTable dt = new DataTable();
			log.Log("Updating CQ Volume DB...  Please wait");

			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_CME_UpdateCQVol", conn);
			cmd.Parameters.AddWithValue("@tblCQVolume", tbl); //"20210604"

			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandTimeout = 3000;
			conn.Open();

			SqlDataReader rdr = cmd.ExecuteReader();
			//dt.Load(rdr);

			conn.Close();

			//return dt;
		}
	}
}
