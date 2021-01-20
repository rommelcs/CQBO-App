using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Configuration;
using System.Data.SqlClient;
using ExcelDataReader;
using System.Diagnostics;
using ClosedXML.Excel;
using GemBox.Spreadsheet;

namespace CSVFileReader.Controller
{
   
    public class Read_from_excel
    {

    
        private static DataSet ds;
      

       
       

        public static void readXLSEPP()
        {
            string sampleXLS = "";
            int filecount = 0;

            //get folder path
            Console.WriteLine("Input file path");
          //  string path = Console.ReadLine();

            try
            {
                // string FilePath = @"D:\Working Folder\2020\02_PhillipUS\Projects\13_CSVFileReader_AML_Citi\01_SourceFile\SampleFile\ExcelFile\citixlsinc202007200903.xls";//the folder where keeps all your excel files
                string path = @"D:\Working Folder\2020\02_PhillipUS\Projects\13_CSVFileReader_AML_Citi\01_SourceFile\SampleFile\ExcelFile\";
                Console.WriteLine(path);

                //Step 1 : Get All XLS Filnames
                string[] files = Directory.GetFiles(path, "*.xls", SearchOption.AllDirectories);
                Console.WriteLine();

                //Step 2 : Loop XLS File based on filename
                if (files.Length != 0)
                {
                    for (int k = 0; k < files.Length; k++)
                    {
                        filecount++;
                        sampleXLS = "";
                        // Console.WriteLine();

                        //Print file name
                        // Console.Write("******* file " + filecount + " : " + files[k] + " *******\n");

                        // Get the data from path.
                        sampleXLS = @files[k];

                        //Step 3 : Read the csv file
                        // ReadFile(sampleCSV);
                        Console.WriteLine(sampleXLS);
                        ReadXLSFinal(sampleXLS);
                    }
                }

                else
                {
                    Console.WriteLine("###You did not input a valid path or no xls file found. Please try again. ");
                }
                Console.WriteLine();
                Console.WriteLine("No fo files found : " + files.Length);

                Console.WriteLine("Records have been saved into DB Successfully. Please check.");


            }
            catch
            {
                Console.WriteLine("***You did not input a valid path or no xls file found");
            }
        }


        public static void ReadXLSFinal(string FilePath)
        {
            //string FilePath = @"D:\Working Folder\2020\02_PhillipUS\Projects\13_CSVFileReader_AML_Citi\01_SourceFile\SampleFile\ExcelFile\citixlsinc202007200903.xls";//the folder where keeps all your excel files


            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            //open file and returns as Stream
            using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = false,
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = false
                        }
                    }); ;
                  //  Console.WriteLine("Yeay");
                }

            }
            // var tablenames = GetTablenames(ds.Tables);
            DataTable dt = ds.Tables[0];
            int rowCount = dt.Rows.Count;
            int colCount = dt.Columns.Count;
            string[] masterTableValue = new string[colCount + 6]; //5 additional columns
            string strFilename = Path.GetFileName(FilePath);
            int colCnt = 0;
            string[,] arrayMT = new string[rowCount + 1, colCount + 6];

            int xlslineno = 0;

            string PrevMainPrefix = "";
            string PrevChilColName = "";
            string CurrChilColName = "";
            string colName = "";
            int PrevColNo = 0;
            string iscurrColbankref = "N";
            string bankrefNo = null;

            int r = 0;

            foreach (DataRow row in dt.Rows)
            {
                r++;
                xlslineno = r;


                arrayMT[r, 1] = Guid.NewGuid().ToString(); //id
                arrayMT[r, 2] = xlslineno.ToString(); //xlascount
                                                      // arrayMT[r, 2] = ""; //RefNo
                arrayMT[r, 4] = strFilename; //sourcefile

                colCnt = 5;
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
                arrayMT[r, 12] = null;//status
                arrayMT[r, 3] = bankrefNo; //RefNo
                                           // Console.Write("");

            }

            //  Console.WriteLine("Loop the array");
            for (int k = 1; k <= rowCount; k++)
            {
                //Console.WriteLine("k: " + k + " : " + masterTableValue[k]);
                for (int m = 1; m <= arrayMT.GetUpperBound(1); m++)
                {

                    // Console.WriteLine("k: " + (k) + " m: "+(m)+" : " + arrayMT[k, m]);
                    masterTableValue[m] = arrayMT[k, m];

                }

                //if ((k != 7438) || (k != 7467))
                //{
                // Console.WriteLine("k: "+k);
                InsertCitiRawFilexls(k, masterTableValue);
                // }
                // Console.WriteLine(" ");
            }





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
                default:
                    break;
            }

            return PrevMainPrefix;
        }

        private static void InsertCitiRawFilexls(int line_no, string[] paramvalue)
        {

            try
            {
                string column1 = paramvalue[5];
                string column2 = paramvalue[6];
                string column3 = paramvalue[7];
                string column4 = paramvalue[8];
                string column5 = paramvalue[9];
                string column6 = paramvalue[10];

                string col1Val = column1;
                string col2Val = column2;
                string col3Val = column3;
                string col4Val = column4;
                string col5Val = column5;
                string col6Val = column6;




                if (paramvalue[5].Contains("'"))
                {
                    col1Val = column1.Replace("'", "''");
                }
                
                if (paramvalue[6].Contains("'"))
                {
                    col2Val = column2.Replace("'", "''");
                }
                if (paramvalue[7].Contains("'"))
                {
                    col3Val = column3.Replace("'", "''");
                }
                if (paramvalue[8].Contains("'"))
                {
                    col4Val = column4.Replace("'", "''");
                }
                if (paramvalue[9].Contains("'"))
                {
                    col5Val = column5.Replace("'", "''");
                }
                if (paramvalue[10].Contains("'"))
                {
                    col6Val = column6.Replace("'", "''");
                }



                // Console.Write("paramvalue: "+ paramvalue.Length);
                var connectionString = ConfigurationManager.ConnectionStrings["DRConn"].ConnectionString;
                string id = Guid.NewGuid().ToString();
                using (var connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    SqlCommand insertCommand = new SqlCommand("INSERT INTO [dbo].[PCI_Citi_RawFile_Citi] (id,[XlsLineNo],[RefNo] ,[SourceFile] ,[Column1],[Column2] ,[Column3] ,[Column4],[Column5] ,[Column6],[Column7] ,[Status])" + "" +
                    //" VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)", connection);
                    " VALUES ('" + id +
                    "'," + paramvalue[2] +
                    ",'" + paramvalue[3] +
                    "','" + paramvalue[4] +
                    "','" + col1Val+//paramvalue[5] +
                    "','" + col2Val + // "','" + paramvalue[6] +
                    "','" + col3Val + // paramvalue[7] +
                    "','" + col4Val + //paramvalue[8] +
                    "','" + col5Val + // paramvalue[9] +
                    "','" + col6Val + // paramvalue[10] +
                    "','" + paramvalue[11] +
                    "','" + paramvalue[12] +
                    "')", connection);

                    //insertCommand.Parameters.Add(new SqlParameter("0", paramvalue[0]));// XlsLineNo
                    //insertCommand.Parameters.Add(new SqlParameter("1", paramvalue[1]));//RefNo
                    //insertCommand.Parameters.Add(new SqlParameter("2", paramvalue[2]));//SourceFile
                    //insertCommand.Parameters.Add(new SqlParameter("3", paramvalue[3]));//Column1
                    //insertCommand.Parameters.Add(new SqlParameter("4", paramvalue[4]));//Column2
                    //insertCommand.Parameters.Add(new SqlParameter("5", paramvalue[5]));//Column3
                    //insertCommand.Parameters.Add(new SqlParameter("6", paramvalue[6]));//Column4
                    //insertCommand.Parameters.Add(new SqlParameter("7", paramvalue[7]));//Column5
                    //insertCommand.Parameters.Add(new SqlParameter("8", paramvalue[8]));//Column6
                    //insertCommand.Parameters.Add(new SqlParameter("9", paramvalue[9]));//Column 7
                    //insertCommand.Parameters.Add(new SqlParameter("10", paramvalue[10]));//status
                    //insertCommand.Parameters.Add(new SqlParameter("11", paramvalue[11]));//status

                    
                        insertCommand.ExecuteNonQuery();
                    

                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Error in InsertMasterTable: line_no: " + line_no );
            }

        }
    }
}
