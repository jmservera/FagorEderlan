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

        /// <summary>
        /// Log class
        /// </summary>
        /// <param name="_category">Category type</param>
        public LogHelper(LogCategory _category, bool _accumulable)
        {
            category = _category;
            accumulable = _accumulable;
            fileName = category.ToString() + ".txt";
            fileFullPath = ConfigurationManager.AppSettings["DefaultPath"] + "\\" + fileName;
            if (!File.Exists(fileFullPath))
            {
                File.Create(fileFullPath);
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
