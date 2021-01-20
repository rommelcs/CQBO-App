using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CSVFileReader.Model.CQBOAPIModel;

namespace CSVFileReader.Controller
{
    public class ValidateCountry
    {
        public bool SearchCountry(CQAPICountryList countryRes, string ctryCD)
        {
            bool retCtryVal = false;
            try { 
            


            retCtryVal = validateCountryCD(countryRes, ctryCD);

            if (!retCtryVal)
            {
                retCtryVal = validateCountryName(countryRes, ctryCD);

            }

            //if country valid
            if (retCtryVal)
            {
                Console.WriteLine("Your Country Code " + ctryCD + " is valid.");
            }
            else
            {
                Console.WriteLine("Your Country Code is invalid");
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SearchCountry: " + ex.Message);
               // throw ex;
            }

            return retCtryVal;

        }

        public  bool validateCountryCD(CQAPICountryList countryRes, string ctryCD)
        {
            bool retVal = false;
            try { 
            

            var cCode = countryRes.data.Where(x => x.cd_Ref == ctryCD).FirstOrDefault();
            if (cCode != null)
            {
                if (cCode.cd_Ref != "")
                {
                    retVal = true;
                }
            }

           
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in validateCountryCD: " + ex.Message);
                //throw ex;
            }
            return retVal;
        }

        public  bool validateCountryName(CQAPICountryList countryRes, string ctryCD)
        {
            bool retVal = false;
            try
            {

                var cCode = countryRes.data.Where(x => x.description.TrimEnd().TrimStart().ToUpper() == ctryCD.ToUpper()).FirstOrDefault().description.ToUpper();

                if (cCode != "")
                {
                    retVal = true;
                }

               
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in validateCountryName: "+ex.Message);
               // throw ex;
            }
            return retVal;

        }

    }
}
