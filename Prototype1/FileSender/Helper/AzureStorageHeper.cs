using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSender.Helper
{
    public class AzureStorageHelper
    {
        // Azure Storage Account Credentials -  DELETE FROM HERE
        //public string azureStorageAccountName = "eticdemostorageaccount";
        public string azureStorageAccountKey;
        public string azureStorageAccountContainer;
        public string azureStorageAccountName;
        public CloudBlobContainer container;
        

        public AzureStorageHelper()
        {
            azureStorageAccountKey = ConfigurationManager.AppSettings["StorageConnectionString"];
            azureStorageAccountContainer = ConfigurationManager.AppSettings["StorageContainer"];
            azureStorageAccountName = ConfigurationManager.AppSettings["StorageAccountName"];

            if (!string.IsNullOrEmpty(azureStorageAccountKey) || !string.IsNullOrEmpty(azureStorageAccountContainer) || !string.IsNullOrEmpty(azureStorageAccountName))
            {
                // Create an Azure CloudStorageAccount object.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName="
                    + azureStorageAccountName + ";AccountKey=" + azureStorageAccountKey);

                // Create a CloudBlobClient object using the CloudStorageAccount.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                // Create a CloudBlobContain object using the CloudBlobClient.

                // Retrieve a reference to a container.
                this.container = blobClient.GetContainerReference(azureStorageAccountContainer);
            }
            else
            {
                if (string.IsNullOrEmpty(azureStorageAccountContainer))
                {
                    Trace.TraceError("Can't connect to azure storage. Error on Azure Container");
                }
                if (string.IsNullOrEmpty(azureStorageAccountKey))
                {
                    Trace.TraceError("Can't connect to azure storage. Error on Azure Account Key");
                }
                else
                {
                    Trace.TraceError("Can't connect to azure storage. Error on Azure Account Name");
                }
            }
            
        }
        public async Task CreateContainerAsync()
        {
            // Create the container if it doesn't already exist.
            await this.container.CreateIfNotExistsAsync();
        }
        public async Task UploadZipToStorage(string fileName, string fileFolder)
        {

            // Retrieve reference to a blob named "userName".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            using (var fileStream = System.IO.File.OpenRead(fileFolder))
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
            blockBlob.Properties.ContentType = "application/zip";
            await blockBlob.SetPropertiesAsync();


            ////zipFile.Flush();
            ////zipFile.Position = 0;
            ////using (var fstr = new FileStream("c:\\temp\\filetest\\test.zip", FileMode.Create))
            ////{
            ////    zipFile.CopyTo(fstr);
            ////}
            //string year, month, day, hour, minute, second;
            //year = DateTime.Now.Year.ToString();
            //month = DateTime.Now.Month.ToString();
            //day = DateTime.Now.Day.ToString();
            //hour = DateTime.Now.Hour.ToString();
            //minute = DateTime.Now.Minute.ToString();
            //second = DateTime.Now.Second.ToString();
            
            //CloudBlockBlob blockBlob = container.GetBlockBlobReference(year + "-" + month + "-" + day + "-" + hour + ":" + minute + ":" + second); // TODO - name for the container.
            //zipFile.Flush();
            //zipFile.Position = 0;
            //blockBlob.Properties.ContentType = "application/zip";
            //blockBlob.UploadFromStream(zipFile);
            //await blockBlob.SetPropertiesAsync();
        }
    }
}
