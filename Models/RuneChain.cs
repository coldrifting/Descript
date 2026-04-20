using Descript.Utils;

namespace Descript.Models;

public record RuneChain
{
    public required string Glyphs { get; init; }
    
    public string Translation { get; init => field = value.ToTitleCase(); } = string.Empty;
    public ConfidenceLevel Confidence { get; init; } = ConfidenceLevel.Low;

    public override int GetHashCode()
    {
        return Glyphs.GetHashCode();
    }

    public virtual bool Equals(RuneChain? other)
    {
        return other?.Glyphs == Glyphs;
    }

    public bool Equals(string? other)
    {
        return other?.Equals(Glyphs) ?? false;
    }
    
    public static RuneChain FromString(string str)
    {
        return new RuneChain
        {
            Glyphs = str
        };
    }
}