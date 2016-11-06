using Microsoft.Azure.Devices.Client;
using System.Configuration;
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

        public async Task UploadZipToStorage(string fileName, string fileFolder)
        {
            await deviceClient.OpenAsync();

            using (var fileStream = System.IO.File.OpenRead(Path.Combine(fileFolder, fileName)))
            {
                await deviceClient.UploadToBlobAsync(fileName, fileStream);
            }
        }

        public Task SendDataAsync(string data)
        {
            return deviceClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes(data)));
        }
    }
}
