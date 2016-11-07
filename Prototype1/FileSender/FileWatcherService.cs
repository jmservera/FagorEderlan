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
using System.Collections.Concurrent;
using System.Net;

namespace FileSender
{
    public partial class FileWatcherService : ServiceBase
    {
        const int FileQuantityInZip = 100;
        ConcurrentDictionary<string, DateTime> files = new ConcurrentDictionary<string, DateTime>();
        Timer sendFilesTimer;
        object sync = new object();
        FileSystemWatcher watcher;
        int zipInterval = 5 * 60 * 10000; //five minutes

        public CloudBlobContainer container;
        public IoTHelper ioTHelper;
        public SerialReader reader;
        public LogHelper logHelper;
        private PoisonFileManager poisonFileManager;

        string csvFolder = ConfigurationManager.AppSettings["folder"];
        string zipFolder;
        string poisonZipFolder;

        public FileWatcherService()
        {
            InitializeComponent();

            this.zipFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZipFiles");
            if (!Directory.Exists(zipFolder))
            {
                Directory.CreateDirectory(zipFolder);
            }
            this.poisonZipFolder = Path.Combine(this.zipFolder, "Poison");
            if (!Directory.Exists(poisonZipFolder))
            {
                Directory.CreateDirectory(poisonZipFolder);
            }
        }

        private void startTimer()
        {
            sendFilesTimer = new Timer(sendFilesTimer_Tick, null, zipInterval / 10, zipInterval / 10);
        }

        private void stopTimer()
        {
            if (sendFilesTimer != null)
            {
                sendFilesTimer.Dispose();
                sendFilesTimer = null;
            }
        }

        protected override void OnStart(string[] args)
        {
            poisonFileManager = new PoisonFileManager(5);
            poisonFileManager.PoisonedFileDetected += PoisonedFileDetectedHandler;
            logHelper = new LogHelper(LogCategory.lastFile, false);
            ReadOldCsvFiles();
            watcher = new FileSystemWatcher(csvFolder, "*.csv");
            watcher.IncludeSubdirectories = true;
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;

            try
            {
                watcher.EnableRaisingEvents = true;

                ioTHelper = new IoTHelper();
                try
                {
                    string comPort = ConfigurationManager.AppSettings["serialPort"];
                    if (!string.IsNullOrEmpty(comPort))
                    {
                        reader = new SerialReader(comPort, ioTHelper);
                    }
                }
                catch (Exception serialEx)
                {
                    Trace.TraceError("{0}: SerialReader will not be used. {1}", nameof(OnStart), serialEx.Message);
                }

                string interval = ConfigurationManager.AppSettings["interval"];
                if (!string.IsNullOrEmpty(interval))
                {
                    int value = 0;
                    if (int.TryParse(interval, out value))
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
            catch (Exception ex)
            {
                Trace.TraceError($"OnStart error: {ex.Message}");
            }
        }

        private void ReadOldCsvFiles()
        {
            OldFileZipper oldFileZipper = new OldFileZipper(csvFolder);
            List<string> listOldFiles =  oldFileZipper.getMissingZippedFileNames();
            listOldFiles.ForEach(s => files.AddOrUpdate(s, DateTime.Now, (key, oldvalue) => DateTime.Now));
            Console.WriteLine("Old csv files:");
            foreach (KeyValuePair<string, DateTime> file in files)
            {
                Console.WriteLine($"\t{file.Key}, {file.Value}");
            }
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
            Trace.TraceInformation($"{this.ServiceName} paused.");
        }

        protected override void OnContinue()
        {
            files.Clear();
            ReadOldCsvFiles();
            watcher.EnableRaisingEvents = true;
            startTimer();
            base.OnContinue();
            Trace.TraceInformation($"{this.ServiceName} resumed.");
        }

        protected override void OnStop()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            stopTimer();
            Trace.TraceInformation($"{this.ServiceName} stoped.");
        }

        int sending;
        private async void sendFilesTimer_Tick(object sender)
        {
            if (!Utils.CheckForInternetConnection()) return;
            try
            {
                if (Interlocked.Exchange(ref sending, 1) != 0) return;

                await this.CheckOldZipFiles();

                var filesToSend = new List<string>();
                var filesEnum = files.GetEnumerator();
                var old = DateTime.Now.AddMilliseconds(0 - zipInterval);
                while (filesEnum.MoveNext())
                {
                    if (filesEnum.Current.Value < old)
                    {
                        filesToSend.Add(filesEnum.Current.Key);
                    }
                }
                foreach (var name in filesToSend)
                {
                    DateTime t;
                    files.TryRemove(name, out t);
                }
                if (filesToSend.Count > 0)
                {
                    try
                    {
                        for (int i = 0; i < filesToSend.Count; i += FileQuantityInZip)
                        {
                            int count = FileQuantityInZip;
                            if (filesToSend.Count - i < FileQuantityInZip)
                                count = filesToSend.Count - i;
                            List<string> fileRange = filesToSend.GetRange(i, count);
                            await CreateZipAndUpload(fileRange, i);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref sending, 0);
            }
        }

        public async Task CreateZipAndUpload (List<string> fileList, int batch)
        {
            string lastFile = "";
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + $"_{batch}.zip";
            Trace.TraceInformation($"Zipping file: {fileName}");

            ZipFile zip = ZipFile.Create(Path.Combine( zipFolder , fileName));
            zip.BeginUpdate();
            foreach (var file in fileList)
            {
                zip.Add(file, Path.GetFileName(file));
                lastFile = file;
            }
            zip.CommitUpdate();
            zip.Close();

            Console.WriteLine("File created.");
            if (!string.IsNullOrEmpty(lastFile))
                logHelper.WriteLog(lastFile);
            else
                Trace.TraceWarning("There are not lastFiles on the log file.");
            await UploadFile(fileName, zipFolder, fileList.Count);
        }

        private async Task UploadFile(string fileName, string zipFolder, int? zippedFileCount)
        {
            string completeFilePath = Path.Combine(zipFolder, fileName);
            try
            {
                if (await ioTHelper.UploadZipToStorage(fileName, zipFolder))
                {
                    if (zippedFileCount == null || zippedFileCount < 1)
                        Console.WriteLine($"{fileName} file sent.");
                    else
                        Console.WriteLine($"{fileName} file sent with {zippedFileCount} csvs.");
                    File.Delete(completeFilePath);
                }
                else
                {
                    Console.WriteLine($"{fileName} file not sent.");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error uploading {fileName}: {e.Message}{Environment.NewLine}Stack trace:{Environment.NewLine}{e.StackTrace}");
                this.poisonFileManager.NotifyErrorInFile(completeFilePath);
            }
        }

        int checking=0;
        private async Task CheckOldZipFiles()
        {
            if (Interlocked.CompareExchange(ref checking, 1, 0) == 0)
            {
                try
                {
                    foreach (string file in Directory.GetFiles(this.zipFolder))
                    {
                        if (file.EndsWith(".zip"))
                            await this.UploadFile(Path.GetFileName(file), this.zipFolder, null);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref checking, 0);
                }
            }
        }

        private void PoisonedFileDetectedHandler(object sender, PoisonedFileEventArgs e)
        {
            try
            {
                File.Move(e.CompleteFilePath, Path.Combine(this.poisonZipFolder, e.FileName));
                File.Delete(e.CompleteFilePath);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error moving {Path.GetFileName(e.CompleteFilePath)} poison file: {ex.Message}{Environment.NewLine}Stack trace:{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}
