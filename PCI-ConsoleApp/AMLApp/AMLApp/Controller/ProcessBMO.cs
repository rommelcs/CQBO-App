using Helpers;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AMLApp.Controller
{
	public class ProcessBMO
	{
        static string connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
        public void processBMOFiles(string fileDate)
        {
            loadFile(fileDate);
        }

        private static void loadFile(string fileDate)
        {
            string sampleCSV = "";
            int filecount = 0;
            string path = ConfigurationManager.AppSettings.Get("BMOSourceFolder");
            string fileToProcess = "*" + fileDate + "*.csv";

            try
            {

                //Step 1 : Get All CSV Filnames

                string[] files = Directory.GetFiles(path, fileToProcess, System.IO.SearchOption.AllDirectories);
                Console.WriteLine();

                //Step 2 : Loop CSV File based on filename
                if (files.Length != 0)
                {
                    for (int k = 0; k < files.Length; k++)
                    {
                        filecount++;
                        sampleCSV = "";

                        // Get the data from path.
                        sampleCSV = @files[k];

                        if (new FileInfo(sampleCSV).Length == 0)
                        {
                            Console.WriteLine("{0} is empty", sampleCSV);

                        }
                        else
                        {
                            //Step 3 : Read the csv file
                            Console.Write("Processing file {0} of {1} - {2}", k+1, files.Length, sampleCSV);
                            ReadFile(sampleCSV);
                        }

                        Console.WriteLine("\nProcessing file {0} of {1} - DONE", k + 1, files.Length);
                    }
                } else
                {
                    Console.WriteLine("No csv file found. Please try again. ");
                }
            }
            catch
            {
                Console.WriteLine("*** Not a valid path or no csv file found..");
            }
        }

        private static void ReadFile(string filename)
        {

            try
            {
                if (new FileInfo(filename).Length == 0)
                {
                    Console.WriteLine("{0} is empty", filename);
                    return;
                }

                string[,] values = LoadCSV(filename);
                int num_rows = values.GetUpperBound(0) + 1;
                int num_cols = values.GetUpperBound(1) + 1;

                //Display the data to show we have it.
                // Console.Write("num_cols: " + num_cols + "\n");
                //Console.Write("no of rows: " + num_rows + "\n");


                string[] masterTableValue = new string[num_cols + 3];
                string strFilename = Path.GetFileName(filename);
                int arraycount = 0;
                string detail_id = "";
                string strColM = "";
                masterTableValue[arraycount] = strFilename;


                DataTable MasterDT = new DataTable();
                MasterDT.Columns.Add("FileName");
                MasterDT.Columns.Add("Col_A");
                MasterDT.Columns.Add("Col_B");
                MasterDT.Columns.Add("Col_C");
                MasterDT.Columns.Add("Col_D");
                MasterDT.Columns.Add("Col_E");
                MasterDT.Columns.Add("Col_F");
                MasterDT.Columns.Add("Col_G");
                MasterDT.Columns.Add("Col_H");
                MasterDT.Columns.Add("Col_I");
                MasterDT.Columns.Add("Col_J");
                MasterDT.Columns.Add("Col_K");
                MasterDT.Columns.Add("Col_L");
                MasterDT.Columns.Add("Col_M");
                MasterDT.Columns.Add("Detail_ID");
                MasterDT.Columns.Add("FileUploadTime");

                DataTable DetailDT = new DataTable();
                DetailDT.Columns.Add("ID");
                DetailDT.Columns.Add("Field_Name");
                DetailDT.Columns.Add("Field_Value");
                DataRow detailRow = DetailDT.NewRow();

                for (int c = 0; c < num_cols; c++) // Read columns  
                {
                    if (c == 12)  // extract column 12 fields
                    {
                        strColM = "";

                        strColM = values[0, c];
                    }
                    arraycount++;
                    masterTableValue[arraycount] = values[0, c]; //a
                }

                arraycount++;

                masterTableValue[arraycount] = detail_id;

                arraycount++;
                masterTableValue[arraycount] = DateTime.Now.ToString();

                if (masterTableValue[13] != null)
                {
                    MasterDT.Rows.Add(masterTableValue);
                    // InsertMasterTable(masterTableValue);

                    DetailDT = SplitDetailsColumn(detail_id, strColM, DetailDT);
                }

                //read remaining rows
                //Step 3 : Read Row by rom
                for (int r = 1; r < num_rows; r++)
                {

                    //Console.Write("");
                    //read column by column




                    arraycount = 0;
                    detail_id = Guid.NewGuid().ToString();

                    for (int c = 0; c < num_cols; c++)
                    {

                        if (c == 12)
                        {
                            strColM = "";

                            strColM = values[r, c];
                        }
                        arraycount++;
                        masterTableValue[arraycount] = values[r, c]; //a


                    }
                    arraycount++;
                    // Console.WriteLine("arraycount: " + arraycount +":"+ strFilename + "_" + r);
                    masterTableValue[arraycount] = detail_id;

                    arraycount++;
                    masterTableValue[arraycount] = DateTime.Now.ToString();

                    // InsertMasterTable(masterTableValue);
                    MasterDT.Rows.Add(masterTableValue);

                    detailRow = DetailDT.NewRow();
                    DetailDT = SplitDetailsColumn(detail_id, strColM, DetailDT);
                    //  DetailDT.Rows.Add(detailRow);

                }

                InsertMasterTable(MasterDT);
                InsertDetailTable(DetailDT);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Reading File : " + ex.Message);
            }
        }

        private static DataTable SplitDetailsColumn(string id, string value, DataTable DetailDT)
        {
            try
            {
                string result = Regex.Replace(value, @".{3}(?:=)", @"#$&"); // to find XXX=

                string[] tryw = result.Split("#", StringSplitOptions.RemoveEmptyEntries);

                foreach (string abc in tryw)
                {


                    string firstsplitstring = abc;

                    if ((firstsplitstring.Contains('=')) && firstsplitstring.Length < 5)
                    {
                        firstsplitstring = firstsplitstring + "UNDEFINED";

                    }
                    else if ((!firstsplitstring.Contains('=')) && firstsplitstring.Length > 5)
                    {
                        firstsplitstring = "UNDEFINED=" + firstsplitstring;
                    }

                    string[] splitvalue = firstsplitstring.Split("=", StringSplitOptions.RemoveEmptyEntries);

                    if (splitvalue.Length == 2)
                    {
                        string fldName = splitvalue[0];
                        string[]  rawDet = splitvalue[1].Split("|");
                        for (int i = 0; i < rawDet.Length; i++)
                        {
                            string newfldName = fldName + (i==0 ? "" : i.ToString());
                            DetailDT.Rows.Add(id, newfldName, rawDet[i].ToString());
                        }
                        
                    }

                }

                string subjectString = @value;
            }
            catch (Exception ex)
            {
                Console.Write("\n Error in SplitDetailsColumn " + ex.Message + " \n");
            }
            return DetailDT;
        }

        private static void InsertMasterTable(DataTable tbl)
        {
            try
            {
                // Console.Write("paramvalue: "+ paramvalue.Length);
                using (var connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    //[USP_PCI_DR_Report_Master]


                    SqlCommand cmdInsert = new SqlCommand("USP_PCI_BMORawTrans_Insert", connection);
                    cmdInsert.CommandType = CommandType.StoredProcedure;

                    cmdInsert.Parameters.AddWithValue("@PCI_DRMasterType", tbl);

                    cmdInsert.ExecuteNonQuery();
                    Console.WriteLine("\nDone Insert Master Table: " + tbl.Rows.Count + " rows");
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("\nError in InsertMasterTable: " + e.ToString());
            }

        }

        private static void InsertDetailTable(DataTable tbl)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmdInsert = new SqlCommand("USP_PCI_BMORawTransDetails_Insert", connection);
                    cmdInsert.CommandType = CommandType.StoredProcedure;
                    cmdInsert.Parameters.AddWithValue("@PCI_DRDetailsType", tbl);

                    cmdInsert.ExecuteNonQuery();


                    Console.WriteLine("\nDone Insert Detail Table: " + tbl.Rows.Count + " rows");
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private static string[,] LoadCSV(string filename)
        {

            // Read the csv file
            string csv_file = System.IO.File.ReadAllText(filename);

            // Split into lines.
            //csv_file = csv_file.Replace('\n', '\r'); //replace new line with escape character
            //string[] lines = csv_file.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries); // split the line with escape character
            string[] lines = csv_file.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries); // split the line with escape character

            // Console.WriteLine("LoadCSV: "+lines.Length);

            

            //get no of row and columns
            int num_rows = lines.Length;
            int num_cols = 13; //13

            // Create Array for the data.
            string[,] values = new string[num_rows, num_cols];
            try
            {
                // Load the array.
                for (int r = 0; r < num_rows; r++)
                {
                    TextFieldParser parser = new TextFieldParser(new StringReader(lines[r]));
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(",");

                    string[] fields;

                    while (!parser.EndOfData)
                    {
                        int c = 0;
                        fields = parser.ReadFields();
                        foreach (string field in fields)
                        {
                            values[r,c] = field.ToString();
                            Console.WriteLine(field);
                            c++;
                        }
                    }

                    parser.Close();

                    //string[] line_r = lines[r].Split(',');

                    //for (int c = 0; c < num_cols; c++)
                    //{

                    //    string strVal = line_r[c].ToString().Replace("\"", ""); //this giving error
                    //    values[r, c] = strVal.Replace("$","");
                    //    Console.Write("\nvalues:" + values[r, c].ToString().Replace("\"",""));
                    //}

                }
                //  Console.Write("values:" +values.Length);
            }
            catch (Exception ex)
            {
                Console.Write("########" + filename + ": Error in Loading CSV file :" + ex.Message + "\n");
            }


            // Return the values.
            return values;
        }

        public void GetBankTrans()
        {
            //[USP_PCI_BankTrans_Get]
            SqlConnection conn = new SqlConnection(connectionString);
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

            //UpdateBOIDData(tbl);

            //tbl = UpdateCountryCodeData(tbl);

            insertData(tbl);




        }

        public void insertData(DataTable tbl)
        {

            try
            {


                //cmd.Parameters.AddWithValue("@tblBankTransUpdate", tbl);
                using (var connection = new SqlConnection(connectionString))
                {

                    //for (int i = 0; i < tbl.Rows.Count; i++)
                    //{
                    SqlCommand cmdInsert = new SqlCommand("USP_PCI_BankTrans_Insert", connection);
                    cmdInsert.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    cmdInsert.Parameters.AddWithValue("@PCI_BankTransType", tbl);
                    cmdInsert.ExecuteNonQuery();
                    Console.WriteLine("Done Insert: " + tbl.Rows.Count + " rows");
                    connection.Close();
                    //}
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

