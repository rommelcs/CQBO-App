using Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static AMLApp.Models.CQBOAPIModels;

namespace AMLApp.Controller
{
    public class processCiti
    {

        static string connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
        public void processCitiFiles(string fileDate)
        {
            string fileXLS = "";
            int filecount = 0;

            XlsReader xlsReader = new XlsReader();
            XlsReader.FileTypes fileType = XlsReader.FileTypes.XLSType;
            string path = ConfigurationManager.AppSettings.Get("CitiSourceFolder");

            //Step 1 : Read all available file in the directory

            string fileToProcess = "*" +fileDate+ "*.*";
            string[] files = Directory.GetFiles(path, fileToProcess, SearchOption.AllDirectories);
            Console.WriteLine();

            if (files.Length != 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    filecount++;
                    fileXLS = "";
                    string fileXLSToProcess = "";

                    // Get the file from path
                    fileXLS = @files[i];

                    string fileExt = Path.GetExtension(fileXLS);
                    //Rename the file to XLS format
                    if (fileExt.ToLower() != ".xls")
                    {
                        File.Move(fileXLS, fileXLS + ".xls");
                        fileXLSToProcess = fileXLS + ".xls";
                    }
                    else
                    {
                        fileXLSToProcess = fileXLS;
                    }

                    // ReadFile(sampleCSV);
                    DataTable xlsDt = xlsReader.ReadXLSToDT(fileType, fileXLSToProcess);
                    if (xlsDt.Columns.Count == 7)
                    {
                        processCitiData(xlsDt, fileXLSToProcess);
                    }
                    else
                    {
                        Console.WriteLine(string.Format("skippiing {0} file as it seems empty", fileXLSToProcess));
                    }


                }
            }
        }

        static void processCitiData(DataTable xlsDt, string filePath)
        {
            try
            {
                int rowCount = xlsDt.Rows.Count;
                int colCount = xlsDt.Columns.Count;
                string[] masterTableValue = new string[colCount + 5]; //5 additional columns
                string strFilename = Path.GetFileName(filePath);
                int colCnt = 0;
                string[,] arrayMT = new string[rowCount + 1, colCount + 5];

                int xlslineno = 0;

                string PrevMainPrefix = "";
                string PrevChilColName = "";
                string CurrChilColName = "";
                string colName = "";
                int PrevColNo = 0;
                string iscurrColbankref = "N";
                string bankrefNo = null;

                DataTable MasterDT = new DataTable();
                MasterDT.Columns.Add("id");
                MasterDT.Columns.Add("XlsLineNo");
                MasterDT.Columns.Add("RefNo");
                MasterDT.Columns.Add("SourceFile");
                MasterDT.Columns.Add("Column1");
                MasterDT.Columns.Add("Column2");
                MasterDT.Columns.Add("Column3");
                MasterDT.Columns.Add("Column4");
                MasterDT.Columns.Add("Column5");
                MasterDT.Columns.Add("Column6");
                MasterDT.Columns.Add("Column7");
                MasterDT.Columns.Add("status");

                int r = 0;

                foreach (DataRow row in xlsDt.Rows)
                {
                    r++;
                    xlslineno = r;
                    Console.Write(string.Format("Processing data {0} of {1}\r", r, xlsDt.Rows.Count));

                    arrayMT[r, 0] = Guid.NewGuid().ToString(); //id
                    arrayMT[r, 1] = xlslineno.ToString(); //xlascount
                                                          // arrayMT[r, 2] = ""; //RefNo
                    arrayMT[r, 3] = strFilename; //sourcefile

                    colCnt = 4;
                    //for (int a = 0; a < 7; a++)
                    //{
                    //    Console.WriteLine(a + " : " + row[a].ToString() + " ");
                    //}

                    for (int c = 0; c < colCount; c++)
                    {
                        // Console.WriteLine("r: " + r+" c: " + c+" "+values[r, c] );

                        if (c == 0)
                        {
                            if (row[c].ToString() == "Bank Reference")
                            {
                                bankrefNo = row[1].ToString();
                                // Console.WriteLine(worksheet.Cells[r, 2].Value);

                            }

                            if (row[c].ToString() == "Name/Address" || row[c].ToString() == "Name/Address")
                            {
                                PrevChilColName = PrevMainPrefix + row[c].ToString(); // prefix + Name/Address
                            }

                            if (row[c].ToString() == " " || row[c].ToString() == "")
                            {
                                CurrChilColName = PrevChilColName + PrevColNo.ToString();  // prefix + Name/Address + PrevColNo
                                PrevColNo++;
                            }
                            else
                            {
                                if (row[c].ToString() == "Name/Address")
                                {
                                    CurrChilColName = PrevMainPrefix + row[c].ToString();
                                }
                                else
                                {
                                    PrevMainPrefix = "";
                                    CurrChilColName = row[c].ToString();
                                }
                                PrevColNo = 1;

                            }

                            PrevMainPrefix = getPrefixCol(PrevMainPrefix, row[c].ToString());
                        }

                        //if (PrevMainPrefix != null)
                        //{
                        //    Console.WriteLine("***********r: "+(r+1)+ " ******c: "+(c+1)+" ****************** " + PrevMainPrefix +""+values[r, c]);
                        // }

                        if (c == 0)
                        {
                            if (row[c].ToString() == "Name/Address")
                            {
                                arrayMT[r, colCnt] = CurrChilColName;
                            }
                            else if (row[c].ToString() == "" || row[c].ToString() == " ")
                            {
                                arrayMT[r, colCnt] = CurrChilColName;
                            }
                            else
                            {
                                arrayMT[r, colCnt] = row[c].ToString();
                                PrevColNo = 0;
                            }
                        }

                        else
                        {
                            if (row[c].ToString() == null)
                            {
                                arrayMT[r, colCnt] = null;
                            }
                            else { arrayMT[r, colCnt] = row[c].ToString(); }

                        }


                        colCnt++;



                    }
                    arrayMT[r, 11] = null;//status
                    arrayMT[r, 2] = bankrefNo; //RefNo
                                               // Console.Write("");

                }

                //  Console.WriteLine("Loop the array");
                for (int k = 1; k <= rowCount; k++)
                {
                    //Console.WriteLine("k: " + k + " : " + masterTableValue[k]);
                    for (int m = 0; m <= arrayMT.GetUpperBound(1); m++)
                    {

                        // Console.WriteLine("k: " + (k) + " m: "+(m)+" : " + arrayMT[k, m]);

                        string colVal = "";


                        if ((m >= 5) && (m <= 10))
                        {
                            if (arrayMT[k, m].Contains("'"))
                            {
                                colVal = arrayMT[k, m].Replace("'", "''");
                            }
                            else
                            {
                                colVal = arrayMT[k, m];
                            }

                        }
                        else
                        {
                            colVal = arrayMT[k, m];
                        }





                        masterTableValue[m] = colVal;

                    }

                    //if ((k != 7438) || (k != 7467))
                    //{
                    // Console.WriteLine("k: "+k);

                    MasterDT.Rows.Add(masterTableValue); // insert array into data table

                    // }
                    // Console.WriteLine(" ");
                }

                InsertCitiRawFilexls(MasterDT);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ReadXLSFinal:  " + ex.Message);
            }
        }

        public void IdentifyCitiTransCountry()
        {
            Console.WriteLine("Identifying Citi Transactions Country..");
            CQBO_API.IndetifyCitiTransCountry();
            Console.WriteLine("\n Citi Transactions country identification ... DONE");
        }

        private static string getPrefixCol(string PrevMainPrefix, string colValue)
        {
            //if(colValue == "Originating Bank Account/ID")
            //{

            //    Console.WriteLine(colValue);
            //}
            // string PrevMainPrefix =  null;
            switch (colValue)
            {
                case "Ordering Bank Account/ID":
                    PrevMainPrefix = "OR_";
                    break;
                case "By Order of Account/ID":
                    PrevMainPrefix = "BY_";
                    break;
                case "Beneficiary Account/ID":
                    PrevMainPrefix = "BE_";
                    break;
                case "Beneficiary Bank Account/ID":
                    PrevMainPrefix = "BB_";
                    break;
                default:
                    break;
            }

            return PrevMainPrefix;
        }

        private static void InsertCitiRawFilexls(DataTable tbl)
        {

            try
            {
                // Console.Write("paramvalue: "+ paramvalue.Length);
                //var connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
                // string id = Guid.NewGuid().ToString();

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmdInsert = new SqlCommand("USP_PCI_Citi_RawFile_Insert", connection);
                    cmdInsert.CommandType = CommandType.StoredProcedure;
                    cmdInsert.Parameters.AddWithValue("@PCI_CitiRawFileType", tbl);
                    cmdInsert.ExecuteNonQuery();
                    Console.WriteLine("\n Done Insert: " + tbl.Rows.Count + " rows");
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Error in InsertCitiRawFilexls:  " + e.Message);
            }

        }

        public void processCitiData()
        {
            try
            {
                // Console.Write("paramvalue: "+ paramvalue.Length);
                //var connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
                // string id = Guid.NewGuid().ToString();

                using (var connection = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("USP_PCI_GetRawTrans_Citi", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    SqlDataReader rdr = cmd.ExecuteReader();


                    DataTable tbl = new DataTable();

                    tbl.Load(rdr);
                    connection.Close();

                    insertAMLCitiData(tbl);
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Error in InsertCitiRawFilexls:  " + e.Message);
            }
        }

        public void insertAMLCitiData(DataTable tbl)
        {

            try
            {

                using (var connection = new SqlConnection(connectionString))
                {

                    //for (int i = 0; i < tbl.Rows.Count; i++)
                    //	{
                    SqlCommand cmdInsert = new SqlCommand("USP_PCI_BankTrans_Insert", connection);
                    cmdInsert.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    cmdInsert.Parameters.AddWithValue("@PCI_BankTransType", tbl);
                    cmdInsert.ExecuteNonQuery();
                    connection.Close();
                    //}
                }

                Console.WriteLine("Done Insert: " + tbl.Rows.Count + " rows");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
