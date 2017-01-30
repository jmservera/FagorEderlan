# IoT File Sender solution #

Here you find all the code for this solution architecture:

* **ArduinoSensor**: it is a simple *.ino* file to read telemetry from an Arduino 101 with a Grove Hat and temperature and light sensors. It will be used by the application to read the data through the serial port. This readings will enhance the provided data with ambient values.
* **AzureDeployment**: an Azure Template for deploying all the needed cloud assets using Azure Resource Manager. This includes creating an IoT Hub, Azure Functions and Azure Stream Analytics.
* **CsvGenerator**: whenever you need to test the whole deployment without a real machine, this generator will simulate the data generation by providing it a source filename with test data and a destination folder where the files must be generated.
* **FileProcessorFunction**: contains two Azure Functions to uncompress the received files and to perform the needed calculations to prepare the data for the machine learning model.
  * GetIndicators: creates a new tuple of curve characteristics from the source file
  * UncompressInputFiiles: takes the arrived file and expands the files that are contained inside. It also cleans the files by removing diacritics and unused values.
* **FileSender**: this is the main component, it should be installed in the control computer, where the *.CSV* files are generated. It uses an IoT Hub secure connection to upload the generated files and to send some realtime information gathered by the sensor in the Arduino 101 board. In fact, it will send anything received through the specified serial port to the IoT Hub. Before sending the files it will compress them to save bandwidth. It also has some reliability checks and strategies for avoiding transient connectivity problems. 
* **Netduino Sensor**: another sensor project, but for the Netduino platform.

If you want to test some of the functionality locally you may need to execute the Azure Storage Locally. To start it manually you can execute the command:

```"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" start```


