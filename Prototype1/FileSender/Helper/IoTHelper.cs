using Microsoft.Azure.Devices.Client;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FileSender.Helper
{
    public class IoTHelper
    {
        DeviceClient deviceClient;

        public IoTHelper()
        {
            var connectionString = ConfigurationManager.AppSettings["IoTHubConnectionString"];
            var connBuilder = IotHubConnectionStringBuilder.Create(connectionString);
            if (connBuilder.UsingX509Cert)
            {
                var x509Certificate = new X509Certificate2("deviceCertificate.pfx", "devicecertificate");
                var authMethod = new DeviceAuthenticationWithX509Certificate(connBuilder.DeviceId, x509Certificate);

                deviceClient = DeviceClient.Create(connBuilder.HostName, authMethod, TransportType.Amqp);
            }
            else
            {
                deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Amqp);
            }
        }
        static int count;
        public async Task<bool> UploadZipToStorage(string fileName, string fileFolder)
        {
            try
            {
                await deviceClient.OpenAsync();

                using (var fileStream = System.IO.File.OpenRead(Path.Combine(fileFolder, fileName)))
                {
                    Console.WriteLine($"Upload {count++} {fileName}");

                    await deviceClient.UploadToBlobAsync(fileName, fileStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0}: {1}", nameof(UploadZipToStorage), ex.Message);
            }
            return false;
        }

        public Task SendDataAsync(string data)
        {
            return deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes(data)));
        }
    }
}
