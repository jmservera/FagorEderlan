using System;
using System.Collections;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using System.Globalization;
using Newtonsoft.Json;

const bool jsonOutput = false;

public static void Run(Stream myBlob, string name, Binder binder, TraceWriter log)
{
    log.Info($"C# Blob trigger function processed: {myBlob}");

    using (var file = new ZipFile(myBlob))
    {
        log.Info(file.Name);

        foreach (ZipEntry zipEntry in file)
        {
            try
            {
                var container = "output-files";
                if (!zipEntry.Name.Contains("_curv"))
                {
                    container = "average-files";
                }
                log.Info($"Container: {container} \t file: {zipEntry.Name}");
                var outputExtension = jsonOutput ? "json" : "csv";
                var attributes = new Attribute[]
                {
                    new BlobAttribute($"{container}/{Path.GetFileNameWithoutExtension(zipEntry.Name)}.{outputExtension}"),
                    new StorageAccountAttribute("files_STORAGE")
                };

                //async work does not manage zip correctly
                using (var writer = binder.BindAsync<TextWriter>(attributes).GetAwaiter().GetResult())
                {
                    using (Stream s = file.GetInputStream(zipEntry))
                    {
                        using (var r = new StreamReader(s, Encoding.GetEncoding(1252)))
                        {
                            var ss = r.ReadLine();
                            ss = "ref;" + ss;
                            var header = cleanHeader(ss);
                            if (zipEntry.Name.Contains("_curv"))
                            {
                                r.ReadLine();
                            }

                            if (jsonOutput)
                            {
                                writeJson(header, zipEntry, writer, r);
                            }
                            else
                            {
                                writeCSV(header, zipEntry, writer, r);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error reading Zip", ex);
            }
        }
    }
}

static void writeJson(string header, ZipEntry zipEntry, TextWriter writer, StreamReader r)
{
    writer.Write("[");
    bool first = true;
    while (!r.EndOfStream)
    {
        if (!first)
        {
            writer.WriteLine(",");
        }
        else
        {
            first = false;
        }
        var datos = extractData(r.ReadLine(), Path.GetFileNameWithoutExtension(zipEntry.Name));
        dynamic jObject = createObject(header, datos);
        writer.Write(JsonConvert.SerializeObject(jObject));
    }
    writer.Write("]");
}

static void writeCSV(string header, ZipEntry zipEntry, TextWriter writer, StreamReader r)
{
    writer.WriteLine(header);

    while (!r.EndOfStream)
    {
        var datos = extractData(r.ReadLine(), Path.GetFileNameWithoutExtension(zipEntry.Name));
        writer.WriteLine(datos);
    }
}
static string extractData(string rawData, string fileName)
{
    var data = cleanValues(rawData);
    return $"{fileName};{data}";
}

static object createObject(string header, string data)
{
    dynamic jObject = new System.Dynamic.ExpandoObject();
    var headers = header.Split(';');
    var datas = data.Split(';');
    for (int i = 0; i < datas.Length; i++)
    {
        ((IDictionary<String, Object>)jObject).Add(headers[i], datas[i]);
    }
    return jObject;
}

static string cleanHeader(string header)
{
    header = header.Replace("1.", ";").Replace(".", "");
    header = string.Join(";", header.Split(';').Select((s) => s.Trim().Replace(" ", string.Empty)).Select((s) =>
    {
        if (!String.IsNullOrEmpty(s) && char.IsDigit(s[0]))
        {
            return s.Substring(1);
        }
        else
        {
            return s;
        }
    }));
    return cleanValues(RemoveDiacritics(header));
}

static string cleanValues(string values)
{
    var valuesArray = values.Split(';');
    var cleanValues = valuesArray.Where((s) => !String.IsNullOrEmpty(s)).Select((s) => s.Trim()).ToArray();
    return string.Join(";", cleanValues);
}

static string RemoveDiacritics(string text)
{
    var normalizedString = text.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder();

    foreach (var c in normalizedString)
    {
        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
        {
            stringBuilder.Append(c);
        }
    }

    return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
}