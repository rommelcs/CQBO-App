using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using ExtractCME_EODTrades.DAL;
using PCI.APIHandler;
using PCI.Helpers;
using SterlingApp.Controller;
using static PCI.APIHandler.Models.CQModels;

namespace ExtractCME_EODTrades.Controller
{
	public class CME_EODTrades
	{
		public void ExtractCME_EODTrades(LogHelper log)
		{
			DataTable dt = new DataTable();
			int CMEToProcess = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CMEDayToProcess"));
			DateTime dayToProcess = DateTime.Now.AddDays(CMEToProcess);
			string tradeDate = dayToProcess.Year.ToString() + dayToProcess.Month.ToString("00") + dayToProcess.Day.ToString("00");

			Console.WriteLine("Extract CME");
			PCIBO_DAL _PCIBOdal = new PCIBO_DAL();

			_PCIBOdal.InsertCME_EODFIXML(log, tradeDate);
			dt = _PCIBOdal.GetCME_EODTrades(log, tradeDate);


			log.Log("Preparing email of extracted data..");

			EmailSettings emailSettings = new EmailSettings();
			emailSettings.SmtpHost = System.Configuration.ConfigurationManager.AppSettings.Get("SmtpHost");  //"smtp.office365.com";
			emailSettings.SmtpPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("SmtpPort"));
			emailSettings.SmtpUser = ConfigurationManager.AppSettings.Get("SmtpUser");
			emailSettings.SmtpPassword = ConfigurationManager.AppSettings.Get("SmtpPassword");
			emailSettings.DefaultSender = ConfigurationManager.AppSettings.Get("DefaultSender");
			emailSettings.EnableSSL = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("EnableSSL"));

			EmailHelper email = new EmailHelper(emailSettings);
			string emailTo = ConfigurationManager.AppSettings.Get("CMEEmailTo");
			string emailSubject = "Daily CME Trades Volume Compare - " + dayToProcess.ToString("MMM dd, yyyy");
			StringBuilder emailBody = new StringBuilder();
			StringBuilder sb = new StringBuilder();
			emailBody.Append("<html><head><style type='text/css'>table, th, td { border: 1px solid black; }</style></head><body>{body}</body></html>");

			sb.Append("<h3>The following are the daily volume comparison between extracted CME EOD FIX file and CQ for " + dayToProcess.ToString("MMM dd, yyyy") + "</h3>");
			sb.Append("<table><thead><tr><th>Exchange</th><th>Exch. Volume</th><th>CQ Volume</th><th>Difference</th></tr></thead><tbody>");

			for (int i = 0; i < dt.Rows.Count; i++)
			{
				sb.Append(string.Format("<tr><td>{0}", dt.Rows[i]["ExchCode"].ToString()));
				sb.Append(string.Format("</td><td>{0:#,##0}", Convert.ToDecimal(dt.Rows[i]["Vol"].ToString())));
				sb.Append(string.Format("</td><td>{0:#,##0}", Convert.ToDecimal(dt.Rows[i]["CQVol"].ToString())));
				if (Convert.ToInt32(dt.Rows[i]["Diff"])!=0)
				{
					sb.Append(string.Format("</td><td style='background-color:yellow; color:red'><b>{0:#,##0}</b></td></tr>", Convert.ToDecimal(dt.Rows[i]["Diff"].ToString())));
				}
				else
				{
					sb.Append(string.Format("</td><td >{0:#,##0}</td></tr>", Convert.ToDecimal(dt.Rows[i]["Diff"].ToString())));
				}

				//Console.WriteLine(dt.Rows[i]["ExchCode"].ToString());
				//Console.WriteLine(dt.Rows[i]["Vol"].ToString());
			}
			sb.Append("</tbody></table>");
			sb.Append("<p><b>NOTE on above numbers:</b> </p>");
			sb.Append("<p style:'margin-left:10px'>1.	Exclude Options Expiration Resulting Futures</p>");
			sb.Append("<p style:'margin-left:10px'>2.	MOS Trades ( Included only top day MOS trades and Excluded the T-1 reversals from count )</p>");

			sb.Append("<p style:'margin-left:10px'><b>***This is an auto-generated email***</b></p>");
			
			email.Send(emailTo, emailSubject, emailBody.ToString().Replace("{body}", sb.ToString()), emailSettings.SmtpUser);

			log.Log("Process Done.....Email sent.");
		}

		public void ExtractCME_Heartbit_Email(LogHelper log, string mesg)
		{
			DataTable dt = new DataTable();
			int CMEToProcess = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CMEDayToProcess"));
			DateTime dayToProcess = DateTime.Now.AddDays(CMEToProcess);

			log.Log("Preparing email of application heartbeat..");

			EmailSettings emailSettings = new EmailSettings();
			emailSettings.SmtpHost = System.Configuration.ConfigurationManager.AppSettings.Get("SmtpHost");  //"smtp.office365.com";
			emailSettings.SmtpPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings.Get("SmtpPort"));
			emailSettings.SmtpUser = ConfigurationManager.AppSettings.Get("SmtpUser");
			emailSettings.SmtpPassword = ConfigurationManager.AppSettings.Get("SmtpPassword");
			emailSettings.DefaultSender = ConfigurationManager.AppSettings.Get("DefaultSender");
			emailSettings.EnableSSL = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("EnableSSL"));

			EmailHelper email = new EmailHelper(emailSettings);
			string emailTo = ConfigurationManager.AppSettings.Get("CMEEmailTo_HB");
			string emailSubject = "CME GROUP EOD FIXML Trades- " + dayToProcess.ToString("MMM dd, yyyy") + " - Application Check";
			StringBuilder emailBody = new StringBuilder();
			StringBuilder sb = new StringBuilder();
			emailBody.Append("<html><head><style type='text/css'>table, th, td { border: 1px solid black; }</style></head><body>{body}</body></html>");

			sb.Append("<h3> "+mesg.ToString()+" </h3>");
			
			email.Send(emailTo, emailSubject, emailBody.ToString().Replace("{body}", sb.ToString()), emailSettings.SmtpUser);

			log.Log("Application Heartbeat.....Email sent.");
		}

		public void GetCQAPI_EODVolume(LogHelper log)
		{

			int CMEToProcess = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CMEDayToProcess"));
			string APIVolMode = ConfigurationManager.AppSettings.Get("APIVolumeMode").ToString();
			DateTime dayToProcess = DateTime.Now.AddDays(CMEToProcess);
			string tradeDate = dayToProcess.Year.ToString() + dayToProcess.Month.ToString("00") + dayToProcess.Day.ToString("00");

			CQAPICMEVolumeListResult CMEVolListRes = new CQAPICMEVolumeListResult();

			CQLoginCredentials cqLoginCredentials = new CQLoginCredentials();
			CQLoginResult CQLoginRes = new CQLoginResult();
			CQAPIHandler cqHandler = new CQAPIHandler();

			string CQAPIuri = "https://bkp.cqfutures.com/CQAPI/"; //_configuration.GetValue<string>("App:CQAPISettings:CQAPIuri");
			string APIuserKey = "cm9tbWVsQ1FCT1BDSUFQSQ==";//_configuration.GetValue<string>("App:CQAPISettings:UserKey");
														   //int pageSize = "";// Convert.ToInt32(_configuration.GetValue<string>("App:CQAPISettings:CQAPIpageSize"));
			cqLoginCredentials.username = "rommel";//_configuration.GetValue<string>("App:CQAPISettings:CQAPIid");
			cqLoginCredentials.password = "kayr0418@PC";//_configuration.GetValue<string>("App:CQAPISettings:CQAPIpw");

			string date = DateTime.Now.ToLongDateString();
			
			//
			CQLoginRes = cqHandler.cqAPILogin(CQAPIuri, cqLoginCredentials);
					   
			// Get CQ API function for CQ Comparison
			CQAPIHandler cqAPI = new CQAPIHandler();


			string userId = CQLoginRes.data.userId.ToString();
			string Mode = APIVolMode;

			string EODTradeVolByExchangeParams = "api/CQBO/EOD/GetCMETradeByExchangeList?userId=" + userId + "&mode=" + Mode + "&TradeDate=" + tradeDate;

			//cqAPI.GetCQVolumeOfCME(CQAPIuri,, CQLoginRes.data.token.ToString());
			CMEVolListRes = cqAPI.GetCQVolumeOfCME(CQAPIuri, EODTradeVolByExchangeParams, CQLoginRes.data.token.ToString());

			// Update PCI DB for values
			PCIBO_DAL pciboDal = new PCIBO_DAL();
			DataTable dtable = new DataTable();

			dtable.Columns.Add("ExchCode", typeof(String));
			dtable.Columns.Add("Volume", typeof(Int32));

			
			foreach (var item in CMEVolListRes.data)
			{
				DataRow dr = dtable.NewRow();
				dr["ExchCode"] = item.exch_Cd.ToString() == "CBOT" ? "CBT" : item.exch_Cd.ToString();
				dr["Volume"] = item.allQty;

				dtable.Rows.Add(dr);
			}

			pciboDal.UpdateCQVol_DB(log, dtable);
		}
	}
}
