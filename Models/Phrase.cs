using System;
using System.Collections.Immutable;
using System.Linq;

namespace Descript.Models;

public class Phrase() : PhraseBase("")
{
    public required ImmutableArray<Element> Elements
    {
        get;
        set
        {
            Glyphs = new string(value.Select(s => s.Glyph).ToArray());
            SetField(ref field, value);
        }
    }

    public string Translation { get; set => SetField(ref field, value); } = string.Empty;
    public ConfidenceLevel Confidence { get; set => SetField(ref field, value); } = ConfidenceLevel.Low;
    
    public bool ShowElements => Confidence == ConfidenceLevel.Low && 
                                Translation == "";

    public bool ShowAddPhraseButton => Confidence == ConfidenceLevel.Low && 
                                       Translation == "" && 
                                       Elements.All(s => s.Confidence == ConfidenceLevel.High);
    
    public static Func<Phrase, bool> ShouldSave => phrase => phrase.Translation != "";
    
    public void Refresh()
    {
        OnPropertyChanged(nameof(Elements));
    }
}