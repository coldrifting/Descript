namespace Descript.Models;

public record RuneSentence
{
    public required string Sentence { get; init; }
    
    public string Category { get; init; } = string.Empty;
    public string SubCategory { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;

    public override int GetHashCode()
    {
        return Sentence.GetHashCode();
    }

    public virtual bool Equals(RuneSentence? other)
    {
        return other?.Sentence == Sentence;
    }

    public bool Equals(string? other)
    {
        return other?.Equals(Sentence) ?? false;
    }
    
    public static RuneSentence FromString(string str)
    {
        return new RuneSentence
        {
            Sentence = str
        };
    }
}

/*
public class TranslationWords(int id)
{
    public int Id { get; set; } = id;
    public List<TranslationWord> Blocks { get; set; } = [];
}

// Word
public class TranslationWord(int id, ConfidenceLevel confidenceLevel, string raw = "", string translation = "", List<TranslationRune>? symbols = null)
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
    
    public List<TranslationRune> Symbols { get; set; } = symbols ?? [];
}

// Rune
public class TranslationRune(int id, ConfidenceLevel confidenceLevel, string raw, string translation)
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
*/