using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CSVFileReader.Model.CQBOAPIModel;

namespace CSVFileReader.Controller
{
    public class ValidateRegion
    {
        public  void SearchRegion(CQAPIRegionList regRes, string regionCD)
        {

            try { 
            bool retRegVal = false;


            retRegVal = validateRegionCD(regRes, regionCD); //check by region code

            //checkbyname if code is invalid
            if (!retRegVal)
            {
                retRegVal = validateRegionName(regRes, regionCD);
            }

                //if region is valid
            if (retRegVal)
            {
                Console.WriteLine("Your Region Code "+ regionCD+" is valid.");
            }
            else 
            {
                //if region is invalid
                Console.WriteLine("Your Region Code is invalid");
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SearchRegion: " + ex.Message);
              //  throw ex;
            }


            // return retRegVal;

        }

        public bool validateRegionCD(CQAPIRegionList regRes, string regionCD)
        {
            bool retVal = false;
            try { 

            var cCode = regRes.data.Where(x => x.form_CodeID == regionCD).FirstOrDefault();
            if (cCode != null)
            {
                if (cCode.form_CodeID != "")
                {
                    retVal = true;
                }
            }

           
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in validateRegionCD: " + ex.Message);
             //   throw ex;
            }
            return retVal;
        }

        public bool validateRegionName(CQAPIRegionList regRes, string regionCD)
        {
            bool retVal = false;
            try
            {
                var rCode = regRes.data.Where(x => x.form_CodeDescription.TrimEnd().TrimStart().ToUpper() == regionCD.ToUpper()).FirstOrDefault().form_CodeDescription.ToUpper();

                if (rCode != "")
                {
                    retVal = true;
                }


               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in validateRegionName: " + ex.Message);
               // throw ex;
            }
            return retVal;
        }

    }
}
