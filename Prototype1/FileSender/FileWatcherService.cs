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
            Console.WriteLine($"File created {e.FullPath}");
            files.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldvalue) => DateTime.Now);
            log();           
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File changed {e.FullPath}");
            files.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldvalue) => DateTime.Now);
            log();
        }

        private void log()
        {
            Console.WriteLine("**** Current list *****");
            foreach(var value in files)
            {
                Console.WriteLine($"{value.Key}:\t{value.Value}");
            }
        }

        protected override void OnPause()
        {
            watcher.EnableRaisingEvents = false;
            stopTimer();
            base.OnPause();
        }
        protected override void OnContinue()
        {
            watcher.EnableRaisingEvents = true;
            startTimer();
            base.OnContinue();
        }
        protected override void OnStop()
        {

            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            stopTimer();
        }

        private async void sendFilesTimer_Tick(object sender)
        {
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
                if (filesToSend.Count > 0)
                {
                    using (MemoryStream stream = new MemoryStream()) //todo use temp file
                    {
                        using (ZipOutputStream zip = new ZipOutputStream(stream))
                        {
                            zip.SetLevel(9);

                            foreach (var file in filesToSend)
                            {
                                DateTime d;
                                if (files.TryRemove(file, out d))
                                {
                                    Trace.TraceInformation($"Zipping file: {file}");
                                    try
                                    {
                                        using (var fileStream = new FileStream(file, FileMode.Open))
                                        {
                                            var name = Path.GetFileName(file);
                                            ZipEntry entry = new ZipEntry(file);
                                            entry.DateTime = DateTime.Now;
                                            zip.PutNextEntry(entry);
                                            StreamUtils.Copy(fileStream, zip, new byte[4096]);
                                            zip.CloseEntry();
                                        }
                                    }
                                    catch (Exception fex)
                                    {
                                        Trace.TraceError($"Exception reading file {file}. {fex.Message}");
                                        files.AddOrUpdate(file, DateTime.Now, (key, oldValue) => DateTime.Now);
                                    }
                                }
                            }
                            // TODO Send stream to Azure
                            await storageHelper.UploadZipToStorage(stream);
                        }
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
