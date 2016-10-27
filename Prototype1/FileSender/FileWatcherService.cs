using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using FileSender.Helper;

namespace FileSender
{
    public partial class FileWatcherService : ServiceBase
    {
        System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> files=new System.Collections.Concurrent.ConcurrentDictionary<string, DateTime>();
        Timer sendFilesTimer;
        object sync = new object();
        FileSystemWatcher watcher;
        int zipInterval = 5 * 60 * 10000; //five minutes

        public CloudBlobContainer container;
        public AzureStorageHelper storageHelper;

        public FileWatcherService()
        {
            InitializeComponent();
        }

        void startTimer()
        {
            sendFilesTimer = new Timer(sendFilesTimer_Tick, null, zipInterval / 10, zipInterval / 10);
        }
        void stopTimer()
        {
            if (sendFilesTimer != null)
            {
                sendFilesTimer.Dispose();
                sendFilesTimer = null;
            }
        }

        protected override void OnStart(string[] args)
        {
            string folder = ConfigurationManager.AppSettings["folder"];

            ////todo: remove, only test
            //files.AddOrUpdate(@"C:\temp\filetest\VINMasterList.csv", DateTime.Now.AddMinutes(-50), (s, d) => DateTime.Now.AddMinutes(-50));
            //sendFilesTimer_Tick(null);
            //return;

            watcher = new FileSystemWatcher(folder, "*.csv");
            watcher.IncludeSubdirectories = true;
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;

            watcher.EnableRaisingEvents = true;

            storageHelper = new AzureStorageHelper();

            string interval = ConfigurationManager.AppSettings["interval"];
            if (!string.IsNullOrEmpty(interval))
            {
                int value = 0;
                if(int.TryParse(interval,out value))
                {
                    if (value > 1000)
                    {
                        Trace.TraceInformation($"Zip interval set to {value}");
                        zipInterval = value;
                    }
                }
            }
            startTimer();
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Trace.TraceInformation($"File created {e.FullPath}");
            files.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldvalue) => DateTime.Now);
            log();           
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Trace.TraceInformation($"File changed {e.FullPath}");
            files.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldvalue) => DateTime.Now);
            log();
        }

        private void log()
        {
            Trace.WriteLine("**** Current list *****");
            foreach(var value in files)
            {
                Trace.WriteLine($"{value.Key}:\t{value.Value}");
            }
        }

        protected override void OnPause()
        {
            Trace.TraceInformation("Pause");
            watcher.EnableRaisingEvents = false;
            stopTimer();
            base.OnPause();
        }
        protected override void OnContinue()
        {
            Trace.TraceInformation("Recover from pause");
            watcher.EnableRaisingEvents = true;
            startTimer();
            base.OnContinue();
        }
        protected override void OnStop()
        {
            Trace.TraceInformation("Stop");

            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            stopTimer();
        }
        private async void sendFilesTimer_Tick(object sender)
        {
            Trace.TraceInformation("File check");
            var filesToSend=new List<string>();
            try
            {

                var filesEnum = files.GetEnumerator();
                var old = DateTime.Now.AddMilliseconds(0-zipInterval);
                while (filesEnum.MoveNext())
                {
                    if (filesEnum.Current.Value < old)
                    {
                        filesToSend.Add(filesEnum.Current.Key);
                    }
                }
                foreach(var name in filesToSend)
                {
                    DateTime t;
                    files.TryRemove(name, out t);
                }
                if (filesToSend.Count > 0)
                {

                    try
                    {
                        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        var zipFolder = Path.Combine(folder, "ZipFiles");
                        Directory.CreateDirectory(zipFolder);
                        
                        var fileName =  DateTime.Now.ToString("yyyyMMddHHmmss") +".zip";
                        ZipFile zip = ZipFile.Create(zipFolder+ "\\" + fileName);
                        zip.BeginUpdate();
                        foreach (var file in filesToSend)
                        {
                            zip.Add(file, Path.GetFileName(file));
                        }
                        zip.CommitUpdate();
                        zip.Close();
                        await storageHelper.UploadZipToStorage(fileName, zipFolder);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}
