using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Descript.Models;

public record Rune
{
    public required char Glyph { get; init; }
    
    public string Translation { get; init => field = value.ToLower(); } = string.Empty;
    public ConfidenceLevel Confidence { get; init; } = ConfidenceLevel.Low;

    public override int GetHashCode()
    {
        return Glyph.GetHashCode();
    }

    public virtual bool Equals(Rune? other)
    {
        return other?.Glyph == Glyph;
    }

    public bool Equals(char? other)
    {
        return other?.Equals(Glyph) ?? false;
    }

    public override string ToString()
    {
        return $"Rune: ({Glyph}) - {Translation} : {Confidence}";
    }

    [JsonIgnore]
    public const int CodePointStart = 0xE000;

    [JsonIgnore]
    public int Id => Glyph - CodePointStart;

    public static Rune FromId(int id)
    {
        return new Rune
        {
            Glyph = (char)(CodePointStart + id)
        };
    }

    public static UnicodeRange UnicodeRange => new(CodePointStart, 4096);
}