using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PCI_BankTransApp.DAL
{
	class CQBOAPI
	{
		static HttpClient client = new HttpClient();

		public class UserCredentials {
			public string username { get; set; }
			public string password { get; set; }
		}

		public class RootObject //This is the main object
		{

			public RootObject(string json)
			{
				JObject jObject = JObject.Parse(json);

				code = (string)jObject["code"];
				message = (string)jObject["message"];
				// data = jUser["data"].ToList();
			}

			public string code { get; set; }

			public string message { get; set; }

		}

		public class Datum
		{

			public Datum(string json)
			{
				JObject jObject = JObject.Parse(json);
				JToken jUser = jObject["data"];
				userId = (string)jUser["userId"];
				token = (string)jUser["token"];
			}
			public string userId { get; set; }
			public string token { get; set; }

		}
		public void RunMe()
		{
			UserCredentials credentials = new UserCredentials();

			string Username = "mhelcor";
			string Password = "CQapi@1234";


			try
			{
				LoginAPI(Username, Password).GetAwaiter();
			}
			catch (Exception e)
			{

				throw e;
			}
			
		}

		//public async Task RunAsync()
		//{
		//	client.BaseAddress = new Uri("https://bkp.cqfutures.com/CQAPI/");
		//	client.DefaultRequestHeaders.Accept.Clear();
		//	client.DefaultRequestHeaders.Accept.Add(
		//		new MediaTypeWithQualityHeaderValue("application/json"));

		//	try
		//	{

		//	}
		//	catch (Exception e)
		//	{

		//		throw e;
		//	}

		//}

		public static async Task LoginAPI(string username, string password)
		{

			
			var clientHandler = new HttpClientHandler
			{
				AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
				AllowAutoRedirect = false
			};

			using (var client = new HttpClient(clientHandler, true))
			{
				client.BaseAddress = new Uri("https://bkp.cqfutures.com/CQAPI/api/Authorize");
				client.DefaultRequestHeaders.ExpectContinue = false;

				var serializedObject = JsonConvert.SerializeObject(
					new { username= username, password = password }
					);

				var request = new HttpRequestMessage(HttpMethod.Post, "Authorize")
				{
					Content = new StringContent(serializedObject, Encoding.UTF8, "application/json")
				};

				var response = await client.SendAsync(request);

				response.EnsureSuccessStatusCode();

				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();

					
					Console.WriteLine("test");
				}

			}


			//client.BaseAddress = new Uri("https://bkp.cqfutures.com/CQAPI/");
			//client.DefaultRequestHeaders.Accept.Clear();
			//client.DefaultRequestHeaders.Accept.Add(
			//	new MediaTypeWithQualityHeaderValue("application/json"));

			//UserCredentials credentials = new UserCredentials();
			//credentials.userName = "mhelcor";
			//credentials.userPassword = "CQapi@1234";

			//HttpClient client = new HttpClient();
			//client.BaseAddress = new Uri("https://bkp.cqfutures.com/CQAPI");
			//client.DefaultRequestHeaders.Accept.Clear();
			//client.DefaultRequestHeaders.Accept.Add(
			//	new MediaTypeWithQualityHeaderValue("application/json"));
			
		 
			//var content = new StringContent(JsonConvert.SerializeObject(credentials), Encoding.UTF8, "application/json");
			//try
			//{
			//	Console.WriteLine(client.BaseAddress.ToString());
			//	 HttpResponseMessage res = await client.PostAsJsonAsync( "dsfgsdfgapi/Authorize",credentials);

			//	if (res.IsSuccessStatusCode)
			//	{
			//		// Get the URI of the created resource.
			//		Console.WriteLine(res.IsSuccessStatusCode.ToString());


			//		// do whatever you need to do here with the returned data //


			//	}
			//	//HttpResponseMessage response = await client.PostAsync("/api/Authorize", content).ConfigureAwait(false);
			//	//if (response.IsSuccessStatusCode)
			//	//{
			//	//	Console.WriteLine("Data posted");
			//	//}
			//	//else
			//	//{
			//	//	Console.WriteLine($"Failed to poste data. Status code:{response.StatusCode}");
			//	//}
			//}
			//catch (Exception e)
			//{

			//	throw e;
			//}
			


		}

	}
}
