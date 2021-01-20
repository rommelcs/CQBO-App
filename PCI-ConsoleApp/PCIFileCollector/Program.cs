using System;
using System.Configuration;
using System.IO;
using PCI.Helpers;
using PCI.Helpers.Models;
using PCI.FileCollector.DAL;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;

namespace PCIFileCollector
{
	class Program
	{
		private static string logFolder = ConfigurationManager.AppSettings.Get("LogFolder");
		private static int	toProcessFile = Convert.ToInt32(ConfigurationManager.AppSettings.Get("ToGetFilesOf"));

		static void Main(string[] args)
		{
			var directory = AppContext.BaseDirectory;
			
			
			var log = new LogHelper(logFolder == "" ? directory : logFolder, true);

			try
			{
				log.Log("StartProcess");

				CollectSFTPFiles(log);

				log.Log("EndProcess");
			}
			catch (Exception ex)
			{

				log.Log("Exception occured");
				log.Log(ex.Message);
				log.Log("Source: " + ex.Source);
				log.Log("Inner exception: " + ex.InnerException);
				log.Log("Stack trace: " + ex.StackTrace);
				throw ex;
			}

			Console.WriteLine(logFolder);
		}

		//Collect Daily Files from different SFTP sources and Copy to centralized location in PCI
		static void CollectSFTPFiles(LogHelper log) 
		{
			AppDAL appDAL = new AppDAL();
			WinSCPHelper scp = new WinSCPHelper();

			// Get the list of items to process
			List<SFTPFileCollection> fileCollection = appDAL.GetSFTPAppSettings(); //Get SFTP Credentials from DB

			// Set up session options
			scp.WinSCPSession(fileCollection, log, toProcessFile);

		}
	}
}
