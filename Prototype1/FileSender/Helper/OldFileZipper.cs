﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSender.Helper
{
    public class OldFileZipper
    {
        // Check file's name and path.
        private string logFile = "%TEMP%" + "log.txt";
        private List<string> FileNames;
        private string LastFileName;

        public OldFileZipper(string path)
        {
            LastFileName = getLastCsvZippedFileName();
            FileNames = getMissingFileNames(path);
        }

        private string getLastCsvZippedFileName()
        {
            string[] lines = File.ReadAllLines(logFile);
            Trace.WriteLine(lines.LastOrDefault());
            return lines.LastOrDefault().Split(',').LastOrDefault();
        }


        private List<string> getMissingFileNames(string folderPath)
        {
            List<string> files = Directory.GetFiles(folderPath).ToList();

            bool finished = false;
            while (!finished)
            {
                if (files.Count == 0)
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

        public List<string> getMissingZippedFileNames()
        {
            return FileNames;
        }
    }
}
