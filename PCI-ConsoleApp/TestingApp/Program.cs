using System;
using System.Text.RegularExpressions;

namespace TestingApp
{
	class Program
	{
		static void Main(string[] args)
		{

			string str = "Maria Alberta Charleson Smith";
			string toFind = "Maria charleson";

			bool result = new Regex(toFind, RegexOptions.IgnoreCase).Match(str).Success;

			Console.WriteLine(result);
		}
	}
}
