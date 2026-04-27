using System;

namespace Descript.Models.Flat;

public record PhraseFlat
{
    public required string Glyphs { get; init; }
    
    public string Translation { get; init; } = string.Empty;
    public ConfidenceLevel Confidence { get; init; } = ConfidenceLevel.Low;

    public static Func<Phrase, PhraseFlat> FromPhrase =>
        phrase => new PhraseFlat
        {
            Glyphs = phrase.Glyphs, 
            Translation = phrase.Translation, 
            Confidence = phrase.Confidence
        };
}