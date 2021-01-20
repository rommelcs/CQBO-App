using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static AMLApp.Models.CQBOAPIModels;

namespace AMLApp.DAL
{
	public class PCIBO_DAL
	{
		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;

		public void GetBankTrans(CQAPICountryList countryRes, CQAPIRegionList regionRes)
		{
			//[USP_PCI_BankTrans_Get]
			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_PCI_BankTrans_Get", conn);

			cmd.CommandType = CommandType.StoredProcedure;
			conn.Open();

			SqlDataReader rdr = cmd.ExecuteReader();
			DataTable tbl = new DataTable();

			tbl.Load(rdr);
			conn.Close();

			tbl = IdentifyCQAccount(tbl);
			IdentifyCountryCode(tbl, countryRes, regionRes);
			//UpdateCountryCode(tbl, countryRes);
		}

		private DataTable IdentifyCQAccount(DataTable tbl)
		{
			for (int i = 0; i < tbl.Rows.Count; i++)
			{
				string CQAcct = string.Empty;
				string idVal = tbl.Rows[i]["Id"].ToString();
				Console.Write(string.Format("\r Processing item {0} of {1}", i + 1, tbl.Rows.Count));

				foreach (DataColumn item in tbl.Columns)
				{
					
					string fldVal = tbl.Rows[i][item.ColumnName].ToString();
					switch (item.ColumnName)
					{
						case "CustomerReference":
							CQAcct = FindCQAccount(fldVal);
							break;
						case "PaymentDetails":
							CQAcct = FindCQAccount(fldVal);
							break;
						case "BE_Id":
							CQAcct = FindCQAccount(fldVal);
							break;
						default:
							break;
					}
					if (CQAcct != "")
					{
						tbl.Rows[i]["AccountNo"] = CQAcct;
						Console.Write(string.Format("\n columnname {0} {1}", item.ColumnName, CQAcct));
					}
					
				}
				Console.WriteLine(string.Format("\n Processing ID {0} - Found CQ Account as {1}", idVal, CQAcct));
			}

			return tbl;
		}

		private string FindCQAccount(string strFldVal)
		{
			string retVal = string.Empty;
			//string pattern = @"^([7-8]){1}([a-zA-Z0-9]){6}?$";

			MatchCollection match = Regex.Matches(strFldVal, @"[789][a-zA-Z0-9]{6}|[789][a-zA-Z]{2}\s*\d{4}|[789][a-zA-Z]{3}\s*\d{3}");
			//List<string> matches = match.Select(a => a.Value.Replace(" ", "")).Distinct().Where(a => !Regex.Match(a, @"([7-8]){1}[a-zA-z]{2}[Rr]{1}[A-Za-z0-9]{3}").Success).ToList();
			List<string> matches = match.Select(a => a.Value.Replace(" ", "")).Distinct().Where(a => !Regex.Match(a, @"[7][a-zA-Z]{2}[Rr]{1}[A-Za-z0-9]{3}").Success).ToList();
			if (matches.Count > 0)
			{
				foreach (var item in matches)
				{
					string pattern = @"^([7-9]){1}([a-zA-Z]){1}([a-zA-Z0-9]){2}([0-9]){3}?$";
					if (Regex.IsMatch(item, pattern))
					{
						retVal = item;
					}
				}
			}

			return retVal;
		}

		public void IdentifyCountryCode(DataTable tbl, CQAPICountryList countryRes, CQAPIRegionList regionRes)
		{
			for (int i = 0; i < tbl.Rows.Count; i++)
			{
				Console.Write(string.Format("\r Processing item {0} of {1}", i+1, tbl.Rows.Count));
				string strId = "489B1546-BD17-4E6A-8EDC-69A876548333";
				if (tbl.Rows[i]["Id"].ToString().ToUpper() == strId.ToUpper())
				{
					string mel = "";
				}
				
				
				for (int c = 0; c < tbl.Columns.Count; c++)
				{
					string strVal = tbl.Rows[i][c].ToString();

					if (strVal.Length > 4)
					{
						if (strVal.Substring(0, 4) == "BIC-" || strVal.Substring(0, 4) == "ABA-") // BMO Format to identify Swift code or ABA code
						{
							string countryCode = string.Empty;
							if (strVal.Substring(0, 4) == "BIC-")
							{
								countryCode = strVal.Substring(8, 2); // Get Value from Swift Code
							}
							else
							{
								countryCode = "US";  //US Bank account via ABA code
							}

							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through " + strVal.Substring(0, 3) + " code format {0}", strVal);
							Console.Write(string.Format("\n Processing item {0} of {1} - Value {2}", i + 1, tbl.Rows.Count, strVal));
							if (tbl.Rows[i]["CountryName"].ToString() != "")
							{
								break;
							}
						}
					}

					if (HasSwiftCode(strVal))
					{
						string countryCode = strVal.Substring(4, 2);
						tbl.Rows[i]["CountryCode"] = countryCode;
						tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
						tbl.Rows[i]["Remarks"] = string.Format("Identified through Swift Code {0}", strVal);
						Console.Write(string.Format("\n Processing item {0} of {1} - Value {2}", i + 1, tbl.Rows.Count, strVal));
						if (tbl.Rows[i]["CountryName"].ToString() != "")
						{
							break;
						}

					}
					if (strVal.Length>=23)
					{
						if (strVal.Substring(0, 10) == "ORDER BANK")
						{
							string strToMatch = strVal.Substring(12, 11);
							if (HasSwiftCode(strToMatch))
							{
								string countryCode = strToMatch.Substring(4, 2);
								tbl.Rows[i]["CountryCode"] = countryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
								tbl.Rows[i]["Remarks"] = string.Format("Identified from Order Bank Swift Code {0}", strToMatch);
								Console.Write(string.Format("\n Processing item {0} of {1} - Value {2}", i + 1, tbl.Rows.Count, strToMatch));
								if (tbl.Rows[i]["CountryName"].ToString() != "")
								{
									break;
								}
							}
						}
					}
					
					if (HasOtherIntlTransferCode(strVal))
					{
						string countryCode = strVal.Substring(0, 2);
						tbl.Rows[i]["CountryCode"] = countryCode;
						tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
						tbl.Rows[i]["Remarks"] = string.Format("Identified through IBAN code format {0}", strVal);
						Console.Write(string.Format("\n Processing item {0} of {1} - Value {2}", i + 1, tbl.Rows.Count, strVal));
						if (tbl.Rows[i]["CountryName"].ToString()!="")
						{
							break;
						}
					}

					if (IsACH(strVal))
					{
						string countryCode = "US";
						tbl.Rows[i]["CountryCode"] = countryCode;
						tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
						tbl.Rows[i]["Remarks"] = string.Format("Identified ACH Transfer {0}", strVal);
						Console.Write(string.Format("\n Processing item {0} of {1} - Value {2}", i + 1, tbl.Rows.Count, strVal));
						if (tbl.Rows[i]["CountryName"].ToString() != "")
						{
							break;
						}
					}

					if (strVal.Length >= 17) // SPECIAL 
					{
						if (strVal.Substring(0, 17) == "ADVANTAGE FUTURES")
						{
							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through Payemnt Details - {0}", strVal.Substring(0, 17));
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
						}
					}
				}

				string fldBEVal = tbl.Rows[i]["BE_Id"].ToString();
				if (fldBEVal == "30865347" || fldBEVal == "30855974" || fldBEVal == "31253713") // Beneficiary is PCI Account 30865347, 30855974
				{
					if (tbl.Rows[i]["CountryName"].ToString() == "") // no value from other validation above
					{
						string fldOBIdVal = tbl.Rows[i]["OB_Id"].ToString();
						string fldBOIdVal = tbl.Rows[i]["BO_Id"].ToString();
						string fldBBIdVal = tbl.Rows[i]["BB_Id"].ToString();
						string fldSourceRefVal = tbl.Rows[i]["SourceRef"].ToString();
						string fldPaymentDetailVal = tbl.Rows[i]["PaymentDetails"].ToString();
						if (fldPaymentDetailVal.Length >=17)
						{
							if (fldPaymentDetailVal.Substring(0, 17) == "INTERNAL TRANSFER")
							{
								string countryCode = "US";
								tbl.Rows[i]["CountryCode"] = countryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
								tbl.Rows[i]["Remarks"] = string.Format("Identified through Payemnt Details - {0}", fldPaymentDetailVal.Substring(0, 17));
								Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
							}
						}
						
						if (IsUSBankFormat(fldOBIdVal) && IsUSBankFormat(fldBOIdVal) && tbl.Rows[i]["CountryName"].ToString() == "")
						{

							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank Account format");
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
							//break;
						}

						if (fldBOIdVal == fldOBIdVal && tbl.Rows[i]["CountryName"].ToString() == "")
						{
							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank Account format. Ordering Bank and Order By is same account.");
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
							//break;
						}

						if ((fldSourceRefVal.Length ==16 || fldSourceRefVal.Length == 15) && fldBOIdVal.Length == 10 && fldOBIdVal.Length==9 && tbl.Rows[i]["CountryName"].ToString() == "")
						{
							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							if (fldSourceRefVal.Length==15)
							{
								tbl.Rows[i]["Remarks"] = string.Format("Identified through USAA federal Savings Bank Source Ref and Order By and Order Bank field format with PCI account as beneficiary");
							}
							else
							{
								tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank Account format from SoruceRef, Order By and Order Bank field with PCI account as beneficiary");
							}
							
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
						}

						if (fldSourceRefVal.Length == 16 && fldOBIdVal.Length == 9 && tbl.Rows[i]["CountryName"].ToString() == "")
						{
							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank Account format from SourceRef and Order By with PCI account as beneficiary");
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
						}

						if (fldBOIdVal.Length == 10 && fldOBIdVal.Length == 9 && tbl.Rows[i]["CountryName"].ToString() == "")
						{
							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank Account format from Order By and Order Bank field with PCI account as beneficiary");
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
						}

						if (fldSourceRefVal.Length>=3)
						{
							if (fldSourceRefVal.Substring(0, 3) == "OPF" && tbl.Rows[i]["CountryName"].ToString() == "") // Now Check if from NAVY Federal Credit Union
							{
								string countryCode = "US";
								tbl.Rows[i]["CountryCode"] = countryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
								tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank Source Ref from Navy Federal Credit Union By Order Address");
								Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
							}
						}
						if (fldSourceRefVal.Length == 12) // JP Morgan Source Ref format
						{
							if (IsJPMorganSourceFormat(fldSourceRefVal))
							{
								if (fldSourceRefVal.Substring(10,2)=="ES")
								{
									string countryCode = "US";
									tbl.Rows[i]["CountryCode"] = countryCode;
									tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
									tbl.Rows[i]["Remarks"] = string.Format("JPMorgan US Bank Source Reference Format - {0}", fldSourceRefVal);
									Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
								}
							}
						}
					}
				}
				else  // Not PCI account as Beneficiary
				{
					if (tbl.Rows[i]["CountryName"].ToString() == "") // no value from other validation above
					{
						string fldOBIdVal = tbl.Rows[i]["OB_Id"].ToString();
						string fldBOIdVal = tbl.Rows[i]["BO_Id"].ToString();
						string fldBBIdVal = tbl.Rows[i]["BB_Id"].ToString();
						string fldSourceRefVal = tbl.Rows[i]["SourceRef"].ToString();

						if (fldBEVal.Length==9 && fldBBIdVal.Length==9)
						{
							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank account format from Benificiary and Bank beneficiary field");
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
						}
						if (fldBBIdVal.Length == 9 && fldBOIdVal=="" && fldOBIdVal=="")
						{
							string countryCode = "US";
							tbl.Rows[i]["CountryCode"] = countryCode;
							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
							tbl.Rows[i]["Remarks"] = string.Format("Identified through US Bank account format from Bank beneficiary field");
							Console.Write(string.Format("\n Processing item {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
						}
					}
				}

				// Check the addresses here

				if (tbl.Rows[i]["CountryName"].ToString() == "") // no value from other validation above
				{
					Console.Write(string.Format("\n identifying address for row {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
					for (int BO = 3; BO >= 0; BO--)
					{
						string fieldCnt = BO == 0 ? "" : BO.ToString();
						string fieldVal = tbl.Rows[i]["BO_NameAdd" + fieldCnt].ToString();
						if (fieldVal != "")
						{
							string retCountryCode = DoSearch(countryRes, fieldVal, regionRes);
							if (retCountryCode != "")
							{
								tbl.Rows[i]["CountryCode"] = retCountryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
								if (tbl.Rows[i]["CountryName"].ToString() != "")
								{
									tbl.Rows[i]["Remarks"] = string.Format("Identified through By Order Address - {0}", fieldVal);
									Console.Write(string.Format(" - {0} identified", retCountryCode));
								}
							}

							if (retCountryCode != "")
							{
								BO = 0;
							}
						}
					}
				}

				if (tbl.Rows[i]["CountryName"].ToString() == "") // no value from other validation above
				{
					Console.Write(string.Format("\n identifying address for row {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
					for (int OB = 3; OB >= 0; OB--)
					{
						string fieldCnt = OB == 0 ? "" : OB.ToString();
						string fieldVal = tbl.Rows[i]["OB_NameAdd" + fieldCnt].ToString();
						if (fieldVal != "")
						{
							string retCountryCode = DoSearch(countryRes, fieldVal, regionRes);
							if (retCountryCode != "")
							{
								tbl.Rows[i]["CountryCode"] = retCountryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
								if (tbl.Rows[i]["CountryName"].ToString() != "")
								{
									tbl.Rows[i]["Remarks"] = string.Format("Identified through Order Bank Address - {0}", fieldVal);
									Console.Write(string.Format(" - {0} identified", retCountryCode));
								}
							}

							if (retCountryCode != "")
							{
								OB = 0;
							}
						}
					}

				}

				if (tbl.Rows[i]["CountryName"].ToString() == "") // no value from other validation above
				{
					Console.Write(string.Format("\n identifying address for row {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
					for (int BB = 3; BB >= 0; BB--)
					{
						string fieldCnt = BB == 0 ? "" : BB.ToString();
						string fieldVal = tbl.Rows[i]["BB_NameAdd" + fieldCnt].ToString();
						if (fieldVal != "")
						{
							if (fieldVal.Length>=18)
							{
								if (fieldVal.Substring(0, 18) == "POST ALL DEBITS TO")
								{
									break;
								}
							}
							
							string retCountryCode = DoSearch(countryRes, fieldVal, regionRes);
							if (retCountryCode != "")
							{
								tbl.Rows[i]["CountryCode"] = retCountryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
								if (tbl.Rows[i]["CountryName"].ToString() != "")
								{
									tbl.Rows[i]["Remarks"] = string.Format("Identified through Benificiary Bank Address - {0}", fieldVal);
									Console.Write(string.Format(" - {0} identified", retCountryCode));
								}
							}

							if (retCountryCode != "")
							{
								BB = 0;
							}
						}
					}

				}

				if (tbl.Rows[i]["CountryName"].ToString() == "") // no value from other validation above
				{
					Console.Write(string.Format("\n identifying address for row {0} of {1} - ID {2}", i + 1, tbl.Rows.Count, tbl.Rows[i]["Id"].ToString()));
					for (int BE = 3; BE >= 0; BE--)
					{
						string fieldCnt = BE == 0 ? "" : BE.ToString();
						string fieldVal = tbl.Rows[i]["BE_NameAdd" + fieldCnt].ToString();
						if (fieldVal != "")
						{
							string retCountryCode = DoSearch(countryRes, fieldVal, regionRes);
							if (retCountryCode != "")
							{
								tbl.Rows[i]["CountryCode"] = retCountryCode;
								tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
								if (tbl.Rows[i]["CountryName"].ToString() != "")
								{
									tbl.Rows[i]["Remarks"] = string.Format("Identified through Benificiary Address - {0}", fieldVal);
									Console.Write(string.Format(" - {0} identified", retCountryCode));
								}
							}

							if (retCountryCode != "")
							{
								BE = 0;
							}
						}
					}

				}
			}

			conn = new SqlConnection(connString);
			conn.Open();
			SqlCommand cmd = new SqlCommand("USP_PCI_BankTrans_Update", conn);
			//cmd.Parameters.Add(
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@tblBankTransUpdate", tbl);
			cmd.ExecuteNonQuery();
			conn.Close();
		}

		private bool IsJPMorganSourceFormat(string strVal)
		{
			bool retVal = false;

			if (strVal.Length == 12)  // US Bank Code
			{
				string pattern = @"^([0-9]){10}([a-zA-Z]){2}?$";
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}

			return retVal;
		}

		private bool IsUSBankFormat(string strVal)
		{
			bool retVal = false;

			if (strVal.Length==4)  // US Bank Code
			{
				string pattern = @"^([0-9]){4}?$";
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}

			if (strVal.Length == 8) // US Bank Account format
			{
				string pattern = @"^([0-9]){8}?$";
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}

			if (strVal.Length == 9) // ABA Code format
			{
				string pattern = @"^([0-9]){9}?$";
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}

			if (strVal.Length == 12) // US Bank Account Format
			{
				string pattern = @"^([0-9]){12}?$";
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}

			if (strVal.Length == 15) // Bank Account Format
			{
				string pattern = @"^([0-1]){5}([0-9]){10}?$";
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}

			return retVal;
		}
		private bool IsACH(string strVal)
		{
			bool retVal = false;
			if (strVal.Length>=3)
			{
				if (strVal.Substring(0, 3) == "ACH")
				{
					retVal = true;
				}
			}
			

			return retVal;
		}
		private bool HasSwiftCode(string strVal)
		{
			bool retVal = false;
			string pattern = @"^([a-zA-Z]){4}([a-zA-Z]){2}([0-9a-zA-Z]){2}([0-9a-zA-Z]{3})?$";
			if (strVal == "NINJATRADER" || strVal == "INTEREST" || strVal == "MAYFIELD" || strVal == "BENSALEM" || strVal == "DRESSLER" 
				|| strVal == "DUNSBOROUGH" || strVal == "GALLEGOS" || strVal == "CHOISEUL" || strVal == "USTRALIA" || strVal == "BUDAPEST"
				|| strVal == "GONZALEZ" || strVal == "GONVILLE" || strVal == "VITACURA" || strVal == "ALTABANK" || strVal == "TRWIBEB1"
				|| strVal == "ASUNCION" || strVal == "ALTAMONT" || strVal == "GRANADOS")
			{
				return false;
			}

			if ((strVal.Length==8 || strVal.Length == 11) && strVal != "NOTPROVIDED")
			{
				if (strVal.Length==11 ) // If full l
				{
					if (strVal.Substring(8,3)=="XXX")
					{
						retVal = true;
					}
					else
					{
						pattern = @"^([a-zA-Z]){4}([a-zA-Z]){2}([0-9a-zA-Z]){2}([a-zA-Z0-9]{3})?$";
					}
					
				}
				
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}
			return retVal;
		}

		private bool HasSwiftCodeFromBody(string strVal)
		{
			bool retVal = false;
			string pattern = @"^([a-zA-Z]){4}([a-zA-Z]){2}([0-9a-zA-Z]){2}([0-9a-zA-Z]{3})?$";

			if (Regex.IsMatch(strVal, pattern))
			{
				retVal = true;
			}
		
			return retVal;
		}

		private bool HasOtherIntlTransferCode(string strVal)
		{
			//IBAN Codes - First 2 refers to Country Code
			bool retVal = false;
			bool hasPattern = false;
			string pattern = string.Empty;

			switch (strVal.Length)
			{
				case 18:
					pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){16}?$";
					hasPattern = true;
					break;
				case 21:
					pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){19}?$";
					hasPattern = true;
					break;
				case 22:
					 pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){20}?$";
					hasPattern = true;
					break;
				case 23:
					pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){21}?$";
					hasPattern = true;
					break;
				case 24:
					 pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){22}?$";
					hasPattern = true;
					break;
				case 26:
					pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){24}?$";
					hasPattern = true;
					break;
				case 27:
					pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){25}?$";
					hasPattern = true;
					break;
				case 29:
					pattern = @"^([a-zA-Z]){2}([a-zA-Z0-9]){27}?$";
					hasPattern = true;
					break;
				default:
					break;
			}
			if (hasPattern)
			{
				if (Regex.IsMatch(strVal, pattern))
				{
					retVal = true;
				}
			}

			return retVal;
		}

		//public void UpdateCountryCode(DataTable tbl, CQAPICountryList countryRes)
		//{

		//	//SqlDataReader rdr = cmd.ExecuteReader();
		//	for (int i = 0; i < tbl.Rows.Count; i++)
		//	{
		//		Console.WriteLine(string.Format(" Identifying country code for {0}", tbl.Rows[i]["Id"]));
		//		string countryCode = string.Empty;
		//		string transDesc = tbl.Rows[i]["TransDesc"].ToString().ToUpper();
		//		bool toSearch = false;

		//		//if (tbl.Rows[i]["Id"].ToString().ToUpper() == "A3F83D7B-F7D0-4A83-B854-038DFCE5C650")
		//		//{
		//		//string mel = string.Empty;
		//		//}
		//		//tbl.Rows[i]["CountryCode"] = "US";
		//		switch (transDesc)
		//		{
		//			case "ACH CREDIT":
		//				countryCode = "US";
		//				break;
		//			case "ACH DEBIT":
		//				countryCode = "US";
		//				break;
		//			case "ISSUE ? DRAFT":
		//				if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
		//				}
		//				break;
		//			case "INTERNAL TRANSFER":
		//				if (tbl.Rows[i]["BE_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
		//				}
		//				break;
		//			case "CHAPS PAYMENT RECD":
		//				if (tbl.Rows[i]["BE_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
		//				}
		//				break;
		//			case "CCY RECD":
		//				if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
		//				}

		//				break;
		//			case "SAME DAY CR TRANSFER":
		//				if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
		//				}
		//				else if (tbl.Rows[i]["OB_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["OB_Id"].ToString().Substring(0, 2);
		//				}
		//				else if (tbl.Rows[i]["BE_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
		//				}
		//				break;
		//			case "SAME DAY DR TRANSFER":
		//				if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
		//				}
		//				else if (tbl.Rows[i]["OB_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["OB_Id"].ToString().Substring(0, 2);
		//				}
		//				else if (tbl.Rows[i]["BE_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
		//				}
		//				break;
		//			case "WIRE PYMT XB EU/EEA AUTO":
		//				if (tbl.Rows[i]["BO_Id"].ToString().Length > 1)
		//				{
		//					countryCode = tbl.Rows[i]["BO_Id"].ToString().Substring(0, 2);
		//				}
		//				else
		//				{
		//					if (tbl.Rows[i]["OB_Id"].ToString() == "" || tbl.Rows[i]["BO_Id"].ToString() == "")
		//					{
		//						countryCode = tbl.Rows[i]["BE_Id"].ToString().Substring(0, 2);
		//					}
		//				}
		//				break;
		//			default:
		//				break;

		//		}
		//		if (countryCode != "")
		//		{
		//			bool validCode = ValidCountryCode(countryRes, countryCode);

		//			if (validCode)
		//			{
		//				tbl.Rows[i]["CountryCode"] = countryCode;
		//				tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
		//				Console.Write(string.Format(" - {0} identified", countryCode));
		//			}
		//			else // look for Country from Bank Order Address
		//			{
		//				string fieldVal = string.Empty;
		//				string retCountryCode = string.Empty;
		//				for (int r = 3; r >= 0; r--)
		//				{
		//					string fieldCnt = r == 0 ? "" : r.ToString();
		//					fieldVal = tbl.Rows[i]["BO_NameAdd" + fieldCnt].ToString();
		//					if (fieldVal != "")
		//					{
		//						if (fieldVal.Length == 11)  // it is a swift code
		//						{
		//							if (fieldVal.Substring(8, 3) == "XXX")
		//							{
		//								fieldVal = fieldVal.Substring(4, 2);
		//							}
		//						}

		//						retCountryCode = DoSearch(countryRes, fieldVal);
		//						tbl.Rows[i]["CountryCode"] = retCountryCode;
		//						tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
		//						Console.Write(string.Format(" - {0} identified", retCountryCode));
		//						if (retCountryCode != "")
		//						{
		//							r = 0;
		//						}
		//					}
		//				}

		//				validCode = ValidCountryCode(countryRes, retCountryCode);
		//				if (validCode)
		//				{
		//					tbl.Rows[i]["CountryCode"] = retCountryCode;
		//					tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
		//					Console.Write(string.Format(" - {0} identified", retCountryCode));
		//				}
		//				else// look for Country from Order By Details Address
		//				{
		//					for (int r1 = 3; r1 >= 0; r1--)
		//					{
		//						string fieldCnt = r1 == 0 ? "" : r1.ToString();
		//						fieldVal = tbl.Rows[i]["OB_NameAdd" + fieldCnt].ToString();
		//						if (fieldVal != "")
		//						{
		//							retCountryCode = DoSearch(countryRes, fieldVal);
		//							tbl.Rows[i]["CountryCode"] = retCountryCode;
		//							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
		//							Console.Write(string.Format(" - {0} identified", retCountryCode));
		//							if (retCountryCode != "")
		//							{
		//								r1 = 0;
		//							}
		//						}
		//					}
		//				}


		//				validCode = ValidCountryCode(countryRes, retCountryCode);
		//				if (validCode)
		//				{
		//					tbl.Rows[i]["CountryCode"] = retCountryCode;
		//					tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
		//					Console.Write(string.Format(" - {0} identified", retCountryCode));
		//				}
		//				else // look for Country from Deneficiary Details Address
		//				{
		//					for (int r2 = 3; r2 >= 0; r2--)
		//					{
		//						string fieldCnt = r2 == 0 ? "" : r2.ToString();
		//						fieldVal = tbl.Rows[i]["BE_NameAdd" + fieldCnt].ToString();
		//						if (fieldVal != "")
		//						{
		//							retCountryCode = DoSearch(countryRes, fieldVal);
		//							tbl.Rows[i]["CountryCode"] = retCountryCode;
		//							tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, retCountryCode);
		//							Console.Write(string.Format(" - {0} identified", retCountryCode));
		//							if (retCountryCode != "")
		//							{
		//								r2 = 0;
		//							}
		//						}
		//					}
		//				}

		//			}
		//		}
		//		else
		//		{
		//			tbl.Rows[i]["CountryCode"] = countryCode;
		//			tbl.Rows[i]["CountryName"] = GetCountryName(countryRes, countryCode);
		//			Console.Write(string.Format(" - {0} identified", countryCode));
		//		}


		//	}

		//	conn = new SqlConnection(connString);
		//	conn.Open();
		//	SqlCommand cmd = new SqlCommand("USP_PCI_BankTrans_Update", conn);
		//	//cmd.Parameters.Add(
		//	cmd.CommandType = CommandType.StoredProcedure;
		//	cmd.Parameters.AddWithValue("@tblBankTransUpdate", tbl);
		//	cmd.ExecuteNonQuery();
		//	conn.Close();


		//}

		public string MatchAccount(string input)
		{
			string result = string.Empty;
			MatchCollection match = Regex.Matches(input, @"[789][a-zA-Z0-9]{6}|[789][a-zA-Z]{2}\s*\d{4}|[789][a-zA-Z]{3}\s*\d{3}");
			List<string> matches = match.Select(a => a.Value.Replace(" ", "")).Distinct().Where(a => !Regex.Match(a, @"[7][a-zA-z]{2}[Rr]{1}[A-Za-z0-9]{3}").Success).ToList();
			if (matches.Count > 0)
			{
				foreach (var item in matches)
				{
					if (item.Substring(0, 1).ToString() == "7")
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
				if (cCode.cd_Ref != "")
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
			if (cCode != null)
			{
				if (cCode.cd_Ref != "")
				{
					retVal = cCode.cd_Ref;
				}
			}

			return retVal;
		}


		public bool IsUSRegion(CQAPIRegionList regionList, string regionCode)
		{
			bool retVal = false;

			var cCode = regionList.data.Where(x => x.form_CodeID == regionCode).FirstOrDefault();
			if (cCode != null)
			{
				if (cCode.form_CodeID != "")
				{
					retVal = true;
				}
			}
			return retVal;
		}

		private string DoSearch(CQAPICountryList countryRes, string fieldVal, CQAPIRegionList regionRes)
		{
			string retVal = string.Empty;
			string newFldVal = fieldVal.Replace(","," ");
			newFldVal = newFldVal.Replace("/", " ");
			newFldVal = newFldVal.Replace(".", " ");

			// Search for Address Field in Bank Order fields Address, 1 ,2 and 3
			bool validCode = false;
			string[] arrVal = newFldVal.Split(" ");
			foreach (var val in arrVal.Reverse())
			{
				string newVal = val.Replace(",", "");
				//newVal = val.Replace(".", "");
				//newVal = val.Replace("/", "");
				//newVal = val.Replace(@"\", "");
				newVal = newVal.ToString() == "USA" ? "US" : newVal.ToString();

				if (IsUSRegion(regionRes, val.ToString()))
				{
					newVal = "US";
				}
				else
				{
					newVal = newVal.ToString() == "YORK" ? "US" : newVal.ToString() == "NEWYORK" ? "US" : newVal.ToString();
					newVal = newVal.ToString() == "MEX" ? "MX" : newVal.ToString() ;
					newVal = newVal.ToString() == "ENGLAND" ? "GB" : newVal.ToString() == "LONDON" ? "GB" : newVal.ToString();
					newVal = newVal.ToString() == "SAUDI" ? "SA" : newVal.ToString() == "KSA" ? "SA" : newVal.ToString();
					newVal = newVal.ToString() == "RUSSIAN" ? "RU" : newVal.ToString() == "RUSSIA" ? "RU" : newVal.ToString();
					newVal = newVal.ToString() == "JOHANNESBURG" ? "ZA" : newVal.ToString() == "AFRICA" ? "ZA" : newVal.ToString() == "PIETERMARITZBURG" ? "ZA" : newVal.ToString(); //PIETERMARITZBURG
					newVal = newVal.ToString() == "UAE" ? "AE" : newVal.ToString() == "EMIRATES" ? "AE" : newVal.ToString() == "EMIRATES/AE" ? "AE" : newVal.ToString(); 
					newVal = newVal.ToString() == "AIRESARGENTINA" ? "AR" : newVal.ToString() == "AIRES" ? "AR" : newVal.ToString();
					newVal = newVal.ToString() == "DOMINICANA" ? "DO" : newVal.ToString() == "DOMINICAN" ? "DO" : newVal.ToString();
					newVal = newVal.ToString() == "HONG" ? "HK" : newVal.ToString() == "KONG" ? "HK" : newVal.ToString();
					newVal = newVal.ToString() == "NSW" ? "AU" : newVal.ToString() == "WALES" ? "AU" : newVal.ToString();
					newVal = newVal.ToString() == "VIET" ? "VN" : newVal.ToString() == "HANOI" ? "VN" : newVal.ToString();
				}

				
				validCode = ValidCountryCode(countryRes, newVal);
				if (validCode)
				{
					retVal = newVal.ToString();
					break;
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

		public void GetAMLReport()
		{
			//[USP_PCI_BankTrans_Get]
			conn = new SqlConnection(connString);
			SqlCommand cmd = new SqlCommand("USP_PCI_AML_ReportGet", conn);

			cmd.CommandType = CommandType.StoredProcedure;
			conn.Open();

			SqlDataReader rdr = cmd.ExecuteReader();
			DataTable tbl = new DataTable();

			tbl.Load(rdr);
			conn.Close();

			
			if (tbl.Rows.Count > 0)
            {
				InsertAMLReport(tbl);

			}
		}

		public void InsertAMLReport(DataTable tblAML)
        {
			try
			{
				using (var connection = new SqlConnection(connString))
				{
					SqlCommand cmdInsert = new SqlCommand("USP_PCI_AML_Report_Insert", connection);
					cmdInsert.CommandType = CommandType.StoredProcedure;
					connection.Open();
					cmdInsert.Parameters.AddWithValue("@PCI_AMLReportType", tblAML);
					cmdInsert.ExecuteNonQuery();
					Console.WriteLine("Done Insert: " + tblAML.Rows.Count + " rows");
					connection.Close();
				
				}

				Console.WriteLine("Done Insert AMLReport");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

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

