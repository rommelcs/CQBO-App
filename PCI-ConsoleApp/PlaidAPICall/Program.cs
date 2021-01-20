using System;
using System.Net;

namespace PlaidAPICall
{
	class Program
	{
		static void Main(string[] args)
		{

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://production.plaid.com/identity/get");

			string str = "";
			
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = data.Length;

		}
	}
}
