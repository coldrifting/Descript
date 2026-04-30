using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Descript.ViewModels.Base;

namespace Descript.Models;

public partial class Sentence : ViewModelBase
{
    public required string SentenceOriginal { get; set => SetField(ref field, value); }
    public string SentenceTranslated { get; private set => SetField(ref field, value); } = string.Empty;

    public required ImmutableArray<PhraseBase> Phrases
    {
        get;
        set
        {
            SentenceTranslated = value
                .Select(phraseBase => phraseBase is Phrase phrase && phrase.Translation != string.Empty 
                    ? phrase.Translation 
                    : phraseBase.Glyphs)
                .Aggregate((a, b) => a + ' ' + b);
            
            SetField(ref field, value);
        }
    }

    public string Category { get; set => SetField(ref field, value); } = string.Empty;
    public string SubCategory { get; set => SetField(ref field, value); } = string.Empty;
    public string Context { get; set => SetField(ref field, value); } = string.Empty;
    
    public float NumUntranslatedPhrases =>
        Phrases.Count(p => p is Phrase px && px.Translation != string.Empty);

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

    public static bool Matches(Sentence sentence, string filterText, string context)
    {
        return filterText.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(filterEntry => 
                sentence.SentenceOriginal.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) || 
                sentence.SentenceTranslated.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase)) && 
               context.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(filterEntry =>
                sentence.Category.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) ||
                sentence.SubCategory.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) ||
                sentence.Context.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public void Refresh()
    {
        OnPropertyChanged(nameof(Phrases));
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

        if (e.PropertyName is nameof(SentenceOriginal))
        {
        }
    }
}