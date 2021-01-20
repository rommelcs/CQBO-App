using System;
using System.Collections.Generic;
using System.Text;

namespace PCI.Helpers.Models
{
	public class WinSCPOptionsModel
	{
        public string Protocol { get; set; }
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SshHostKey { get; set; }
    }

	public class SFTPFileCollection
	{
		public string Code { get; set; }
		public string ConfigType { get; set; }
		public string Hostname { get; set; }
		public string Protocol { get; set; }
		public int TCP_Port { get; set; }
		public int CredentialNeed { get; set; }
		public string Host_Key { get; set; }
		public string PrivateKeyPath { get; set; }
		public string PrivatekeyPassPhrase { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string FileExt { get; set; }
		public string Description { get; set; }
		public string FileLoc { get; set; }
		public string Filename { get; set; }
		public string DestFilename { get; set; }
		public Boolean DatedFilename { get; set; }
		public string DateFormatInFile { get; set; }

	}
}
