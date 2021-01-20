using System;
using System.IO;

namespace PCI.Helpers
{
    public class LogHelper
    {
        private string _logFile;

        public LogHelper(bool useDateInFilename = false)
        {
            _logFile = "log" + (useDateInFilename ? DateTime.Now.ToString("yyyyMMdd") : "") + ".txt";
            if (!File.Exists(_logFile))
            {
                File.Create(_logFile).Close();
            }
        }

        public LogHelper(string path, bool useDateInFilename = false)
        {
            if (!Directory.Exists(path))
            {
                // Create the directory
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            
            _logFile = path + "//log" + (useDateInFilename ? DateTime.Now.ToString("yyyyMMdd") : "") + ".txt";
            if (!File.Exists(_logFile))
            {
                File.Create(_logFile).Close();
            }
        }

        public void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss #") + message);
            using (StreamWriter stream = new StreamWriter(_logFile, true))
            {
                stream.Write(DateTime.Now.ToString("yyyyy/MM/dd hh:mm:ss #") + message + stream.NewLine);
            }
        }
    }
}
