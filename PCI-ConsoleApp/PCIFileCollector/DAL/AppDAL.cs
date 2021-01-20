using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using PCI.Helpers.Models;

namespace PCI.FileCollector.DAL
{
	public class AppDAL
	{
		SqlConnection conn = null;
		//private object cmeAPI;
		readonly string connString = ConfigurationManager.ConnectionStrings["PCIBO"].ConnectionString;

		public List<SFTPFileCollection> GetSFTPAppSettings()
		{
			try
			{
				List<SFTPFileCollection> sftpFileCollection = new List<SFTPFileCollection>();

				conn = new SqlConnection(connString);
				SqlCommand cmd = new SqlCommand("USP_App_GetSFTPConfigs", conn);

				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();

				SqlDataReader rdr = cmd.ExecuteReader();
				DataTable tbl = new DataTable();

				tbl.Load(rdr);

				conn.Close();

				for (int i = 0; i < tbl.Rows.Count; i++)
				{
					SFTPFileCollection sftpFile = new SFTPFileCollection();
					sftpFile.Code = tbl.Rows[i]["Code"].ToString();
					sftpFile.ConfigType = tbl.Rows[i]["ConfigType"].ToString();
					sftpFile.Hostname = tbl.Rows[i]["Hostname"].ToString();
					sftpFile.Protocol = tbl.Rows[i]["Protocol"].ToString();
					sftpFile.TCP_Port = Convert.ToInt32(tbl.Rows[i]["TCP_Port"]);
					sftpFile.CredentialNeed = Convert.ToInt32(tbl.Rows[i]["CredentialsNeed"]);
					sftpFile.Host_Key = tbl.Rows[i]["Host_Key"].ToString();
					sftpFile.PrivateKeyPath = tbl.Rows[i]["PrivateKeyPath"].ToString();
					sftpFile.PrivatekeyPassPhrase = tbl.Rows[i]["PrivatekeyPassPhrase"].ToString();
					sftpFile.Username = tbl.Rows[i]["Username"].ToString();
					sftpFile.Password = tbl.Rows[i]["Password"].ToString();
					sftpFile.FileExt = tbl.Rows[i]["FileExt"].ToString();
					sftpFile.Description = tbl.Rows[i]["Description"].ToString();
					sftpFile.FileLoc = tbl.Rows[i]["FileLoc"].ToString();
					sftpFile.Filename = tbl.Rows[i]["Filename"].ToString();
					sftpFile.DestFilename = tbl.Rows[i]["DestFilename"].ToString();
					sftpFile.DatedFilename = Convert.ToBoolean(tbl.Rows[i]["DatedFilename"]==null ? 0 : tbl.Rows[i]["DatedFilename"]);
					sftpFile.DateFormatInFile = tbl.Rows[i]["DateFormatInFile"].ToString();
					sftpFileCollection.Add(sftpFile);
				}

				return sftpFileCollection; 

			}
			catch (Exception ex)
			{
				throw ex;
			}
			
		}

	}
}
