using System;
using System.Configuration;
using System.IO;
using AMLApp.Controller;
using AMLApp.DAL;

namespace AMLApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string argVal = string.Empty;

            

            if (args.Length == 0)
            {
                Help();
            }
            else
            {
                argVal = args[0];

                switch (argVal.ToLower())
                {
                    case "help":
                        Help();
                        break;
                    case "-p":
                        ProcessBankTrans();
                        break;
                    case "-a":
                        ProcessAMLAggregateReport();
                        break;
                    default:
                        break;
                }

                Console.WriteLine(argVal);
            }
        }

        static void ProcessAMLAggregateReport() 
        {
            AMLAggregateReport aReport = new AMLAggregateReport();

            aReport.GenerateAMLAggregatedReport();
        }
        static void ProcessBankTrans()
        {
            processCiti citi = new processCiti();
            ProcessBMO bmo = new ProcessBMO();
            PCIBO_DAL pci_dal = new PCIBO_DAL();
            ArchiveData archive = new ArchiveData();

            string fileDate = ConfigurationManager.AppSettings.Get("SpecificDate");
            int fileToProcess = Convert.ToInt32(ConfigurationManager.AppSettings.Get("DayToProcess"));

            if (fileDate == "")
            {
                fileDate = "YYYYmmdd";
                DateTime dtNow = DateTime.Now.AddDays(fileToProcess);

                fileDate = fileDate.Replace("YYYY", dtNow.ToString("yyyy"));
                fileDate = fileDate.Replace("mm", dtNow.ToString("MM"));
                fileDate = fileDate.Replace("dd", dtNow.ToString("dd"));
            }

            archive.processArchiveData();

            citi.processCitiFiles(fileDate); //From Excel to Raw Data in Table [PCIBO].[dbo].[PCI_Citi_RawFile] --USP_PCI_Citi_RawFile_Insert

            citi.processCitiData(); //Insert the RawData into [PCIBO].[dbo].[PCI_BankTrans] table

            bmo.processBMOFiles(fileDate);

            bmo.GetBankTrans();

            citi.IdentifyCitiTransCountry();

            pci_dal.GetAMLReport();
        }

        static void Help()
        {
            Console.WriteLine("Application parameter is missing.  Please pass a parameter value for the following options:");
            Console.WriteLine("-p    Process and Extract Bank Tranactions Data");
            Console.WriteLine("-a    AML Aggregated Deposit Report");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Sample usage: AMLAPP [-p] | [-a]");

            Console.Read();
        }
    }
}
