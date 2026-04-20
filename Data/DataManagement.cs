using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Descript.Models;

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
        },
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static T[] Load<T>()
    {
        if (!Directory.Exists(FolderPath))
        {
            Console.WriteLine($"Directory does not exist: {FolderPath}");
            return [];
        }

        string fileName = typeof(T).Name.Pluralize();
        string filePath = Path.Combine(FolderPath, $"{fileName}.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return [];
        }

        string json = File.ReadAllText(filePath);
        T[] items = JsonSerializer.Deserialize<T[]>(json, SerializerOptions) ?? [];

        return items;
    }
    
    public static void Save<T>(IEnumerable<T> items)
    {
        if (!Directory.Exists(FolderPath))
        {
            Console.WriteLine($"Directory does not exist: {FolderPath}");
            return;
        }

        string fileName = typeof(T).Name.Pluralize() + ".json";
        string filePath = Path.Combine(FolderPath, $"{fileName}");
        
        string json = JsonSerializer.Serialize(items, SerializerOptions)
            .Replace("[{", "[\n\t{")
            .Replace("}]", "}\n]")
            .Replace("},{", "},\n\t{");
        try
        {
            Console.WriteLine($"Saving: {fileName}");
            File.WriteAllText(filePath, Regex.Unescape(json));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save data: {fileName}");
            Console.WriteLine(ex.Message);
        }
    }

    private static string Pluralize(this string str)
    {
        if (str.EndsWith('y'))
        {
            return string.Concat(str.AsSpan(0, str.Length - 1), "ies");
        }

        return str + "s";
    }
}