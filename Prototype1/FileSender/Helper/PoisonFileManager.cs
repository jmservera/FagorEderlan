using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSender.Helper
{
    public class PoisonFileManager
    {
        private Dictionary<string, int> poisonFiles = new Dictionary<string, int>();

        public int AllowedErrorLimit { get; set; } = 3;

        public event EventHandler<PoisonFileEventArgs> AllowedErrorLimitReached;

        public PoisonFileManager() { }

        public PoisonFileManager(int errorLimit)
        {
            this.AllowedErrorLimit = errorLimit;
        }

        public void AddOrUpdatePoisonFile(string fileCompletePath)
        {
            if (this.poisonFiles.ContainsKey(fileCompletePath))
            {
                this.poisonFiles[fileCompletePath]++;
                if (this.poisonFiles[fileCompletePath] > this.AllowedErrorLimit)
                {
                    this.poisonFiles.Remove(fileCompletePath);
                    this.AllowedErrorLimitReached?.Invoke(this, new PoisonFileEventArgs(fileCompletePath));
                }
            }
            else
            {
                this.poisonFiles.Add(fileCompletePath, 1);
            }
        }
    }

    public class PoisonFileEventArgs
    {
        public string CompleteFilePath { get; set; } = string.Empty;

        public PoisonFileEventArgs() { }

        public PoisonFileEventArgs(string completeFilePath)
        {
            this.CompleteFilePath = completeFilePath;
        }
    }
}
