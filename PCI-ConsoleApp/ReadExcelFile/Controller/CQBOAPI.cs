using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json.Serialization;
using CSVFileReader.Model;
using static CSVFileReader.Model.CQBOAPIModel;
using System.Linq;

namespace CSVFileReader.Controller
{
    public class CQBOAPI
    {
        static String str = String.Empty;
        public static async Task LoginAPIUser(UserCredentials credential, CQAPIResultData ValidUser)
        {
            try
            {
                string retVal = string.Empty;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://10.30.5.181/CQAPI/api/Authorize");
                    client.DefaultRequestHeaders.ExpectContinue = false;

                    var serializedObject = JsonConvert.SerializeObject(
                        new { username = credential.username, password = credential.password }
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

                        if (result.code == 0)
                        {
                            retVal = result.data.token;
                            str = retVal;
                            ValidUser.userId = result.data.userId;
                            ValidUser.token = result.data.token;
                        }
                        Console.WriteLine("------------ AUTHENTICATION DONE------------\n");

                    }
                    else
                    {
                        Console.WriteLine("\n " + response.StatusCode);
                        Console.WriteLine("\n " + response.ReasonPhrase.ToString());
                        Console.WriteLine("\n " + response.RequestMessage.ToString());
                        Console.WriteLine("\n " + response.ToString());
                    }


                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

        }

        public static  async Task<bool> getCountryCode(CQAPIResultData ValidUser, string ctryCD)
        {
            try
            {
                String uriAPI = "https://bkp.cqfutures.com/CQAPI/api/CQBO/country/GetCountryList";
                bool retVal = false;
               
                int found = 0; // if found = 1 means found
              

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(uriAPI);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add($"Authorization", $"Bearer " + ValidUser.token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(uriAPI + "?userId=" + ValidUser.userId + "&cd_type=CNTR&PageSize=500&PageNo=1");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        //Console.WriteLine("Country List");
                        CQAPICountryList countryRes = JsonConvert.DeserializeObject<CQAPICountryList>(content);


                        //list out all country list
                        //foreach (var item in countryRes.data)
                        //{
                        //    Console.WriteLine(item.cd_Ref + " || " + item.description);

                        //}
                        ValidateCountry country = new ValidateCountry();

                        retVal = country.SearchCountry(countryRes, ctryCD);
                    }
                }

                
                return retVal;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public static async Task getRegionList(CQAPIResultData ValidUser, string ctryCode,string regionCD)
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

                    HttpResponseMessage response = await client.GetAsync(uriAPI + "?userId=" + ValidUser.userId + "&Form_Code=DIVDCD&ref_codeID=" + ctryCode );

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
