using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static PCI_BankTransApp.Models.CQBOAPIModel;

namespace PCI_BankTransApp.DAL
{
	public class PCIBO_DAO
	{
		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
		
		public void GetBankTrans(CQAPICountryList countryRes)
		{
			//[USP_PCI_BankTrans_Get]
			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_PCI_BankTrans_Get", conn);

			cmd.CommandType = CommandType.StoredProcedure;
			conn.Open();

			SqlDataReader rdr = cmd.ExecuteReader();
			DataTable tbl = new DataTable();

			tbl.Load(rdr);

			//while (rdr.Read())
			//{
			//	string cqAcct = string.Empty;

			//	string pd = rdr["PaymentDetails"].ToString().ToUpper();

			//	string acctMatch = MatchAccount(pd);
			//	acctMatch = acctMatch + " || " + rdr["CustomerReference"].ToString().ToUpper();


			//	Console.WriteLine(rdr["Id"].ToString().ToUpper() + " || " + rdr["BO_Id"].ToString().ToUpper() + " || " + rdr["OB_NameAdd"].ToString().ToUpper() + " || " + acctMatch);

			//}
			conn.Close();

			UpdateCountryCode(tbl, countryRes);
		}

		public void UpdateCountryCode(DataTable tbl, CQAPICountryList countryRes)
		{

			//SqlDataReader rdr = cmd.ExecuteReader();
			for (int i = 0; i < tbl.Rows.Count; i++)
			{

				Console.WriteLine(string.Format(" Identifying country code for {0}", tbl.Rows[i]["Id"]));
				string countryCode = string.Empty;
				string transDesc = tbl.Rows[i]["TransDesc"].ToString().ToUpper();
				bool toSearch = false;

				if (tbl.Rows[i]["Id"].ToString().ToUpper() == "A3F83D7B-F7D0-4A83-B854-038DFCE5C650")
				{
					string mel = string.Empty;
				}
				//tbl.Rows[i]["CountryCode"] = "US";
				switch (transDesc)
				{
					case "ACH CREDIT":
						countryCode = "US";
						break;
					case "ACH DEBIT":
						countryCode = "US";
						break;
					case "ISSUE ? DRAFT":
						if (tbl.Rows[i]["BO_Id"].ToString().Length>1)
						{
							countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
						}
						break;
					case "INTERNAL TRANSFER":
						if (tbl.Rows[i]["BE_Id"].ToString().Length>1)
						{
							countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
						}
						break;
					case "CHAPS PAYMENT RECD":
						if (tbl.Rows[i]["BE_Id"].ToString().Length>1)
						{
							countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
						}
						break;
					case "CCY RECD":
						if (tbl.Rows[i]["BO_Id"].ToString().Length>1)
						{
							countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
						}
						
						break;
					case "SAME DAY CR TRANSFER":
						if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
						{
							countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
						}
						else if (tbl.Rows[i]["OB_Id"].ToString().Length > 1)
						{
							countryCode = tbl.Rows[i]["OB_Id"].ToString().Substring(0, 2);
						}
						else if (tbl.Rows[i]["BE_Id"].ToString().Length > 1)
						{
							countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
						}
						break;
					case "SAME DAY DR TRANSFER":
						if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
						{
							countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
						} else if (tbl.Rows[i]["OB_Id"].ToString().Length > 1)
						{
							countryCode = tbl.Rows[i]["OB_Id"].ToString().Substring(0, 2);
						} else if (tbl.Rows[i]["BE_Id"].ToString().Length > 1)
							{
								countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
							}
						break;
					case "WIRE PYMT XB EU/EEA AUTO":
						if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
						{
							countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
						}
						else
						{
							if (tbl.Rows[i]["OB_Id"].ToString() == "" || tbl.Rows[i]["BO_Id"].ToString() == "")
							{
								countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
							}
						}
						break;
					default:
						break;

				}
				if (countryCode != "")
				{
					bool validCode = ValidCountryCode(countryRes, countryCode);

					if (validCode)
					{
						tbl.Rows[i]["CountryCode"] = countryCode;
						tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
						Console.Write(string.Format(" - {0} identified", countryCode));
					}
					else // look for Country from Bank Order Address
					{
						string fieldVal = string.Empty;
						string retCountryCode = string.Empty;
						for (int r = 3; r >= 0; r--)
						{
							string fieldCnt = r == 0 ? "" : r.ToString();
							fieldVal = tbl.Rows[i]["BO_NameAdd" + fieldCnt].ToString();
							if (fieldVal != "")
							{
								if (fieldVal.Length==11 )  // it is a swift code
								{
									if (fieldVal.Substring(8, 3) == "XXX")
									{
										fieldVal = fieldVal.Substring(4, 2);
									}
								}

								retCountryCode = DoSearch(countryRes, fieldVal);
								tbl.Rows[i]["CountryCode"] = retCountryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
								Console.Write(string.Format(" - {0} identified", retCountryCode));
								if (retCountryCode!="")
								{
									r = 0;
								}
							}
						}

						validCode = ValidCountryCode(countryRes, retCountryCode);
						if (validCode)
						{
							tbl.Rows[i]["CountryCode"] = retCountryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
							Console.Write(string.Format(" - {0} identified", retCountryCode));
						}
						else// look for Country from Order By Details Address
						{ 
							for (int r1 = 3; r1 >= 0; r1--)
							{
								string fieldCnt = r1 == 0 ? "" : r1.ToString();
								fieldVal = tbl.Rows[i]["OB_NameAdd" + fieldCnt].ToString();
								if (fieldVal != "")
								{
									retCountryCode = DoSearch(countryRes, fieldVal);
									tbl.Rows[i]["CountryCode"] = retCountryCode;
									tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
									Console.Write(string.Format(" - {0} identified", retCountryCode));
									if (retCountryCode != "")
									{
										r1 = 0;
									}
								}
							}
						}


						validCode = ValidCountryCode(countryRes, retCountryCode);
						if (validCode)
						{
							tbl.Rows[i]["CountryCode"] = retCountryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
							Console.Write(string.Format(" - {0} identified", retCountryCode));
						}
						else // look for Country from Deneficiary Details Address
						{
							for (int r2 = 3; r2 >= 0; r2--)
							{
								string fieldCnt = r2 == 0 ? "" : r2.ToString();
								fieldVal = tbl.Rows[i]["BE_NameAdd" + fieldCnt].ToString();
								if (fieldVal != "")
								{
									retCountryCode = DoSearch(countryRes, fieldVal);
									tbl.Rows[i]["CountryCode"] = retCountryCode;
									tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
									Console.Write(string.Format(" - {0} identified", retCountryCode));
									if (retCountryCode != "")
									{
										r2 = 0;
									}
								}
							}
						}

					}
				}
				else
				{
					tbl.Rows[i]["CountryCode"] = countryCode;
					tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
					Console.Write(string.Format(" - {0} identified", countryCode));
				}
				
				
			}

			conn = new SqlConnection(connString);
			conn.Open();
			SqlCommand cmd = new SqlCommand("USP_PCI_BankTrans_Update", conn);
			//cmd.Parameters.Add(
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@tblBankTransUpdate",tbl);
			cmd.ExecuteNonQuery();
			conn.Close();

			
		}

		public string MatchAccount(string input)
		{
			string result = string.Empty;
			MatchCollection match = Regex.Matches(input, @"[789][a-zA-Z0-9]{6}|[789][a-zA-Z]{2}\s*\d{4}|[789][a-zA-Z]{3}\s*\d{3}");
			List<string> matches = match.Select(a => a.Value.Replace(" ", "")).Distinct().Where(a => !Regex.Match(a, @"[7][a-zA-z]{2}[Rr]{1}[A-Za-z0-9]{3}").Success).ToList();
			if (matches.Count>0)
			{
				foreach (var item in matches)
				{
					if (item.Substring(0,1).ToString()=="7")
					{
						result = item;
					}
					
				}
			}
			return result;
		}

		public bool ValidCountryCode(CQAPICountryList countryRes, string countryCode)
		{
			bool retVal = false;

			var cCode = countryRes.data.Where(x => x.cd_Ref == countryCode).FirstOrDefault();
			if (cCode != null)
			{
				if (cCode.cd_Ref!="")
				{
					retVal = true;
				}
			}
			return retVal;
		}

		public string GetCountryName(CQAPICountryList countryRes, string countryCode)
		{
			string retVal = string.Empty;

			var cCode = countryRes.data.Where(x => x.cd_Ref == countryCode).FirstOrDefault();
			if (cCode != null)
			{
				if (cCode.cd_Ref != "")
				{
					retVal = cCode.description.ToString().ToUpper();
				}
			}
			return retVal;
		}

		public string GetCountryCodeByName(CQAPICountryList countryRes, string countryName)
		{
			string retVal = string.Empty;

			var cCode = countryRes.data.Where(x => x.description.ToUpper() == countryName.ToUpper()).FirstOrDefault();
			if (cCode!=null)
			{
				if (cCode.cd_Ref != "")
				{
					retVal = cCode.cd_Ref;
				}
			}

			return retVal;
		}
		private string DoSearch(CQAPICountryList countryRes, string fieldVal)
		{
			string retVal = string.Empty;

			// Search for Address Field in Bank Order fields Address, 1 ,2 and 3
			bool validCode = false;
			string[] arrVal = fieldVal.Split(" ");
			foreach (var val in arrVal)
			{
				string newVal = val.ToString() == "USA" ? "US" : val.ToString();
				validCode = ValidCountryCode(countryRes, newVal);
				if (validCode)
				{
					retVal = newVal.ToString();
				}
				else
				{
					retVal = GetCountryCodeByName(countryRes, newVal);
					if (retVal != "")
					{
						return retVal;
					}
				}
			}
			return retVal;
		}

		public void test()
		{
			//toSearch = DoSearch(countryCode);
			//if (toSearch)
			//{
			//	string cc = countryCode.Substring(0, 2);
			//	if (CheckCountryCode(countryRes, cc))
			//	{
			//		countryCode = cc;
			//	}
			//	else
			//	{
			//		countryCode = "";

			//	}
			//}
		}
	}
}
