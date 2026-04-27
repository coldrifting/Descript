using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Descript.Models;
using Descript.Models.Flat;

namespace Descript.Data;

public static class DataManagement
{
    static DataManagement()
    {
#if DEBUG
        FolderPath = Path.Combine(AppContext.BaseDirectory, "Seed");
#else
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        FolderPath = Path.Combine(appDataPath, "Descript");

        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }
#endif
    }

    private static string FolderPath { get; set; }

    private static JsonSerializerOptions SerializerOptions => new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter<ConfidenceLevel>()
        }
    };

    public static Translations Load()
    {
        if (!Directory.Exists(FolderPath))
        {
            Console.WriteLine($"Directory does not exist: {FolderPath}");
            return new Translations();
        }

        string filePath = Path.Combine(FolderPath, "Translations.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Translations File not found: {filePath}");
            return new Translations();
        }

        string json = File.ReadAllText(filePath);
        Translations translations = JsonSerializer.Deserialize<Translations>(json, SerializerOptions) ?? new Translations();

        return translations;
    }
    
    public static void Save(Translations translations)
    {
        if (!Directory.Exists(FolderPath))
        {
            Console.WriteLine($"Directory does not exist: {FolderPath}");
            try
            {
                Directory.CreateDirectory(FolderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Unable to create directory: {FolderPath}");
                return;
            }
        }

        string filePath = Path.Combine(FolderPath, "Translations.json");

        string elementsJson = JsonSerializer.Serialize(translations.Elements, SerializerOptions)
            .Replace("},{", "},\n\t\t{")
            .Replace("[{", "[\n\t\t{")
            .Replace("}]", "}\n\t]");
        string phrasesJson = JsonSerializer.Serialize(translations.Phrases, SerializerOptions)
            .Replace("},{", "},\n\t\t{")
            .Replace("[{", "[\n\t\t{")
            .Replace("}]", "}\n\t]");
        string sentencesJson = JsonSerializer.Serialize(translations.Sentences, SerializerOptions)
            .Replace("},{", "},\n\t\t{")
            .Replace("[{", "[\n\t\t{")
            .Replace("}]", "}\n\t]");

        string json = $"{{\n\t\"Elements\": {elementsJson}," + 
                      $"\n\t\"Phrases\": {phrasesJson}," +
                      $"\n\t\"Sentences\": {sentencesJson} \n }}";
        
        try
        {
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save data: {filePath}");
            Console.WriteLine(ex.Message);
        }
    }
}