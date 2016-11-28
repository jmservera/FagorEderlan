using System;
using System.Configuration;
using System.IO;

namespace FileSender.Helper
{
    public class LogHelper
    {
        LogCategory category;
        bool accumulable;
        string fileName;
        string fileFullPath;

        static LogHelper()
        {
            string logFolder = ConfigurationManager.AppSettings["logHelperFolder"];
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
        }

        /// <summary>
        /// Log class
        /// </summary>
        /// <param name="category">Category type</param>
        public LogHelper(LogCategory category, bool accumulable)
        {
            this.category = category;
            this.accumulable = accumulable;
            fileName = this.category.ToString() + ".txt";
            fileFullPath = $@"{ConfigurationManager.AppSettings["logHelperFolder"]}\{fileName}";
            if (!File.Exists(fileFullPath))
            {
                using (FileStream stream = File.Create(fileFullPath)) { }
            }
        }

        /// <summary>
        /// Function to write on log file
        /// </summary>
        /// <param name="text">Content to be written</param>
        public void WriteLog(string text)
        {
            TextWriter tsw = new StreamWriter(fileFullPath,accumulable);

            //Writing text to the file.
            tsw.WriteLine(text);

            //Close the file.
            tsw.Close();
        }

        /// <summary>
        /// Function to read log file's content
        /// </summary>
        /// <returns>Read content</returns>
        public string ReadLog()
        {
            string text = File.ReadAllText(fileFullPath);
            return text;
        }
    }
}
