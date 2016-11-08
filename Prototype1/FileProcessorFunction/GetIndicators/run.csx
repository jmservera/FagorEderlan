using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;

public static void Run(Stream myBlob, string name, out string outputBlob, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

    int year = int.Parse(name.Substring(0, 2));
    int month = int.Parse(name.Substring(2, 2));
    int day = int.Parse(name.Substring(4, 2));
    int hour = int.Parse(name.Substring(6, 2));
    int minute = int.Parse(name.Substring(8, 2));
    int seconds = int.Parse(name.Substring(10, 2));

    DateTime thisDate = new DateTime(year + 2000, month, day, hour, minute, seconds);

    using (var reader = new StreamReader(myBlob))
    {
        var line = reader.ReadLine();   // read the line with the header 
        line = reader.ReadLine();       // read the line with units information
        List<float> tiempo = new List<float>();
        List<double> carrera = new List<double>();
        List<float> diffPre = new List<float>();

        while (!reader.EndOfStream)
        {
            line = reader.ReadLine();     // reading lines
            var values = line.Split(';'); // reading columns!!!

            tiempo.Add(float.Parse(values[1]));//.Replace(".", ",")));   //ms
            carrera.Add(double.Parse(values[2]));//.Replace(".", ","))); //mm
            diffPre.Add(float.Parse(values[3]));//.Replace(".", ",")));
        }

        var velocity = CalculateDerivate(carrera);      // m/s
        var acceleration = CalculateDerivate(velocity); // m/s^2
        var jerk = CalculateDerivate(acceleration);     // m/s^3

        List<double> sublist = carrera.GetRange(20, 40);  // get the range of the curve 
        double areaCarreraSub = AreaUC(sublist);         // calculate de area of the subcurve

        var velocityChars = SomeStatistics(velocity);
        var accelerationChars = SomeStatistics(acceleration);
        var jerkChars = SomeStatistics(jerk);

        //outputBlob = name + textVelocity + " ; " + textAcceleration + " ; " + textJerk + " ; " + areaCarreraSub.ToString();
        // string header = "fileName; vmax; vmin; vmean; amax; amin; amean; jmax; jmin; jmean; area";

        var outObject = new
        {
            date = thisDate,
            reference = name,
            rarea = areaCarreraSub,
            vmax = velocityChars.Item1,
            vmin = velocityChars.Item2,
            vmean = velocityChars.Item3,
            amax = accelerationChars.Item1,
            amin = accelerationChars.Item2,
            amean = accelerationChars.Item3,
            jmax = jerkChars.Item1,
            jmin = jerkChars.Item2,
            jmean = jerkChars.Item3
        };

        outputBlob = JsonConvert.SerializeObject(outObject);
    }
}

static List<double> CalculateDerivate(List<double> curva)
{
    List<double> derivateList = new List<double>();
    float dt = (float)1 / 1000; // seconds
    double derivate, curr, prev;

    for (var i = 0; i < curva.Count - 1; i++)
    {
        curr = curva[i + 1] / 1000; //metre
        prev = curva[i] / 1000; //metre
        if ((curr - prev) == 0)
        {
            derivate = 0;
        }
        else
        {
            derivate = (curr - prev) / dt;
        }
        derivateList.Add(derivate);
    }
    return derivateList;
}

static double AreaUC(List<double> curva)
{
    // the area under the whole curve
    double y, sumArea = 0;

    for (var i = 1; i < curva.Count() - 1; i++)
    {
        y = Math.Abs(curva[i] - curva[i + 1]);
        sumArea = sumArea + (y / 2);
    }
    return sumArea; // Math.Round(sumArea,4); 
}

static Tuple<double, double, double> SomeStatistics(List<double> curva)
{
    //double sx = StandardDeviation(curva);
    return new Tuple<double, double, double>(curva.Max(), curva.Min(), curva.Sum() / curva.Count);
}
