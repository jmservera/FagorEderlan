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
            string actualDate = DateTime.Now.ToString("yyMMddhhmmss");
            if (fileName.Contains("_curv"))
            {
                actualDate += "_curv";
                actualDate += ".csv";
                File.Copy(Path.Combine(referencePath, fileName), Path.Combine(destinationPath, actualDate), true);
            }
            else
            {
                string allText = File.ReadAllText(Path.Combine(referencePath, fileName));
                string firstLine = allText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0];
                string secondLine = allText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[1];
                string[] data = secondLine.Split(';');
                var timestamp = DateTime.Now;
                data[0] = timestamp.ToString("dd.MM.yy");
                data[1] = timestamp.ToString("hh:mm:ss");
                actualDate += ".csv";
                File.WriteAllLines(Path.Combine(destinationPath, actualDate), new string[] { firstLine, string.Join(";", data) });
            }
            Console.WriteLine("Created {0} file", actualDate);
        }

        private string referencePath { get; set; }
        private string destinationPath { get; set; }
    }
}
