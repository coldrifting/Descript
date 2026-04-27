using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.Models.Flat;
using Descript.ViewModels.Base;
using Descript.ViewModels.Dialog;

namespace Descript.ViewModels;

public partial class ViewModelSentences(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    
    public DialogSentence Dialog { get; } = new(mainWindowViewModel);

    private readonly Dictionary<string, Sentence> _sentences = new();

    public string FilterText { get; set => SetField(ref field, value); } = string.Empty;
    public string FilterContextText { get; set => SetField(ref field, value); } = string.Empty;
    public bool ShowFilterCancel => FilterText.Length > 0;
    public bool ShowFilterContextCancel => FilterContextText.Length > 0;
    
    public IEnumerable<Sentence> Sentences => _sentences.Values.OrderBy(s => s.OriginalSentence);
    public IEnumerable<Sentence> SentencesFiltered => OrderBy(_sentences.Values
        .Where(sentence => IsSentenceMatch(sentence, FilterText))
        .Where(sentence => IsContextMatch(sentence, FilterContextText)));

    private IOrderedEnumerable<Sentence> OrderBy(IEnumerable<Sentence> sentences)
    {
        return SortMode switch
        {
            SentenceSortMode.ByCategory => sentences.OrderBy(rs => rs.Category)
                .ThenBy(sentence => sentence.SubCategory.ToLower())
                .ThenBy(sentence => sentence.Context.ToLower())
                .ThenBy(sentence => sentence.OriginalSentence.ToLower()),
            SentenceSortMode.ByLeastTranslated => sentences.OrderByDescending(sentence => sentence.UntranslatedPhrasesPercentage)
                .ThenBy(sentence => sentence.Category.ToLower())
                .ThenBy(sentence => sentence.SubCategory.ToLower())
                .ThenBy(sentence => sentence.Context.ToLower())
                .ThenBy(sentence => sentence.OriginalSentence.ToLower()),
            _ => sentences.OrderBy(sentence => sentence.TranslatedPhrasesPercentage)
                .ThenBy(sentence => sentence.Category.ToLower())
                .ThenBy(sentence => sentence.SubCategory.ToLower())
                .ThenBy(sentence => sentence.Context.ToLower())
                .ThenBy(sentence => sentence.OriginalSentence.ToLower())
        };
    }

    // Add Sentence Dialog
    public int SelectionStart { get; set => SetField(ref field, value); }
    public int SelectionEnd { get; set => SetField(ref field, value); }
    
    public SentenceSortMode SortMode { get; set => SetField(ref field, value); } = SentenceSortMode.ByCategory;

    public List<SentenceSortMode> SortModes =>
    [
        SentenceSortMode.ByCategory,
        SentenceSortMode.ByLeastTranslated,
        SentenceSortMode.ByMostTranslated
    ];

    [RelayCommand]
    private void ClearFilterText()
    {
        FilterText = string.Empty;
        OnPropertyChanged(nameof(SentencesFiltered));
    }

    [RelayCommand]
    private void ClearFilterContextText()
    {
        FilterContextText = string.Empty;
        OnPropertyChanged(nameof(SentencesFiltered));
    }

    private bool IsSentenceMatch(Sentence sentence, string sentenceRaw)
    {
        string currentTranslations = sentence.OriginalSentence
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Vm.ViewModelPhrases[s]?.Translation ?? "")
            .Aggregate((a, b) => a + b);
        
        return sentenceRaw.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(filterEntry => 
                sentence.OriginalSentence.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) || 
                currentTranslations.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase));
    }

    private static bool IsContextMatch(Sentence sentence, string context)
    {
        return context.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(filterEntry =>
                sentence.Category.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) ||
                sentence.SubCategory.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) ||
                sentence.Context.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public bool Add(SentenceFlat sentenceFlat, bool update = true)
    {
        if (_sentences.TryAdd(sentenceFlat.Sentence, ToSentence(sentenceFlat)))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Sentences));
            }

            return true;
        }

        Console.WriteLine($"Sentence {sentenceFlat.Sentence} already exists");
        return false;
    }

    public void Add(IEnumerable<SentenceFlat> sentences)
    {
        foreach (SentenceFlat sentenceFlat in sentences)
        {
            Add(sentenceFlat, false);
        }

        OnPropertyChanged(nameof(Sentences));
    }

    public void Edit(string sentence, SentenceFlat sentenceFlat)
    {
        if (sentence == sentenceFlat.Sentence)
        {
            //_sentences[sentenceFlat.Sentence].Phrases
        }
        else
        {
            foreach (Phrase oldPhrase in _sentences[sentence].Phrases.OfType<Phrase>().Where(oldPhrase => oldPhrase is { Translation: "" }))
            {
                Vm.ViewModelPhrases.Remove(oldPhrase.Glyphs);
            }
            
            _sentences.Remove(sentence);
            Add(sentenceFlat);
        }
        
        OnPropertyChanged(nameof(Sentences));
    }

    public bool TryGet(string sentenceRaw, [MaybeNullWhen(false)] out Sentence sentence)
    {
        return _sentences.TryGetValue(sentenceRaw, out sentence);
    }

    private Sentence ToSentence(SentenceFlat sentenceFlat)
    {
        List<PhraseBase> phrases = [];
        
        string[] sentencePhrases = Sentence.Split(sentenceFlat.Sentence);
        foreach (string phrase in sentencePhrases)
        {
            if (Element.IsElement(phrase.FirstOrDefault(' ')))
            {
                Vm.ViewModelPhrases.Add(phrase);
                phrases.Add(Vm.ViewModelPhrases[phrase] ?? throw new ArgumentException(""));
            }
            else
            {
                phrases.Add(new PhraseBase(phrase));
            }
        }

        return new Sentence
        {
            OriginalSentence = sentenceFlat.Sentence,
            Phrases = [..phrases],
            Category = sentenceFlat.Category,
            SubCategory = sentenceFlat.SubCategory,
            Context = sentenceFlat.Context,
        };
    }

    [RelayCommand]
    private void DeleteSentence(string sentence)
    {
        // Remove words with no added info
        foreach (Phrase oldPhrase in _sentences[sentence].Phrases.OfType<Phrase>().Where(oldPhrase => oldPhrase is { Translation: "" }))
        {
            Vm.ViewModelPhrases.Remove(oldPhrase.Glyphs);
        }
        
        _sentences.Remove(sentence);
        OnPropertyChanged(nameof(Sentences));
    }

    [RelayCommand]
    private void Filter(string filter)
    {
        FilterText = filter;
    }

    [RelayCommand]
    private void FilterChar(char filter)
    {
        FilterText = filter.ToString();
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(Sentences):
            case nameof(SortMode):
                OnPropertyChanged(nameof(SentencesFiltered));
                break;

            case nameof(FilterText):
                OnPropertyChanged(nameof(ShowFilterCancel));
                OnPropertyChanged(nameof(SentencesFiltered));
                break;
            case nameof(FilterContextText):
                OnPropertyChanged(nameof(ShowFilterContextCancel));
                OnPropertyChanged(nameof(SentencesFiltered));
                break;
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(Sentences));
    }
}