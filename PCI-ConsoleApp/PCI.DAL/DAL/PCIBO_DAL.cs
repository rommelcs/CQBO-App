using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;

namespace PCI.DAL.DAL
{
	class PCIBO_DAL
	{
		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;

		public static object ConfigurationManager { get; private set; }

		
	}
}
