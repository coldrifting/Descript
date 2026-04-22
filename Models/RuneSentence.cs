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

    public bool EqualsFull(RuneSentence? other)
    {
        return other?.Sentence == Sentence && 
               other.Category == Category && 
               other.SubCategory == SubCategory && 
               other.Context == Context;
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