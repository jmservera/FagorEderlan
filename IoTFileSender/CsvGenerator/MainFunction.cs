using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CsvGenerator
{
    public class MainFunction
    {
        FileGenerator generator;
        List<string> fileNames;
        int i = 0;
        int zipInterval = 30 * 1000; //five minutes
        //int zipInterval = 1000;

        public MainFunction(string _referencePath, string _destinationPath)
        {
            referencePath = _referencePath;
            destinationPath = _destinationPath;
            generator = new FileGenerator(_referencePath, _destinationPath);
        }

        public void StartTimer()
        {
            fileNames = generator.getCsvFileNames();
            Timer sendFilesTimer;
            sendFilesTimer = new Timer(createFilesTimer_Tick, null, zipInterval, zipInterval);
            Console.WriteLine("Process started, press intro to finish...");
            Console.ReadLine();
        }

        private void createFilesTimer_Tick(object sender)
        {
            Console.WriteLine("{0} of {1}", i, fileNames.Count);
            if (fileNames.Count() > i)
            {
                generator.GenerateCsv(referencePath, destinationPath, fileNames.ElementAt(i));
                i++;
                generator.GenerateCsv(referencePath, destinationPath, fileNames.ElementAt(i));
                i++;
            }
            else
            {
                Environment.Exit(Environment.ExitCode);
            }
        }

        public string referencePath { get; set; }
        public string destinationPath { get; set; }
    }
}
