using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Descript.ViewModels.Base;

namespace Descript.Models;

public partial class Sentence : ViewModelBase
{
    public required string OriginalSentence { get; set => SetField(ref field, value); }
    public required ImmutableArray<PhraseBase> Phrases { get; set => SetField(ref field, value); }

    public string Category { get; set => SetField(ref field, value); } = string.Empty;
    public string SubCategory { get; set => SetField(ref field, value); } = string.Empty;
    public string Context { get; set => SetField(ref field, value); } = string.Empty;
    
    public float NumUntranslatedPhrases =>
        Phrases.Count(p => p is Phrase { Confidence: ConfidenceLevel.Low or ConfidenceLevel.Medium });

    public float NumUntranslatedElements => Phrases
            .OfType<Phrase>()
            .Where(p => p.Translation == string.Empty)
            .SelectMany(p => p.Elements)
            .Select(e => e.Confidence != ConfidenceLevel.High)
            .Count();
    
    public static string[] Split(string sentence)
    {
        return RunesRegex().Split(sentence)
            .Select(s => s.Trim())
            .Where(s => s != string.Empty)
            .ToArray();
    }

    [GeneratedRegex(@"([\uE000-\uEFFF]+)")]
    private static partial Regex RunesRegex();

    public void Refresh()
    {
        OnPropertyChanged(nameof(Phrases));
        Console.WriteLine("Phrase_Changing...");
        foreach (PhraseBase phraseBase in Phrases)
        {
            if (phraseBase is Phrase phrase)
            {
                phrase.Refresh();
            }
        }
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(OriginalSentence))
        {
        }
    }
}