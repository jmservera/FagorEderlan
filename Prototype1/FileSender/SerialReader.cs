using FileSender.Helper;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSender
{
    public class SerialReader:IDisposable
    {
        IoTHelper hub;
        string port;

        SerialPort serialPort;
        private Queue<byte> recievedData = new Queue<byte>();
        public SerialReader(string port, IoTHelper hub)
        {
            this.port = port;
            this.hub = hub;
            serialPort = new SerialPort(port,115200,Parity.None,8,StopBits.One);
            serialPort.DtrEnable = true;
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
        }

        private async void startReading()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    var values= serialPort.ReadLine();
                    await hub.SendDataAsync(values);
                }
            });
        }

        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var value=serialPort.ReadLine();
            //byte[] data = new byte[serialPort.BytesToRead];
            //int lenght=serialPort.Read(data, 0, data.Length);
            //var value=Encoding.ASCII.GetString(data);
            await hub.SendDataAsync(value);
        }

        void processData()
        {
            // Determine if we have a "packet" in the queue
            if (recievedData.Count > 50)
            {
                var packet = Enumerable.Range(0, 50).Select(i => recievedData.Dequeue());
            }
        }

        public void Dispose()
        {
           Dispose(true);
        }

        protected void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (serialPort != null)
                {
                    serialPort.Dispose();
                }
            }
        }

        ~SerialReader()
        {
            Dispose(false);
        }

    }
}
