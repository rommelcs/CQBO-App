using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace CSVFileReader.Controller
{
   public class AMLCiti
    {


        public static void loadFileCiti()
        {
            string sampleCSV = "";
            int filecount = 0;

            //get folder path
            Console.WriteLine("Input file path");
            string path = Console.ReadLine();

            path = "D:\\Working Folder\\2020\\02_PhillipUS\\Projects\\13_CSVFileReader_AML_Citi\\01_SourceFile\\Sample\\testfile";

            Console.WriteLine("Path: " +path);
            try
            {

                //Step 1 : Get All CSV Filnames
                string[] files = Directory.GetFiles(path, "*.csv", SearchOption.AllDirectories);
                Console.WriteLine();

                //Step 2 : Loop CSV File based on filename
                if (files.Length != 0)
                {
                    for (int k = 0; k < files.Length; k++)
                    {
                        filecount++;
                        sampleCSV = "";
                        // Console.WriteLine();

                        //Print file name
                        // Console.Write("******* file " + filecount + " : " + files[k] + " *******\n");

                        // Get the data from path.
                        sampleCSV = @files[k];

                        //Step 3 : Read the csv file
                        ReadFileCiti(sampleCSV);
                    }
                }

                else
                {
                    Console.WriteLine("###You did not input a valid path or no csv file found. Please try again. ");
                }
                Console.WriteLine();
                Console.WriteLine("No fo files found : " + files.Length);

                Console.WriteLine("Records have been saved into DB Successfully. Please check.");


            }
            catch
            {
                Console.WriteLine("***You did not input a valid path or no csv file found");
            }
        }


        private static void ReadFileCiti(string filename)
        {

            try
            {
                string[,] values = LoadCSVCiti(filename);
                int num_rows = values.GetUpperBound(0) + 1;
                int num_cols = values.GetUpperBound(1) + 1;

                

                //Display the data to show we have it.
                Console.Write("\nnum_cols: " + num_cols + "\n");
                Console.Write("no of rows: " + num_rows + "\n");
                Console.Write("");
                Console.WriteLine("*************Processing :  " + filename + "*************");
                Console.Write("");

                string[] masterTableValue = new string[num_cols + 6]; //5 additional columns
                string strFilename = Path.GetFileName(filename);
                int colcount = 0;
                string[,] arrayMT = new string[num_rows+1, num_cols + 6];

                int xlslineno = 0;

                string PrevMainPrefix = "";
                string PrevChilColName = "";
                string CurrChilColName = "";
                string colName = "";
                int PrevColNo = 0;
                string iscurrColbankref = "N";
                string bankrefNo = null;

                //read remaining rows
                //Step 3 : Read Row by row
                for (int r = 0; r < num_rows; r++)
                {
                    xlslineno = r + 1;

                    
                    arrayMT[r, 0] = Guid.NewGuid().ToString(); //id
                    arrayMT[r, 1] = xlslineno.ToString(); //xlascount
                   // arrayMT[r, 2] = ""; //RefNo
                    arrayMT[r, 3] = strFilename; //sourcefile

                    colcount = 4;
                    for (int c = 0; c < num_cols; c++)
                    {
                        // Console.WriteLine("r: " + r+" c: " + c+" "+values[r, c] );

                        if (c == 0)
                        {
                            if (values[r, c] == "Bank Reference")
                            {
                                bankrefNo = values[r, 1];
                                Console.WriteLine(values[r, 1]);
                               
                            }
                            
                            if (values[r, c] == "Name/Address" || values[r, c] == "Name/Address")
                            {
                                PrevChilColName = PrevMainPrefix + values[r, c]; // prefix + Name/Address
                            }

                            if (values[r, c] == " " || values[r, c] == "")
                            {
                                CurrChilColName = PrevChilColName + PrevColNo.ToString();  // prefix + Name/Address + PrevColNo
                                PrevColNo++;
                            }
                            else
                            {
                                if (values[r, c] == "Name/Address")
                                {
                                    CurrChilColName = PrevMainPrefix + values[r, c];
                                }
                                else
                                {
                                    CurrChilColName = values[r, c];
                                }
                                PrevColNo = 1;

                            }

                            PrevMainPrefix = getPrefix(PrevMainPrefix, values[r, c]);
                        }

                        //if (PrevMainPrefix != null)
                        //{
                        //    Console.WriteLine("***********r: "+(r+1)+ " ******c: "+(c+1)+" ****************** " + PrevMainPrefix +""+values[r, c]);
                        // }

                        if (c == 0)
                        {
                            if (values[r, c] == "Name/Address")
                            {
                                arrayMT[r, colcount] = CurrChilColName;
                            }
                            else if (values[r, c] == "" || values[r, c] == " ")
                            {
                                arrayMT[r, colcount] = CurrChilColName;
                            }
                            else
                            {
                                arrayMT[r, colcount] = values[r, c];
                                PrevColNo = 0;
                            }
                        }

                        else
                        {
                            arrayMT[r, colcount] = values[r, c];
                        }
                        

                        colcount++;



                    }
                    arrayMT[r, 11] = "1";//status
                    arrayMT[r, 2] = bankrefNo; //RefNo
                                        // Console.Write("");

                }

              //  Console.WriteLine("Loop the array");
                

                for (int k = 0; k < num_rows; k++)
                {
                    //Console.WriteLine("k: " + k + " : " + masterTableValue[k]);
                    for(int m = 0;m< arrayMT.GetUpperBound(1); m++)
                    {
                        
                        //Console.WriteLine("k: " + (k+1) + " m: "+(m+1)+" : " + arrayMT[k, m]);
                        masterTableValue[m] = arrayMT[k, m];
                        
                    }

                    if ((k != 7438) || (k != 7467))
                    { 
                       // Console.WriteLine("k: "+k);
                        InsertCitiRawFile(k,masterTableValue);
                    }
                   // Console.WriteLine(" ");
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ReadFile : " + ex.Message);
            }
            //   Console.WriteLine();
            //   Console.ReadLine();

           

        }

        private static string getPrefix(string PrevMainPrefix,string colValue)
        {
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

        private static string[,] LoadCSVCiti(string filename)
        {

            // Read the csv file
            string csv_file = System.IO.File.ReadAllText(filename);

            // Split into lines.
            csv_file = csv_file.Replace('\n', '\r'); //replace new line with escape character
            string[] lines = csv_file.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries); // split the line with escape character

            // Console.WriteLine("LoadCSV: "+lines.Length);

            //get no of row and columns
            int num_rows = lines.Length;
            int num_cols = 7; //13

            // Create Array for the data.
            string[,] values = new string[num_rows, num_cols];
            try
            {
                // Load the array.
                for (int r = 0; r < num_rows; r++)
                {
                    string[] line_r = lines[r].Split(',');

                    for (int c = 0; c < num_cols; c++)
                    {

                        values[r, c] = line_r[c]; //this giving error

                    }

                }
                  Console.Write("\n\n\nvalues:" +values.Length);
            }
            catch (Exception ex)
            {
                Console.Write("\n ########" + filename + ": Error in LoadCSV :" + ex.Message + "\n");
            }


            // Return the values.
            return values;
        }
        private static void InsertCitiRawFile(int line_no,string[] paramvalue)
        {

            try
            {
                // Console.Write("paramvalue: "+ paramvalue.Length);
                var connectionString = ConfigurationManager.ConnectionStrings["DRConn"].ConnectionString;
                string id = Guid.NewGuid().ToString();
                using (var connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    SqlCommand insertCommand = new SqlCommand("INSERT INTO [dbo].[PCI_Citi_RawFile_Citi] (id,[XlsLineNo],[RefNo] ,[SourceFile] ,[Column1],[Column2] ,[Column3] ,[Column4],[Column5] ,[Column6],[Column7] ,[Status])" + "" +
                    //" VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)", connection);
                    " VALUES ('" + paramvalue[0] +
                    "'," + paramvalue[1] +
                    ",'" + paramvalue[2] +
                    "','" + paramvalue[3] +
                    "','" + paramvalue[4] +
                    "','" + paramvalue[5] +
                    "','" + paramvalue[6] +
                    "','" + paramvalue[7] +
                    "','" + paramvalue[8] +
                    "','" + paramvalue[9] +
                    "','" + paramvalue[10] +
                    "','" + paramvalue[11] +
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
                Console.WriteLine("Error in InsertMasterTable: line_no: "+ (line_no+1));
            }

        }
    }
}
