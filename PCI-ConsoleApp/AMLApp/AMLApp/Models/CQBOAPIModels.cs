using System;
using System.Collections.Generic;
using System.Text;

namespace AMLApp.Models
{
	public class CQBOAPIModels
	{
		public class CQAPIResult
		{
			public int code { get; set; }
			public string message { get; set; }

			public CQAPIResultData data { get; set; }
		}

		public class CQAPIResultData
		{
			public int userId { get; set; }
			public string token { get; set; }
		}

		public class CQAPICountryList
		{
			public string totalRecord { get; set; }
			public string pageSize { get; set; }
			public string currentPage { get; set; }
			public string totalPages { get; set; }
			public string code { get; set; }
			public string message { get; set; }
			public List<CQAPICountryListData> data { get; set; }
		}

		public class CQAPICountryListData
		{
			public string cd_Ref { get; set; }
			public string description { get; set; }
		}

		public class CQAPIRegionList
		{
			public string totalRecord { get; set; }
			public string pageSize { get; set; }
			public string currentPage { get; set; }
			public string totalPages { get; set; }
			public string code { get; set; }
			public string message { get; set; }
			public List<CQAPIRegionListData> data { get; set; }
		}

		public class CQAPIRegionListData
		{
			public string form_CodeID { get; set; }
			public string form_CodeDescription { get; set; }
		}
	}
}
