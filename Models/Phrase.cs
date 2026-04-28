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
    public string DisplayString => Translation != string.Empty ? Glyphs + " · " + Translation : Glyphs;
    
    public bool HasTranslation => Translation != "";
    public bool ShowAddPhraseButton => !HasTranslation && Elements.All(s => s.Confidence == ConfidenceLevel.High);
    
    
    public void Refresh()
    {
        OnPropertyChanged(nameof(Elements));
        OnPropertyChanged(nameof(DisplayString));
    }
}