using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSender.Helper
{
    /// <summary>
    /// Manages the number of errors for each notified file. When the allowed errors limit is reached for a file, it
    /// is removed from the manager and PoisonedFileDetected event is fired with file's information.
    /// </summary>
    public class PoisonFileManager
    {
        /// <summary>
        /// Dictionary containing the files with errors. The Key of the dictionary is the complete path of the file
        /// and the Value is the number of errors for that file.
        /// </summary>
        private Dictionary<string, int> filesWithErrors = new Dictionary<string, int>();

        /// <summary>
        /// Number of errors allowed per file before being marked as poisoned file.
        /// </summary>
        public int AllowedErrorsLimit { get; set; } = 3;

        /// <summary>
        /// Occurs when a file reaches the allowed errors limit.
        /// </summary>
        public event EventHandler<PoisonedFileEventArgs> PoisonedFileDetected;

        /// <summary>
        /// Initializes a new instance of the PoisonFileManager class, with the default allowed error limit.
        /// </summary>
        public PoisonFileManager() { }

        /// <summary>
        /// Initializes a new instance of the PoisonFileManager class with the specified allowed error limit.
        /// </summary>
        /// <param name="allowedErrorsLimit">Times a file can be notified for errors before being marked it as poisoned.</param>
        public PoisonFileManager(int allowedErrorsLimit)
        {
            this.AllowedErrorsLimit = allowedErrorsLimit;
        }

        /// <summary>
        /// Notifies error for the especidied file. When a file reaches the allowed errors limit PoisonedFileDetected
        /// event is fired with file's information and it is removed from the manager.
        /// </summary>
        /// <param name="completeFilePath">Complete path of the file.</param>
        public void NotifyErrorInFile(string completeFilePath)
        {
            string fileName = Path.GetFileName(completeFilePath);
            if (this.filesWithErrors.ContainsKey(completeFilePath))
            {
                this.filesWithErrors[completeFilePath]++;
                Console.WriteLine($"{fileName}: {this.filesWithErrors[completeFilePath]} errors");
                if (this.filesWithErrors[completeFilePath] >= this.AllowedErrorsLimit)
                {
                    this.filesWithErrors.Remove(completeFilePath);
                    this.PoisonedFileDetected?.Invoke(this, new PoisonedFileEventArgs(completeFilePath, this.AllowedErrorsLimit));
                    Console.WriteLine($"{fileName}: poisoned file!");
                }
            }
            else
            {
                this.filesWithErrors.Add(completeFilePath, 1);
                Console.WriteLine($"New file with error: {fileName}");
                Console.WriteLine($"{fileName}: {this.filesWithErrors[completeFilePath]} error");
            }
        }

        /// <summary>
        /// Returns the number of errors for the specified file.
        /// </summary>
        /// <param name="completeFilePath">Complete path of the file.</param>
        /// <returns>The number of errors for the file, or 0 if the specified file is not managed.</returns>
        public int GetNumberOfErrorsForFile(string completeFilePath)
        {
            if (this.filesWithErrors.ContainsKey(completeFilePath))
            {
                return this.filesWithErrors[completeFilePath];
            }
            else
            {
                return 0;
            }
        }
    }

    public class PoisonedFileEventArgs : EventArgs
    {
        /// <summary>
        /// Complete path of the poisoned file.
        /// </summary>
        public string CompleteFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Directory information of the poisoned file.
        /// </summary>
        public string Directory { get { return Path.GetDirectoryName(this.CompleteFilePath); } }

        /// <summary>
        /// Poisoned file name and extension).
        /// </summary>
        public string FileName { get { return Path.GetFileName(this.CompleteFilePath); } }

        /// <summary>
        /// Number of notified errors for the poisoned file.
        /// </summary>
        public int NumberOfErrors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the PoisonedFileEventArgs class with the specified file path.
        /// </summary>
        /// <param name="completeFilePath">Complete path of the poisoned file.</param>
        /// <param name="numberOfErrors">Number of notified errors for the poisoned file.</param>
        public PoisonedFileEventArgs(string completeFilePath, int numberOfErrors)
        {
            this.NumberOfErrors = numberOfErrors;
            this.CompleteFilePath = completeFilePath;
        }
    }
}
