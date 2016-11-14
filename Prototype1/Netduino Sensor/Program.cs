using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Text;
using System.IO.Ports;

namespace Sensorduino
{
    /// <summary>
    /// NOTE: THIS CODE WORKS ON A NETDUINO 4.2 SDK / uFWK ENVIRONMENT
    ///       IT MUST BE DEVELOP ON VISUAL STUDIO 2010-2013
    /// </summary>
    public class Program
    {
        // establish the frequency of delivery of messages
        static int frequency = 5000;

        public static void Main()
        {
            // init onboard led port
            var ledPort = new OutputPort(Pins.ONBOARD_LED, false);
            bool on = true;

            // analog pin for temperature
            AnalogInput temperaturePin = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);

            // analog pin for light
            AnalogInput lightPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A1);

            // init UART
            string message = "";
            SerialPort _serialPort = new SerialPort("COM1", 9600, Parity.Even, 8, StopBits.One);
            _serialPort.Open();

            while (true)
            {
                // make blink on board led to know it's working
                ledPort.Write(on);
                on = !on;

                // ZX-Sensors gives values between 0 and 1023
                // read temperature value and convert it to celcius units
                int rawTemperature = temperaturePin.ReadRaw();
                double celciusTemp = ConvertToCelcius(rawTemperature);
                Debug.Print("Temp value: " + celciusTemp.ToString());

                // read light value
                int light = lightPin.ReadRaw();
                Debug.Print("Light value: " + light + "\n");

                // manually serialize message into JSON
                message = "{ 'temp':" + celciusTemp + ", 'light':" + light + " }";

                // send through serial port
                _serialPort.Write(Encoding.UTF8.GetBytes(message), 0, message.Length);

                // Wait to next cycle
                Thread.Sleep(frequency);
            }
        }

        /// <summary>
        /// Little convertor from raw data to Celcius
        /// </summary>
        /// <param name="val">Raw input</param>
        /// <returns>Celcius unit</returns>
        private static double ConvertToCelcius(double val)
        {
            if (val < 340)
            {
                val = -5.0 + (val - 120.0) / (340 - 120) * (15.0 + 5.0) + 3.5;
            }
            else if (val < 500)
            {
                val = 15.0 + (val - 340.0) / (500 - 340) * (30 - 15.0) + 3.5;
            }
            else if (val < 660)
            {
                val = 30.0 + (val - 500.0) / (660 - 500) * (50 - 30) + 3.5;
            }
            else
            {
                val = 50.0 + (val - 660.0) / (790 - 660) * (75 - 50) + 3.5;
            }
            return val;
        }
    }
}
