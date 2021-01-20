using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CSVFileReader.Controller
{
   public  class TransformData
    {
		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["DRConn"].ConnectionString;

		public void GetBankTrans()
		{
			//[USP_PCI_BankTrans_Get]
			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_PCI_Trans_BMOData", conn);

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

			UpdateBOIDData(tbl);

			//tbl = UpdateCountryCodeData(tbl);

			//insertData(tbl);




		}

		

		public void UpdateBOIDData(DataTable tbl)
		{
			//Console.WriteLine(string.Format(" Im inside UpdateCountryCode "));
			//SqlDataReader rdr = cmd.ExecuteReader();
			for (int i = 0; i < tbl.Rows.Count; i++)
			{
				//Console.WriteLine("Loop "+i);

				string countryCode = string.Empty;
				string transDesc = tbl.Rows[i]["TransDesc"].ToString().ToUpper();
				bool toSearch = false;

				if (tbl.Rows[i]["RefNo"].ToString().ToUpper() == "071000288")
				{
					string mel = string.Empty;
				}
				//tbl.Rows[i]["CountryCode"] = "US";

				if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
				{
					string strBOID = tbl.Rows[i]["BO_Id"].ToString();

					if (strBOID.Contains("|"))
					{
						string[] tryw = strBOID.Split("|", StringSplitOptions.RemoveEmptyEntries);


						//Console.WriteLine("strBOID: " + strBOID + " ~" + tryw.Count()  );

						for (int k = 0; k < tryw.Count(); k++)
						{
							
							if (k == 0)
								tbl.Rows[i]["BO_Id"] = tryw[k].ToString();
							else if (k == 1)
								tbl.Rows[i]["BO_NameAdd"] = tryw[k].ToString();
							else if (k == 2)
								tbl.Rows[i]["BO_NameAdd1"] = tryw[k].ToString();
							else if (k == 3)
								tbl.Rows[i]["BO_NameAdd2"] = tryw[k].ToString();
							else if (k == 4)
								tbl.Rows[i]["BO_NameAdd3"] = tryw[k].ToString();
						}
					}



				//	countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
				}

					
				//if (countryCode != "")
				//{
					
				//}
				//else
				//{
				//	tbl.Rows[i]["CountryCode"] = countryCode;
				//	Console.Write(string.Format(" - {0} identified", countryCode));
				//}


			}

			//conn = new SqlConnection(connString);
			//conn.Open();
			//SqlCommand cmd = new SqlCommand("USP_PCI_BankTrans_Update", conn);

			//cmd.CommandType = CommandType.StoredProcedure;
			//cmd.Parameters.AddWithValue("@tblBankTransUpdate", tbl);
			//cmd.ExecuteNonQuery();
			//conn.Close();
			for (int i = 0; i < tbl.Rows.Count; i++)
			{

			string test = 	tbl.Rows[i]["BO_Id"].ToString() ;
			}

				UpdateCountryCodeData(tbl);

		}


		public void UpdateCountryCodeData(DataTable tbl)
		{
			for (int i = 0; i < tbl.Rows.Count; i++)
			{
				if (tbl.Rows[i]["BO_Id"].ToString() != null)
				{

					string test = tbl.Rows[i]["BO_Id"].ToString();
				}
			}



			for (int i = 0; i < tbl.Rows.Count; i++)
			{
				//Console.WriteLine("Loop "+i);

				string countryCode = string.Empty;
				string transDesc = tbl.Rows[i]["BO_Id"].ToString().ToUpper();
				bool toSearch = false;
				string swiftcode = string.Empty;
				string bnkcode = string.Empty;
		


				if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
				{
					string strBO = string.Empty;

					strBO = tbl.Rows[i]["BO_Id"].ToString();

					
					if ((strBO.Substring(0, 3)) == "BIC")
					{
						
						swiftcode = strBO.Substring(4, 6);
						bnkcode = swiftcode.Substring(0, 4);
						countryCode = swiftcode.Substring(4, 2);
						Console.WriteLine("strBO: " + strBO + " swiftcode: "+ swiftcode + " bnkcode: " + bnkcode + " countrycode: " + countryCode);

						if (countryCode != null)
						{
							tbl.Rows[i]["CountryCode"] = countryCode;
						}
					}

					
					
				}

			}



			insertData(tbl);
		}
	
	public void insertData(DataTable tbl)
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
						cmdInsert.Parameters.AddWithValue("@id", tbl.Rows[i]["id"].ToString());
						cmdInsert.Parameters.AddWithValue("@SourceFileName", tbl.Rows[i]["SourceFile"].ToString());
						cmdInsert.Parameters.AddWithValue("@ValueDate", tbl.Rows[i]["ValueDate"].ToString());
						cmdInsert.Parameters.AddWithValue("@EntryDate", tbl.Rows[i]["EntryDate"].ToString());
						cmdInsert.Parameters.AddWithValue("@PostedTime", tbl.Rows[i]["PostedTime"].ToString());
						cmdInsert.Parameters.AddWithValue("@BankReference", tbl.Rows[i]["BankReference"].ToString());
						cmdInsert.Parameters.AddWithValue("@BankSource", tbl.Rows[i]["BankSource"].ToString());
						cmdInsert.Parameters.AddWithValue("@CustomerReference", tbl.Rows[i]["CustomerReference"].ToString());
						cmdInsert.Parameters.AddWithValue("@AccountNo", tbl.Rows[i]["AccountNo"].ToString());
						cmdInsert.Parameters.AddWithValue("@TransType", tbl.Rows[i]["TransType"].ToString());
						cmdInsert.Parameters.AddWithValue("@TransDesc", tbl.Rows[i]["TransDesc"].ToString());
						cmdInsert.Parameters.AddWithValue("@PaymentDetails", tbl.Rows[i]["PaymentDetails"].ToString());
						//cmdInsert.Parameters.AddWithValue("@TransAmount", (tbl.Rows[i]["TransAmount"].ToString()));
						cmdInsert.Parameters.AddWithValue("@TransAmount", 0);
						cmdInsert.Parameters.AddWithValue("@Currency", tbl.Rows[i]["Currency"].ToString());
						cmdInsert.Parameters.AddWithValue("@SourceRef", tbl.Rows[i]["SourceRef"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_Id", tbl.Rows[i]["OB_Id"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd", tbl.Rows[i]["OB_NameAdd"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd1", tbl.Rows[i]["OB_NameAdd1"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd2", tbl.Rows[i]["OB_NameAdd2"].ToString());
						cmdInsert.Parameters.AddWithValue("@OB_NameAdd3", tbl.Rows[i]["OB_NameAdd3"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_Id", tbl.Rows[i]["BO_Id"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd", tbl.Rows[i]["BO_NameAdd"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd1", tbl.Rows[i]["BO_NameAdd1"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd2", tbl.Rows[i]["BO_NameAdd2"].ToString());
						cmdInsert.Parameters.AddWithValue("@BO_NameAdd3", tbl.Rows[i]["BO_NameAdd3"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_Id", tbl.Rows[i]["BE_Id"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd", tbl.Rows[i]["BE_NameAdd"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd1", tbl.Rows[i]["BE_NameAdd1"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd2", tbl.Rows[i]["BE_NameAdd2"].ToString());
						cmdInsert.Parameters.AddWithValue("@BE_NameAdd3", tbl.Rows[i]["BE_NameAdd3"].ToString());
						cmdInsert.Parameters.AddWithValue("@CountryCode", tbl.Rows[i]["CountryCode"].ToString());
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
