using System;
using Microsoft.SharePoint.Client;


namespace SecuritiesReports
{
	class Program
	{
		static void Main(string[] args)
		{
            string siteUrl = "https://phillipcapitalinc.sharepoint.com/sites/ITDevelopment/Shared%20Documents/Forms/AllItems.aspx?viewid=6450d676%2D8196%2D4de6%2Dba18%2Dfa5d0d5248ae";

            ClientContext clientContext = new ClientContext(siteUrl);

            //ClientContext context = new ClientContext("https://mycompany.sharepoint.com/sites/parts/");
            var credentials = new SharePointOnlineCredentials('rcortezano@phillipcapital.com', 'kayr0418@PC01');


            clientContext.Credentials = credentials;

            Web site = clientContext.Web;
            FolderCollection collFolder = site.Folders;
            clientContext.Load(collFolder);
            clientContext.ExecuteQuery();

            Console.WriteLine("The current site contains the following folders:\n\n");
            foreach (Folder myFolder in collFolder)
                Console.WriteLine(myFolder.Name);
        }
	}
}
