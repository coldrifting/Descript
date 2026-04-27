using System;
using Descript.ViewModels.Base;

namespace Descript.Models;

public class Element : ViewModelBase
{
    public required char Glyph { get; set => SetField(ref field, value); }
    
    public string Translation { get; set => SetField(ref field, value); } = string.Empty;
    public ConfidenceLevel Confidence { get; set => SetField(ref field, value); } = ConfidenceLevel.Low;

    public bool IsCurrentSelection { get; init; }

    private const int CodePointStart = 0xE000;
    public int Id => Glyph - CodePointStart;
    
    public static Element FromId(int id)
    {
        return new Element
        {
            Glyph = (char)(CodePointStart + id)
        };
    }
    
    public static Element FromIdSelected(int id)
    {
        return new Element
        {
            Glyph = (char)(CodePointStart + id),
            IsCurrentSelection = true,
            Confidence = ConfidenceLevel.Confirmed
        };
    }

    public static Func<Element, Element> Select => element =>
        new Element
        {
            Glyph = element.Glyph,
            Confidence = element.Confidence,
            Translation = element.Translation,
            IsCurrentSelection = true
        };
    
    public static Func<Element, bool> ShouldSave => element => element.Translation != "";

    public static bool IsElement(char glyph)
    {
        return glyph >= CodePointStart && glyph <= CodePointStart + 4096;
    }
}