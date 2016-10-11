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

namespace FileSender
{
    public partial class FileWatcherService : ServiceBase
    {
        FileSystemWatcher watcher;
        public FileWatcherService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string folder = ConfigurationManager.AppSettings["folder"];

            watcher = new FileSystemWatcher(folder, "*.csv");
            watcher.IncludeSubdirectories = true;
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;

            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Trace.WriteLine($"File created {e.FullPath}");
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Trace.WriteLine($"File changed {e.FullPath}");
        }

        protected override void OnStop()
        {
        }


    }
}
