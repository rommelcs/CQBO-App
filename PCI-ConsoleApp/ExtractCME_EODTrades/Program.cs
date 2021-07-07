using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using ExtractCME_EODTrades.Controller;
using PCI.APIHandler;
using PCI.FileCollector.DAL;
using PCI.Helpers;
using PCI.Helpers.Models;

namespace ExtractCME_EODTrades
{
	class Program
	{
		
		private static string logFolder = ConfigurationManager.AppSettings.Get("LogFolder");
		private static string sftpFileCode = ConfigurationManager.AppSettings.Get("SFTPFileCode");
		private static string localFolder = ConfigurationManager.AppSettings.Get("LocalFolder");
		private static int dateFilesToGet = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CMEDayToProcess"));

		static void Main(string[] args)
		{

			var directory = AppContext.BaseDirectory;
			var log = new LogHelper(logFolder == "" ? directory : logFolder, true);

			try
			{
				log.Log("StartProcess ");
				//Get CQ Numbers
				DownloadCQAPI_EODTradeVolume(log);

				// Download 2 CME EOD Trade zip file for CME and NYMEX
				DownloadCME_EODTrades(log);
								
				// Unzip CME files in preparation for extraction
				UnzipCME_EODTrades(log);

				// Extract EOD and send email of the details
				ExtractCME_EODTrades(log);

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
		}

		static void ExtractCME_EODTrades(LogHelper log)
		{
			CME_EODTrades eodTrades = new CME_EODTrades();
			
			eodTrades.ExtractCME_EODTrades(log);
			

		}

		static void DownloadCQAPI_EODTradeVolume(LogHelper log)
		{
			CME_EODTrades eodTrades = new CME_EODTrades();

			eodTrades.GetCQAPI_EODVolume(log);
		}

		static void DownloadCME_EODTrades(LogHelper log)
		{
			AppDAL appDAL = new AppDAL();
			CME_EODTrades eodTrades = new CME_EODTrades();

			log.Log("Downloading CME EOD Trade file..");
			int downloaded = 0;

			WinSCPHelper scp = new WinSCPHelper();
			// Get the list of items to process
			List<SFTPFileCollection> fileCollection = appDAL.GetSFTPAppSettingsByCode(sftpFileCode); //Get SFTP Credentials from DB

			//checked if file is already available in local folder - means already downloaded
			List<SFTPFileCollection> fileCollectionLeft = checkLocalFile(fileCollection, dateFilesToGet, log);

			if (fileCollectionLeft.Count<=0)
			{
				log.Log("No files to be downloaded.  Files might be already existing in local folder.");
				return;
			}

			string processTime = DateTime.Now.ToString();
			Console.WriteLine("Start processing at {0}", processTime);
			log.Log("Starts - " + processTime);
			while (fileCollectionLeft.Count>0)
			{

				// Set up session options
			    fileCollectionLeft = scp.InitWinSCPSession(fileCollectionLeft, log, dateFilesToGet);

				if (fileCollectionLeft.Count>0)
				{
					string mesg = "Still waiting for "+fileCollectionLeft.Count.ToString() +" file(s) from CME.  Next file check will be at "+ DateTime.Now.AddMilliseconds(300000) + " for the remaining files" ;

					eodTrades.ExtractCME_Heartbit_Email(log, mesg);
					
					Console.WriteLine(mesg);
					Thread.Sleep(60000); //60 seconds

					Console.Clear();
				}
			}
		}

		static List<SFTPFileCollection> checkLocalFile(List<SFTPFileCollection> fileCollection, int toProcessDay, LogHelper log)
		{
			for (int i = 0; i < fileCollection.Count; i++)
			{
				string fileName = fileCollection[i].Filename.ToString().Trim();

				if (fileCollection[i].DatedFilename)
				{
					//Format the filename with the dated values
					DateTime dtNow = DateTime.Now.AddDays(toProcessDay);
					string dateFileName = fileCollection[i].DateFormatInFile.ToString();
					if (dateFileName.ToLower().Contains("yyyy"))
					{
						dateFileName = dateFileName.Replace("YYYY", dtNow.ToString("yyyy"));
					}
					else
					{
						dateFileName = dateFileName.Replace("YY", dtNow.ToString("yy"));
					}

					dateFileName = dateFileName.Replace("mm", dtNow.ToString("MM"));
					dateFileName = dateFileName.Replace("dd", dtNow.ToString("dd"));

					fileName = fileName.Replace("_DATE_", dateFileName);
				}
				string filePath = fileCollection[i].FileLoc.ToString().Trim() + fileName;
				string DestfileFolder =localFolder;//fileCollection[i].DestFilename.ToString().Trim(); // Destination folder
				string localFile = DestfileFolder + fileName;

				if (File.Exists(localFile))
				{
					log.Log(fileName +  " file is already in local folder.");
					fileCollection.RemoveAt(i);
					i--;
				}
			}
			return fileCollection;
		}

		static void UnzipCME_EODTrades(LogHelper log)
		{
			WinSCPHelper winSCP = new WinSCPHelper();
			var directoryPath = new DirectoryInfo(localFolder);

			winSCP.UnzipFiletoFolder(directoryPath, log);
		}
	}
}
