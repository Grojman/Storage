using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace GeneradorInformesCualitativos.Services;

public static class LocalDataManager
{
    public static void SaveFileJson(object valeToJson, string filePath)
    {
        if (!File.Exists(filePath)) File.Create(filePath).Close();

        File.WriteAllText(filePath, JsonConvert.SerializeObject(valeToJson));
    }

    public static T? DeserializeJsonFile<T>(string filePath) where T : class
    {
        if (!File.Exists(filePath)) return null;

        return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
    }

    public static void SaveFlowDocument(FlowDocument document, string filePath) 
    {
        TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            textRange.Save(fileStream, DataFormats.Rtf);
        }
    }

    public static FlowDocument GetFlowDocument(string filePath)
    {

        FlowDocument document = new();

        if (File.Exists(filePath))
        {
            TextRange textRange = new TextRange(document.ContentStart, document.ContentEnd);
            using (FileStream fileStream = new FileStream(ApplicationPaths.reportLocalFilePath, FileMode.Open))
            {
                textRange.Load(fileStream, DataFormats.Rtf);
            }
        }

        return document;
    }

}