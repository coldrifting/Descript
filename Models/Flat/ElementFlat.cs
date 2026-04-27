using System;

namespace Descript.Models.Flat;

public record ElementFlat
{
    public required char Glyph { get; init; }

    public string Translation { get; init; } = string.Empty;
    public ConfidenceLevel Confidence { get; init; } = ConfidenceLevel.Low;

    public static Func<Element, ElementFlat> FromElement =>
        element => new ElementFlat
        {
            Glyph = element.Glyph, 
            Translation = element.Translation, 
            Confidence = element.Confidence
        };
    
    public static Func<ElementFlat, Element> ToElement =>
        element => new Element
        {
            Glyph = element.Glyph, 
            Translation = element.Translation, 
            Confidence = element.Confidence
        };
}