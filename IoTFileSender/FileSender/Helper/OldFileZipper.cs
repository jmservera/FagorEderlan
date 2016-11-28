using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileSender.Helper
{
    public class OldFileZipper
    {
        private List<string> FileNames;
        private string LastFileName;

        /// <summary>
        /// Old zipped filed's class constructor
        /// </summary>
        /// <param name="path">Folder path where Csv files are stored</param>
        public OldFileZipper(string path)
        {
            LastFileName = getLastCsvZippedFileName();
            FileNames = getMissingFileNames(path);
        }

        /// <summary>
        /// Gets last Csv files that has been zipped.
        /// </summary>
        /// <returns></returns>
        private string getLastCsvZippedFileName()
        {
            LogHelper fileLog = new LogHelper(LogCategory.lastFile, false);
            string line = fileLog.ReadLog();
            if (!string.IsNullOrEmpty(line))
                Trace.WriteLine($"Last zipped csv {line}");
            return line;
        }

        /// <summary>
        /// Returns a list of Csv file names that have not been zipped.
        /// </summary>
        /// <param name="folderPath">Path where Csv files ar stored</param>
        /// <returns></returns>
        private List<string> getMissingFileNames(string folderPath)
        {
            List<string> files = Directory.GetFiles(folderPath).ToList();

            bool finished = false;
            while (!finished)
            {
                if (files.Count == 0 || string.IsNullOrEmpty(this.LastFileName))
                {
                    finished = true;
                }
                else
                {
                    if (files.ElementAt(0) == LastFileName)
                    {
                        finished = true;
                    }
                    files.RemoveAt(0);
                }
            }

            return files;
        }

        /// <summary>
        /// Return saved list of Csv files when object class is created
        /// </summary>
        /// <returns></returns>
        public List<string> getMissingZippedFileNames()
        {
            return FileNames;
        }
    }
}
