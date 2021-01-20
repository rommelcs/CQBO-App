using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PCI_BankTransApp.DAL;
using System.Linq;
using static PCI_BankTransApp.DAL.CQBOAPI;
using PCI_BankTransApp.Models;
using static PCI_BankTransApp.Models.CQBOAPIModel;
using PCI_BankTransApp.Controller;

namespace PCI_BankTransApp
{
	class Program
	{

		static String str = String.Empty;
		static CQAPIResultData ValidUser = new CQAPIResultData();

		static void Main(string[] args)
		{
			//string apiToken = string.Empty;

			CQBOAPI cqApi = new CQBOAPI();

			

			//string Username = "mhelcor";
			//string Password = "CQapi@1234";
			//cqApi.RunMe();

			UserCredentials credential = new UserCredentials();
			credential.username = "mhelcor";
			credential.password = "CQapi@1234";

			LoginAPIUser(credential).Wait();

			getCountryList(credential.username, str).Wait();

			//getRegionList(ValidUser, "US", "").Wait();



			//Console.WriteLine("Below is the token");
			//Console.WriteLine(str);


		}

		static async Task LoginAPIUser(UserCredentials credential)
		{
			string retVal = string.Empty;

			
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("https://bkp.cqfutures.com/CQAPI/api/Authorize");
				client.DefaultRequestHeaders.ExpectContinue = false;

				var serializedObject = JsonConvert.SerializeObject(
					credential
					// new { username = username, password = password }
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

					CQBOAPIModel.CQAPIResult result = JsonConvert.DeserializeObject<CQBOAPIModel.CQAPIResult>(content);

					if (result.code==0)
					{
						retVal = result.data.token;
						str = retVal;
						ValidUser.userId = result.data.userId;
						ValidUser.token = result.data.token;
					}
				}

			}
		}

		static async Task getCountryList(string userid, string token)
		{
			try
			{
				String uriAPI = "https://bkp.cqfutures.com/CQAPI/api/CQBO/country/GetCountryList";

				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(uriAPI);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Add($"Authorization", $"Bearer " + token);
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					HttpResponseMessage response = await client.GetAsync(uriAPI+ "?userId="+ ValidUser.userId+"&cd_type=CNTR&PageSize=500&PageNo=1");

					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync();
						Console.WriteLine("Country List");
						CQAPICountryList countryRes = JsonConvert.DeserializeObject<CQAPICountryList>(content);

						PCIBO_DAO dao = new PCIBO_DAO();
						

						dao.GetBankTrans(countryRes);
					}
				}
			} 
			catch (Exception e)
			{

				throw e;
			}
		}

		public static async Task getRegionList(CQAPIResultData ValidUser, string ctryCode, string regionCD)
		{
			try
			{
				String uriAPI = "https://bkp.cqfutures.com/CQAPI/api/CQBO/region/GetRegionList";

				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(uriAPI);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Add($"Authorization", $"Bearer " + ValidUser.token);
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					HttpResponseMessage response = await client.GetAsync(uriAPI + "?userId=" + ValidUser.userId + "&Form_Code=DIVDCD&ref_codeID=" + ctryCode);

					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync();
						//Console.WriteLine("Region List");
						CQAPIRegionList regionRes = JsonConvert.DeserializeObject<CQAPIRegionList>(content);


						//Console.WriteLine("regionRes totalRecord: " + regionRes.totalRecord);
						//Console.WriteLine("regionRes.data count: " + regionRes.data.Count);
						//foreach(var item in regionRes.data)
						//{
						//    Console.WriteLine(item.form_CodeID + " || " + item.form_CodeDescription);

						//}


						ValidateRegion region = new ValidateRegion();
						region.SearchRegion(regionRes, regionCD);
					}
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.Message);
				Console.ResetColor();
				throw ex;
			}
		}
	}
}
