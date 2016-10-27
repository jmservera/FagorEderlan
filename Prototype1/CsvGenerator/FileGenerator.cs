using System;
using System.Collections.Generic;
using System.IO;

namespace CsvGenerator
{
    public class FileGenerator
    {
        public FileGenerator(string _referencePath, string _destinationPath)
        {
            referencePath = _referencePath;
            destinationPath = _destinationPath;
        }

        public List<string> getCsvFileNames()
        {
            var fileList = Directory.GetFiles(referencePath, "*.csv");

            List<string> fileNames = new List<string>();
            foreach (var item in fileList)
            {
                string tempName = Path.GetFileName(item);
                fileNames.Add(tempName);
            }

            return fileNames;
        }

        public void GenerateCsv(string referencePath, string destinationPath, string fileName)
        {
            File.Copy(referencePath + fileName, destinationPath + fileName, true);
            Console.WriteLine("Created {0} file", fileName);
        }

        private string referencePath { get; set; }
        private string destinationPath { get; set; }
    }
}
