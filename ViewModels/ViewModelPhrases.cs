using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.Models.Flat;
using Descript.Utils;
using Descript.ViewModels.Base;
using Descript.ViewModels.Dialog;

namespace Descript.ViewModels;

public partial class ViewModelPhrases(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    public DialogPhrase Dialog { get; } = new(mainWindowViewModel);
    
    private readonly Dictionary<string, Phrase> _allPhrases = new();
    
    public Phrase? this[string index] => _allPhrases.GetValueOrDefault(index);
    
    public IEnumerable<Phrase> Phrases => _allPhrases.Values.OrderBy(phrase => phrase.Glyphs);
    public IList<Phrase> PhrasesFiltered => Phrases
        .Where(phrase => phrase.Translation.ContainsTrimmed(FilterText) || phrase.Glyphs.ContainsTrimmed(FilterText))
        .OrderBy(phrase => phrase.Translation == "" ? "Ω" : phrase.Translation)
        .ToList();

    public string FilterText { get; set => SetField(ref field, value); } = string.Empty;
    public bool CanClearFilters => FilterText.Length > 0;

    public bool IsShown => !Vm.IsRuneListShown;
    public void UpdateIsShown() => OnPropertyChanged(nameof(IsShown));
    
    [RelayCommand]
    private void Primary(string glyphs)
    {
        if (Vm.ViewModelSentences.Dialog.IsOpen)
        {
            Vm.ViewModelSentences.Dialog.InsertAtCursor(glyphs);
        }
        else
        {
            Dialog.Open(glyphs);
        }
    }

    [RelayCommand]
    private void Edit(string glyphs)
    {
        Dialog.Open(glyphs);
    }
    
    [RelayCommand]
    private static void Copy(string glyphs)
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        clipboard?.SetTextAsync(glyphs);
    }
    
    [RelayCommand]
    private void ClearFilters()
    {
        FilterText = string.Empty;
    }
    
    public bool TryGet(string word, [MaybeNullWhen(false)] out Phrase phrase)
    {
        return _allPhrases.TryGetValue(word, out phrase);
    }
    
    public void Add(IEnumerable<PhraseFlat> phrases)
    {
        foreach (PhraseFlat phraseFlat in phrases)
        {
            ImmutableArray<Element> elements = [..phraseFlat.Glyphs.Select(g => Vm.ViewModelElement[g] ?? Vm.ViewModelElement.AddOrEdit(g))];

            Add(new Phrase {Elements = elements, Translation = phraseFlat.Translation }, false);
        }
        
        OnPropertyChanged(nameof(Phrases));
    }
    
    private void Add(Phrase phrase, bool update = true)
    {
        if (_allPhrases.TryAdd(phrase.Glyphs, phrase))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Phrases));
            }

            return;
        }

        Console.WriteLine($"Phrase {phrase.Glyphs} already exists");
    }

    private Phrase Add(string glyphs, bool update = true)
    {
        if (TryGet(glyphs, out Phrase? existingPhrase))
        {
            return existingPhrase;
        }

        ImmutableArray<Element> elements = [..glyphs.Select(g => Vm.ViewModelElement[g] ?? Vm.ViewModelElement.AddOrEdit(g))];

        Phrase phrase = new() { Elements = elements};
        Add(phrase, update);
        return phrase;
    }

    public void Add(string[] glyphs)
    {
        foreach (string glyph in glyphs)
        {
            Add(glyph, false);
        }
        
        OnPropertyChanged(nameof(Phrases));
    }

    public void Edit(string glyphs, string newTranslation)
    {
        if (_allPhrases.TryGetValue(glyphs, out Phrase? phrase))
        {
            string oldTranslation = phrase.Translation;

            if (oldTranslation == newTranslation)
            {
                return;
            }

            phrase.Translation = newTranslation;

            OnPropertyChanged(nameof(Phrases));
        }
        else
        {
            Console.WriteLine($"Word {phrase?.Glyphs ?? "(null)"} does not exist");
        }
    }

    public void Remove(IEnumerable<string> glyphsSet)
    {
        foreach (string glyphs in glyphsSet)
        {
            Remove(glyphs, false);
        }
        
        OnPropertyChanged(nameof(Phrases));
    }

    public IEnumerable<PhraseBase> GetPhrases(string sentenceRaw)
    {
        return Sentence
            .Split(sentenceRaw)
            .Select(phrase => Element.IsElement(phrase.FirstOrDefault(' ')) 
                ? Add(phrase) 
                : new PhraseBase(phrase));
    }

    private void Remove(string glyphs, bool update = true)
    {
        if (!TryGet(glyphs, out Phrase? phrase))
        {
            return;
        }

        if (phrase.Translation.Trim() != "")
        {
            return;
        }

        _allPhrases.Remove(glyphs);

        if (update)
        {
            OnPropertyChanged(nameof(Phrases));
        }
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(FilterText):
                OnPropertyChanged(nameof(CanClearFilters));
                OnPropertyChanged(nameof(PhrasesFiltered));
                break;
            
            case nameof(Phrases):
                OnPropertyChanged(nameof(PhrasesFiltered));
                Vm.ViewModelSentences.Refresh();
                break;
        }
    }
}