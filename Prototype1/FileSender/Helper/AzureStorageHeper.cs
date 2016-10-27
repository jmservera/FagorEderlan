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
        // Azure Storage Account Credentials
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
            using (var fileStream = System.IO.File.OpenRead(fileFolder+ "\\" + fileName))
            {
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
            blockBlob.Properties.ContentType = "application/zip";
            await blockBlob.SetPropertiesAsync();
        }
    }
}
