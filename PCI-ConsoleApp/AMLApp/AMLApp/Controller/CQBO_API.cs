using AMLApp.DAL;
using AMLApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static AMLApp.Models.CQBOAPIModels;

namespace AMLApp.Controller
{
	public class CQBO_API
	{

		public class UserCredentials
		{
			public string username { get; set; }
			public string password { get; set; }
		}

		static String str = String.Empty;
		static CQAPIResultData ValidUser = new CQAPIResultData();
		public static CQAPIRegionList USRegionList = new CQAPIRegionList();
		public static async Task LoginAPIUser(UserCredentials credential)
		{
			string retVal = string.Empty;
			//CQAPIResultData ValidUser = new CQAPIResultData();

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

					CQBOAPIModels.CQAPIResult result = JsonConvert.DeserializeObject<CQBOAPIModels.CQAPIResult>(content);

					if (result.code == 0)
					{
						retVal = result.data.token;
						str = retVal;
						ValidUser.userId = result.data.userId;
						ValidUser.token = result.data.token;
					}

				}
			}
		}

		public static CQAPICountryListData IndetifyCitiTransCountry()
		{
			CQAPICountryListData resultData = new CQAPICountryListData();
			
			CQBO_API.UserCredentials credential = new CQBO_API.UserCredentials();
			credential.username = "mhelcor";
			credential.password = "CQapi@1234";

			LoginAPIUser(credential).Wait();

			if (ValidUser.userId != 0  && ValidUser.token != "")
			{
				getCountryList(ValidUser.userId, ValidUser.token).Wait();
			}

			return resultData;
			
		}

		public static async Task getRegionList(int userId, string token)
		{
			try
			{
				String uriAPI = "https://bkp.cqfutures.com/CQAPI/api/CQBO/region/GetRegionList";

				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(uriAPI);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Add($"Authorization", $"Bearer " + token);
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					HttpResponseMessage response = await client.GetAsync(uriAPI + "?userId=" + userId + "&Form_Code=DIVDCD&ref_codeID=US");

					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync();
						USRegionList = JsonConvert.DeserializeObject<CQAPIRegionList>(content);
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
		static async Task getCountryList(int userId, string token)
		{

			getRegionList(userId, token).Wait();

			try
			{
				String uriAPI = "https://bkp.cqfutures.com/CQAPI/api/CQBO/country/GetCountryList";

				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(uriAPI);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Add($"Authorization", $"Bearer " + token);
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					HttpResponseMessage response = await client.GetAsync(uriAPI + "?userId=" + userId + "&cd_type=CNTR&PageSize=500&PageNo=1");

					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync();
						Console.WriteLine("Country List");
						CQAPICountryList countryRes = JsonConvert.DeserializeObject<CQAPICountryList>(content);

						PCIBO_DAL dao = new PCIBO_DAL();

						dao.GetBankTrans(countryRes, USRegionList);
					}
				}
			}
			catch (Exception e)
			{

				throw e;
			}
		}

		static async Task getCQInfoClientName(int userId, string token, string cqAccount)
		{
			try
			{
				String uriAPI = "https://bkp.cqfutures.com/CQAPI/api/CQBO/client/GetInfoClientName";

				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(uriAPI);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Add($"Authorization", $"Bearer " + token);
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					//HttpResponseMessage response = await client.GetAsync(uriAPI + "?userId=" + userId + "&cd_type=CNTR&PageSize=500&PageNo=1");
					HttpResponseMessage response = await client.GetAsync(uriAPI + "?userId=" + userId + "&AccountStatus=Acive&AccountNo=" + cqAccount);

					if (response.IsSuccessStatusCode)
					{
						var content = await response.Content.ReadAsStringAsync();
						Console.WriteLine("Country List");
						CQAPICountryList countryRes = JsonConvert.DeserializeObject<CQAPICountryList>(content);

						PCIBO_DAL dao = new PCIBO_DAL();
					}
				}
			}
			catch (Exception e)
			{

				throw e;
			}
		}
	}
}
