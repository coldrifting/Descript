using System;

namespace Descript.Models.Flat;

public record SentenceFlat
{
    public required string Sentence { get; init; }
    
    public string Category { get; init; } = string.Empty;
    public string SubCategory { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;

    public static Func<Sentence, SentenceFlat> FromSentence =>
        sentence => new SentenceFlat
        {
            Sentence = sentence.OriginalSentence, 
            Category = sentence.Category, 
            SubCategory = sentence.SubCategory,
            Context = sentence.Context
        };
}