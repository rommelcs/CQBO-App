using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Configuration;
using CSVFileReader.Controller;

namespace CSVFileReader
{

    //Author:SHubana
    //Date : 
    //Desc: Read csv file and insert into master and detail table
    //-------------------------------------------------------------
    //mod by: shubana
    //Date:20/7/2020
    //Desc : Transform master and detail into pivot table  and insert into tb_PCI_BankTrans table
    //-------------------------------------------------------------
    class Program
    {
        
        static void Main(string[] args)
        {
            string sampleCSV = "";
            int filecount = 0;

            //get folder path
            Console.WriteLine("****************Menu Selection :*************************");
            Console.WriteLine("Select Your Menu\n 1 - Load \n 2 - Transform Data");

            Console.WriteLine("**************************************************");
            string menu = Console.ReadLine();
            Console.WriteLine("**************************************************");
            if (menu == "1")
            {
                loadFIle();
            }
            else if(menu == "2")
            {
                TransformData trans = new TransformData();
                trans.GetBankTrans();

            }
           

        //   Console.WriteLine();

            Console.ReadLine();

        }

        private static void loadFIle()
        {
            string sampleCSV = "";
            int filecount = 0;

            //get folder path
            Console.WriteLine("Input file path");
            string path = Console.ReadLine();

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
                        ReadFile(sampleCSV);
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


            private static void ReadFile(string filename)
        {

            try
            {
                string[,] values = LoadCSV(filename);
                int num_rows = values.GetUpperBound(0) + 1;
                int num_cols = values.GetUpperBound(1) + 1;

                //Display the data to show we have it.
                // Console.Write("num_cols: " + num_cols + "\n");
                //Console.Write("no of rows: " + num_rows + "\n");
                Console.Write("");
                Console.WriteLine("*************Processing :  "+ filename+"*************");
                Console.Write("");

                string[] masterTableValue = new string[num_cols + 3];
                string strFilename = Path.GetFileName(filename);
                int arraycount = 0;
                string detail_id = "";
                string strColM = "";
                masterTableValue[arraycount] = strFilename;

                //read header or first row
                for (int c = 0; c < num_cols; c++)
                {
                    //Console.Write(values[0, c] + "\t");
                    if (c == 12)
                    {
                        strColM = "";

                        strColM = values[0, c];
                    }
                    arraycount++;
                    masterTableValue[arraycount] = values[0, c]; //a
                }
                arraycount++;
                // Console.WriteLine("arraycount: " + arraycount +":"+ strFilename + "_" + r);
                masterTableValue[arraycount] = detail_id;

                arraycount++;
                masterTableValue[arraycount] = DateTime.Now.ToString();
                if (masterTableValue[13] != null)
                {

                    InsertMasterTable(masterTableValue);
                    SplitDetailsColumn(detail_id, strColM);
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

                        // Console.Write(values[r, c] + "\t");
                        //if (c < 12)
                        //{

                        //}
                        //else if (c == 12)
                        //{
                        //    SplitDetailsColumn(values[r, c]);
                        //}

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

                    InsertMasterTable(masterTableValue);
                    SplitDetailsColumn(detail_id, strColM);


                }






            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in ReadFile : "+ ex.Message);
            }
         //   Console.WriteLine();
         //   Console.ReadLine();

        }

        private static void FirstRow(int num_rows, int num_cols)
        {

            
        }

            private static string[,] LoadCSV(string filename)
        {
            
                // Read the csv file
                string csv_file = System.IO.File.ReadAllText(filename);

                // Split into lines.
                csv_file = csv_file.Replace('\n', '\r'); //replace new line with escape character
                string[] lines = csv_file.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries); // split the line with escape character

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
                    string[] line_r = lines[r].Split(',');
                    
                        for (int c = 0; c < num_cols; c++)
                        {
                          
                            values[r, c] = line_r[c]; //this giving error

                        }
                    
                }
              //  Console.Write("values:" +values.Length);
            }
            catch(Exception ex)
            {
                Console.Write("\n ########"+ filename + ": Error in LoadCSV :" +ex.Message+"\n");
            }
            

            // Return the values.
            return values;
        }

        private static void SplitDetailsColumn(string id, string value)
        {
            try
            {
              //  Console.WriteLine();
              //  Console.Write("\n********************************\n");
              //  Console.Write("Hey I gonna Split You :  \nvalue: " + value);

                string result = Regex.Replace(value, @".{3}(?:=)", @"#$&"); // to find XXX=
                //Console.WriteLine("\nresult: " + result);

                string[] tryw  = result.Split("#", StringSplitOptions.RemoveEmptyEntries);
               // Console.Write("\n********************SPILT********************************\n");
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

                       // Console.Write("\n" + splitvalue[0] + "\t" + splitvalue[1]);
                        InsertDetailTable(id, splitvalue[0], splitvalue[1]);

                    }

                }

                string subjectString = @value;

               // Console.Write("\n********************************\n");



            }
            catch (Exception ex)
            {
                Console.Write("\n Error in SplitDetailsColumn "+ ex.Message + " \n" );
            }
        }
        private static void InsertMasterTable(string[] paramvalue)
        {
           
            try
            {
               // Console.Write("paramvalue: "+ paramvalue.Length);
                var connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    SqlCommand insertCommand = new SqlCommand("INSERT INTO [dbo].[PCI_BMOHarris] ([FileName],[Col_A],[Col_B],[Col_C],[Col_D],[Col_E],[Col_F],[Col_G],[Col_H] ,[Col_I],[Col_J],[Col_K],[Col_L],[Col_M],[Detail_ID],[FileUploadTime])" + "" +
                        " VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15)", connection);

                    insertCommand.Parameters.Add(new SqlParameter("0", paramvalue[0]));
                    insertCommand.Parameters.Add(new SqlParameter("1", paramvalue[1]));
                    insertCommand.Parameters.Add(new SqlParameter("2", paramvalue[2]));
                    insertCommand.Parameters.Add(new SqlParameter("3", paramvalue[3]));
                    insertCommand.Parameters.Add(new SqlParameter("4", paramvalue[4]));
                    insertCommand.Parameters.Add(new SqlParameter("5", paramvalue[5]));
                    insertCommand.Parameters.Add(new SqlParameter("6", paramvalue[6]));
                    insertCommand.Parameters.Add(new SqlParameter("7", paramvalue[7]));
                    insertCommand.Parameters.Add(new SqlParameter("8", paramvalue[8]));
                    insertCommand.Parameters.Add(new SqlParameter("9", paramvalue[9]));
                    insertCommand.Parameters.Add(new SqlParameter("10", paramvalue[10]));
                    insertCommand.Parameters.Add(new SqlParameter("11", paramvalue[11]));
                    insertCommand.Parameters.Add(new SqlParameter("12", paramvalue[12]));
                    insertCommand.Parameters.Add(new SqlParameter("13", paramvalue[13])); //col_m
                    insertCommand.Parameters.Add(new SqlParameter("14", paramvalue[14])); //details_id
                    insertCommand.Parameters.Add(new SqlParameter("15",DateTime.Now)); //timestamp


                    insertCommand.ExecuteNonQuery();

                }
            }
                catch (SqlException e)
                {
                    Console.WriteLine("Error in InsertMasterTable: " + e.ToString());
                }

            }

        private static void InsertDetailTable(string id, string fieldname,string fieldvalue)
        {
            try
            {
               
                var connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand insertCommand = new SqlCommand("INSERT INTO [dbo].[PCI_BMOHarris_Details] ([ID],[Field_Name],[Field_Value]) VALUES (@0, @1, @2)", connection);



                    insertCommand.Parameters.Add(new SqlParameter("0", id));
                    insertCommand.Parameters.Add(new SqlParameter("1", fieldname));
                    insertCommand.Parameters.Add(new SqlParameter("2", fieldvalue));
                    insertCommand.ExecuteNonQuery();

                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

        }


    }
}
