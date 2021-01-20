using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CSVFileReader.Controller
{
    public class AMLCitiTransformData
    {

		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["DRConn"].ConnectionString;

		public void GetCitiTrans()
		{
			//[USP_PCI_BankTrans_Get]
			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_PCI_GetRawTrans_Citi_Test", conn);

			cmd.CommandType = CommandType.StoredProcedure;
			conn.Open();

			SqlDataReader rdr = cmd.ExecuteReader();


			//while (rdr.Read())
			//{
			//	string cqAcct = string.Empty;

			//	string pd = rdr["SourceFile"].ToString().ToUpper();

			//	//string acctMatch = MatchAccount(pd);
			//	//acctMatch = acctMatch + " || " + rdr["CustomerReference"].ToString().ToUpper();


			//	//	Console.WriteLine(rdr["Id"].ToString().ToUpper() + " || " + rdr["BO_Id"].ToString().ToUpper() + " || " + rdr["OB_NameAdd"].ToString().ToUpper() + " || " );
			//	Console.WriteLine(rdr["SourceFile"].ToString().ToUpper() + " || " + rdr["BO_Id"].ToString().ToUpper() + " || " + rdr["OB_NameAdd"].ToString().ToUpper() + " || ");

			//}
			DataTable tbl = new DataTable();

			tbl.Load(rdr);
			conn.Close();

			insertAMLCitiData(tbl);

			//tbl = UpdateCountryCodeData(tbl);

			//insertData(tbl);




		}
		public void insertAMLCitiData(DataTable tbl)
		{

			try
			{


				//cmd.Parameters.AddWithValue("@tblBankTransUpdate", tbl);
				using (var connection = new SqlConnection(connString))
				{

					for (int i = 0; i < tbl.Rows.Count; i++)
					{
						SqlCommand cmdInsert = new SqlCommand("USP_PCI_Trans_BMOData_Insert", connection);
						cmdInsert.CommandType = CommandType.StoredProcedure;
						connection.Open();
						cmdInsert.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
						cmdInsert.Parameters.AddWithValue("@SourceFileName", tbl.Rows[i]["SourceFile"].ToString());
						cmdInsert.Parameters.AddWithValue("@ValueDate", tbl.Rows[i]["ValueDate"].ToString());
						cmdInsert.Parameters.AddWithValue("@EntryDate", tbl.Rows[i]["EntryDate"].ToString());
						cmdInsert.Parameters.AddWithValue("@PostedTime", tbl.Rows[i]["PostedTime"].ToString());
						cmdInsert.Parameters.AddWithValue("@BankReference", tbl.Rows[i]["BankRef"].ToString());
						cmdInsert.Parameters.AddWithValue("@BankSource", "");//tbl.Rows[i]["BankSource"].ToString());
						cmdInsert.Parameters.AddWithValue("@CustomerReference", tbl.Rows[i]["CustRef"].ToString());
						cmdInsert.Parameters.AddWithValue("@AccountNo","");// tbl.Rows[i]["AccountNo"].ToString());
						cmdInsert.Parameters.AddWithValue("@TransType","");// tbl.Rows[i]["TransType"].ToString());
						cmdInsert.Parameters.AddWithValue("@TransDesc", tbl.Rows[i]["TransDesc"].ToString());
						cmdInsert.Parameters.AddWithValue("@PaymentDetails", tbl.Rows[i]["PaymentDetails"].ToString());
						//cmdInsert.Parameters.AddWithValue("@TransAmount", "");//(tbl.Rows[i]["TransAmount"].ToString()));
						cmdInsert.Parameters.AddWithValue("@TransAmount", 0);
						cmdInsert.Parameters.AddWithValue("@Currency","");// tbl.Rows[i]["Currency"].ToString());
						cmdInsert.Parameters.AddWithValue("@SourceRef", "");//tbl.Rows[i]["SourceRef"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_Id", tbl.Rows[i]["OrderingID"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd", tbl.Rows[i]["OrderingAdd"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd1", tbl.Rows[i]["OrderingAdd1"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd2", tbl.Rows[i]["OrderingAdd2"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd3", tbl.Rows[i]["OrderingAdd3"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_Id", tbl.Rows[i]["ByOrderID"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd", tbl.Rows[i]["ByOrderAdd"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd1", tbl.Rows[i]["ByOrderAdd1"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd2", tbl.Rows[i]["ByOrderAdd2"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd3", tbl.Rows[i]["ByOrderAdd3"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_Id", tbl.Rows[i]["BenifitID"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd", tbl.Rows[i]["BenefitAdd"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd1", tbl.Rows[i]["BenefitAdd1"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd2", tbl.Rows[i]["BenefitAdd2"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd3", tbl.Rows[i]["BenefitAdd3"].ToString());
						cmdInsert.Parameters.AddWithValue("@CountryCode","");// tbl.Rows[i]["CountryCode"].ToString());
						cmdInsert.ExecuteNonQuery();
						connection.Close();
					}
				}

				Console.WriteLine("Done Insert");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}






		}


	}
}
