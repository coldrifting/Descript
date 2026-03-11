using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Descript.Models;

[method: JsonConstructor]
public class Translation(int id, List<int> wordIds, List<string> plainWords, string category = "", string subCategory = "", string context = "")
{
    public int Id { get; } = id;
    public List<int> WordIds { get; } = wordIds;
    public List<string> PlainWords { get; } = plainWords;
    public string Category { get; set; } = category;
    public string SubCategory { get; set; } = subCategory;
    public string Context { get; set; } = context;
}

public class TranslationBlocks(int id)
{
    public int Id { get; set; } = id;
    public List<TranslationBlock> Blocks { get; set; } = [];
}

// Word
public class TranslationBlock(int id, ConfidenceLevel confidenceLevel, string raw = "", string translation = "", List<TranslationBlockItem>? symbols = null)
{
    public int Id { get; set; } = id;
    
    public ConfidenceLevel ConfidenceLevel { get; set; } = confidenceLevel;
    
    public bool IsConfirmed => ConfidenceLevel == ConfidenceLevel.Confirmed;
    public bool IsHigh => ConfidenceLevel == ConfidenceLevel.High;
    public bool IsMedium => ConfidenceLevel == ConfidenceLevel.Medium;
    public bool IsLow => ConfidenceLevel == ConfidenceLevel.Low;

    public string Raw { get; set; } = raw;
    public string Translation { get; set; } = translation;

    public bool HasTranslation => Translation != "" && !Translation.StartsWith('?');
    public bool HasNoTranslation => !HasTranslation;
    
    public List<TranslationBlockItem> Symbols { get; set; } = symbols ?? [];
}

// Rune
public class TranslationBlockItem(int id, ConfidenceLevel confidenceLevel, string raw, string translation)
{
    public int Id { get; set; } = id;
    
    public ConfidenceLevel ConfidenceLevel { get; set; } = confidenceLevel;
    
    public bool IsConfirmed => ConfidenceLevel == ConfidenceLevel.Confirmed;
    public bool IsHigh => ConfidenceLevel == ConfidenceLevel.High;
    public bool IsMedium => ConfidenceLevel == ConfidenceLevel.Medium;
    public bool IsLow => ConfidenceLevel == ConfidenceLevel.Low;
    
    public string Raw { get; set; } = raw;
    public string Translation { get; set; } = translation;
}
