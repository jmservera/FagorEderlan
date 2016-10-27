using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSender.Helper
{
    public class LogHelper
    {
        LogCategory category;
        bool accumulable;
        string fileFullPath;

        /// <summary>
        /// Log class
        /// </summary>
        /// <param name="_category">Category type</param>
        public LogHelper(LogCategory _category, bool _accumulable)
        {
            category = _category;
            accumulable = _accumulable;
            fileFullPath = "%TEMP%\\" + category.ToString() + ".txt";
            if (!File.Exists(fileFullPath))
            {
                File.Create(fileFullPath);
            }
        }

        public void WriteLog(string text)
        {
            TextWriter tsw = new StreamWriter(fileFullPath,accumulable);

            //Writing text to the file.
            tsw.WriteLine(text);

            //Close the file.
            tsw.Close();
        }

        public string ReadLog()
        {
            string text = File.ReadAllText(fileFullPath);
            return text;
        }
    }
}
