using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using System.Globalization;

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
                var attributes = new Attribute[]
                {
                    new BlobAttribute($"{container}/{zipEntry.Name}"),
                    new StorageAccountAttribute("storageederlan_STORAGE")
                };

                //async work does not manage zip correctly
                using (var writer = binder.BindAsync<TextWriter>(attributes).GetAwaiter().GetResult())
                {
                    using (Stream s = file.GetInputStream(zipEntry))
                    {
                        using (var r = new StreamReader(s, Encoding.GetEncoding(1252)))
                        {
                            if (zipEntry.Name.Contains("_curv"))
                            {
                                var ss = r.ReadLine();
                                ss = "ref;" + ss;
                                writer.WriteLine(cleanHeader(ss));
                                r.ReadLine();

                                while (!r.EndOfStream)
                                {
                                    var datos = cleanValues(r.ReadLine());
                                    datos = $"{Path.GetFileNameWithoutExtension(zipEntry.Name)};{datos}";
                                    writer.WriteLine(datos);
                                }
                            }
                            else
                            {
                                var ss = r.ReadLine();
                                ss = "ref;" + ss;
                                writer.WriteLine(cleanHeader(ss));
                                while (!r.EndOfStream)
                                {
                                    var datos = cleanValues(r.ReadLine());
                                    datos = $"{Path.GetFileNameWithoutExtension(zipEntry.Name)};{datos}";
                                    writer.WriteLine(datos);
                                }
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