using System;
using System.Collections.Generic;
using System.Text;
using WinSCP;
using PCI.Helpers.Models;
using System.Linq;
using System.IO;

namespace PCI.Helpers
{
	public class WinSCPHelper
	{

        public int WinSCPSession(List<SFTPFileCollection> sftpCollection, LogHelper log, int toProcessDay)
        {
            try
            {
                var exchCollection = sftpCollection.GroupBy(u => u.Code).ToList();
                

                foreach (var exch in exchCollection)
                {
                    string code = exch.FirstOrDefault().Code;
                    var fileCollection = sftpCollection.Where(u => u.Code==code).ToList();
                    Console.WriteLine("Processing.. " + code.ToString());
                    log.Log("Processing.. " + code.ToString());



                    if (exch.FirstOrDefault().PrivateKeyPath.ToString() != string.Empty)
                    {
                        SessionOptions sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = exch.FirstOrDefault().Hostname,
                            UserName = exch.FirstOrDefault().Username.ToString().TrimEnd(),
                            Password = exch.FirstOrDefault().Password.ToString().TrimEnd(),
                            PortNumber = Convert.ToInt32(exch.FirstOrDefault().TCP_Port),
                            SshHostKeyFingerprint = exch.FirstOrDefault().Host_Key.ToString().TrimEnd(), 
                            SshPrivateKeyPath = exch.FirstOrDefault().PrivateKeyPath.ToString().TrimEnd(),
                            PrivateKeyPassphrase = exch.FirstOrDefault().PrivatekeyPassPhrase.ToString().TrimEnd() 

                        };

                        CreateSFTPSession(sessionOptions, fileCollection, log, toProcessDay);
                    }
                    else
                    {
                        SessionOptions sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = exch.FirstOrDefault().Hostname,
                            PortNumber = Convert.ToInt32(exch.FirstOrDefault().TCP_Port),
                            UserName = exch.FirstOrDefault().Username.ToString().TrimEnd(),
                            Password = exch.FirstOrDefault().Password.ToString().TrimEnd(),
                            SshHostKeyFingerprint = exch.FirstOrDefault().Host_Key.ToString().TrimEnd() // "ssh-rsa 2048 jnrVgUsNrZ+VmunhM0XrnWnlc2gVhSBOK+jFx5vQoMc="
                        };
                        CreateSFTPSession(sessionOptions, fileCollection, log, toProcessDay);
                    }

                }

                return 0;
            }
            catch (Exception ex)
            {
                //throw ex;
                log.Log("Exception occured");
                log.Log(ex.Message);
                log.Log("Source: " + ex.Source);
                log.Log("Inner exception: " + ex.InnerException);
                log.Log("Stack trace: " + ex.StackTrace);
                return -1;

            }
        }

        static void CreateSFTPSession(SessionOptions sessionOptions, List<SFTPFileCollection> fileCollection, LogHelper log, int toProcessDay)
        {
            try
            {
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

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
                        string DestfileFolder = fileCollection[i].DestFilename.ToString().Trim() ; // Destination folder
                        TransferOperationResult transferResult;
                        transferResult = session.GetFiles(filePath, Path.GetFullPath(DestfileFolder), false, transferOptions);
                        if (!transferResult.IsSuccess)
                        {
                            Console.WriteLine(string.Format("{0} - File not exist",filePath));
                            log.Log(string.Format("{0} - File not exist", filePath));
                        }
                        else
                        {
                            // Throw on any error
                            transferResult.Check();

                            // Print results
                            foreach (TransferEventArgs transfer in transferResult.Transfers)
                            {
                                Console.WriteLine("Download of {0} succeeded", transfer.FileName);
                                log.Log(string.Format("Download of {0} succeeded", transfer.FileName));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log("Exception occured");
                log.Log(ex.Message);
                log.Log("Source: " + ex.Source);
                log.Log("Inner exception: " + ex.InnerException);
                log.Log("Stack trace: " + ex.StackTrace);
                return;
            }
        }

	}
}
