using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace AMLApp.Controller
{
    public class ArchiveData
    {

        static string connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
        public void processArchiveData()
        {
            try
            {
                // Console.Write("paramvalue: "+ paramvalue.Length);
                //var connectionString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;
                // string id = Guid.NewGuid().ToString();

                using (var connection = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("USP_PCI_AML_Archive_ProcessData", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    SqlDataReader rdr = cmd.ExecuteReader();


                    DataTable tbl = new DataTable();

                    tbl.Load(rdr);
                    connection.Close();

                }
            }
            catch (SqlException e)
            {
                Console.WriteLine("Error in processArchiveData:  " + e.Message);
            }
        }
    }
}
