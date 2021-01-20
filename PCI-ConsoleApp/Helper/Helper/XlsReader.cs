using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Helpers
{
	public class XlsReader
	{
        private static DataSet ds;
        public enum FileTypes
		{
			XLSType = 1,
			XLSXType = 2,
			CSVType =3,
			TxtType = 4
		}

		public DataTable ReadXLSToDT(XlsReader.FileTypes fileType, string fileLoc)
		{
            DataTable xlsDT = new DataTable();
            switch (fileType)
			{
				case XlsReader.FileTypes.XLSType:
					xlsDT = ReadXLSFile(fileLoc);
					break;
				default:
					break;
			}

            return xlsDT;
		}

		static DataTable ReadXLSFile(string fileLoc)
		{
			Console.WriteLine(string.Format("processing {0}",fileLoc));

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            //open file and returns as Stream
            using (var stream = File.Open(fileLoc, FileMode.Open, FileAccess.Read))
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

			//Console.WriteLine(string.Format("converted data to DT - {0}", fileLoc));
			//DataColumn colGuid = new DataColumn();
			//colGuid.ColumnName = "Guid";
			//colGuid.DataType = System.Type.GetType("System.Guid");
			//ds.Tables[0].Columns.Add(colGuid);

			//DataColumn colXlsLineNo = new DataColumn();
			//colXlsLineNo.ColumnName = "XlsLineNo";
			//colXlsLineNo.DataType = System.Type.GetType("System.Int");
			//ds.Tables[0].Columns.Add(colXlsLineNo);

			DataTable dt = ds.Tables[0];
            return dt;
		}

	}
}
