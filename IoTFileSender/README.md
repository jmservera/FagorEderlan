# IoT File Sender solution #

Here you find all the code for this solution architecture:

* **ArduinoSensor**: it is a simple *.ino* file to read telemetry from an Arduino 101 with a Grove Hat and temperature and light sensors. It will be used by the application to read the data through the serial port. This readings will enhance the provided data with ambient values.

* **AzureDeployment**: an Azure Template for deploying all the needed cloud assets using Azure Resource Manager. This includes creating an IoT Hub, Azure Functions and Azure Stream Analytics.

To execute the Azure Storage Locally you can do:

"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" start
