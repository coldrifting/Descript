using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Descript.Data;
using Descript.Interfaces;
using Descript.Models;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public partial class ViewModelRuneSentence(ViewModelMainWindow viewModelMainWindow) : ViewModelBase, ILoadSave
{
    private ViewModelMainWindow Vm { get; } = viewModelMainWindow;

    private readonly Dictionary<string, RuneSentence> _translations = new();

    public string FilterText { get; set => SetField(ref field, value); } = string.Empty;
    public bool ShowFilterCancel => FilterText.Length > 0;
    
    private IEnumerable<RuneSentence> Translations => _translations.Values.OrderBy(s => s.Sentence);
    public IEnumerable<RuneSentenceExtended> TranslationsFiltered => _translations.Values
        .Where(rs => IsSentenceMatch(rs, FilterText))
        .Select(sentence => new RuneSentenceExtended
        {
            Sentence = sentence.Sentence,
            Category = sentence.Category,
            SubCategory = sentence.SubCategory,
            Context = sentence.Context,
            RuneChains = GetRuneChains(sentence.Sentence)
        });
    
    // Add Sentence Dialog
    private string _sentenceId = "";
    public bool IsSentenceDialogOpen { get; set => SetField(ref field, value); }
    public string SentenceEntry { get; set => SetField(ref field, value); } = "";
    public bool IsSentenceValid { get; set => SetField(ref field, value); }
    public int SelectionStart { get; set => SetField(ref field, value); }
    public int SelectionEnd { get; set => SetField(ref field, value); }

    public void Load()
    {
        Add(DataManagement.Load<RuneSentence>());
    }

    private ImmutableList<RuneChainExtended> GetRuneChains(string sentence)
    {
        return sentence
            .Split(' ')
            .Select(s => Vm.ViewModelRuneChain.Words.FirstOrDefault(runeChain => s.Equals(runeChain.Glyphs), new RuneChain { Glyphs = s, Confidence = ConfidenceLevel.Confirmed }))
            .Select(r => new RuneChainExtended
            {
                Glyphs = r.Glyphs,
                Confidence = r.Confidence,
                Translation = r.Translation,
                Runes = r.Glyphs
                    .ToCharArray()
                    .Select(c => Vm.ViewModelRune.Runes.FirstOrDefault(rx => c.Equals(rx.Glyph), new Rune { Glyph = c, Confidence = ConfidenceLevel.Confirmed } ))
                    .ToImmutableList()
            })
            .ToImmutableList();
    }

    public void Save()
    {
        DataManagement.Save(Translations);
    }

    [RelayCommand]
    private void OpenAddSentenceDialog()
    {
        _sentenceId = "";
        SentenceEntry = "";
        IsSentenceDialogOpen = true;
    }
    
    [RelayCommand]
    private void OpenEditSentenceDialog(string sentence)
    {
        _sentenceId = sentence;
        SentenceEntry = sentence;
        IsSentenceDialogOpen = true;
    }

    [RelayCommand]
    private void CancelDialog()
    {
        IsSentenceDialogOpen = false;
    }

    [RelayCommand]
    private void ClearFilterText()
    {
        FilterText = string.Empty;
        OnPropertyChanged(nameof(TranslationsFiltered));
    }

    public void InsertIntoSentenceInput(string input)
    {
        if (SelectionStart == SelectionEnd)
        {
            SentenceEntry = SentenceEntry.Insert(Math.Min(SelectionStart, SentenceEntry.Length), input);
            SelectionEnd = SelectionStart + 1;
            SelectionStart = SelectionEnd;
        }
        else
        {
            SentenceEntry = SelectionEnd > SelectionStart
                ? SentenceEntry.Remove(SelectionStart, SelectionEnd - SelectionStart).Insert(SelectionStart, input)
                : SentenceEntry.Remove(SelectionEnd, SelectionStart - SelectionEnd).Insert(SelectionEnd, input);
            
            SelectionStart = Math.Min(SelectionStart, SelectionEnd) + 1;
            SelectionEnd = SelectionStart;
        }
    }

    [RelayCommand]
    private void SubmitSentence()
    {
        IsSentenceDialogOpen = false;
        
        RuneSentence t = ConvertFromSentence(SentenceEntry.Trim());

        _translations.Add(t.Sentence, t);
        
        // Remove old sentence if editing
        if (_sentenceId != t.Sentence)
        {
            _translations.Remove(_sentenceId);
            foreach (string se in _sentenceId.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!t.Sentence.Contains(se))
                {
                    Vm.ViewModelRuneChain.Remove(se);
                }
            }
        }
        
        OnPropertyChanged(nameof(TranslationsFiltered));
    }
    
    private RuneSentence ConvertFromSentence(string sentence)
    {
        foreach (string str in sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (ViewModelRuneChain.IsValidWord(str))
            {
                Vm.ViewModelRuneChain.Add(str);
            }

        }

        return RuneSentence.FromString(sentence);
    }

    private bool IsSentenceMatch(RuneSentence runeSentence, string sentence)
    {
        string currentTranslations = runeSentence.Sentence
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Vm.ViewModelRuneChain[s]?.Translation ?? "")
            .Aggregate((a, b) => a + b);
        
        return sentence.ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(filterEntry => 
                runeSentence.Sentence.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) || 
                currentTranslations.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public bool Add(RuneSentence runeSentence, bool update = true)
    {
        if (_translations.TryAdd(runeSentence.Sentence, runeSentence))
        {
            if (update)
            {
                OnPropertyChanged(nameof(TranslationsFiltered));
            }

            return true;
        }

        Console.WriteLine("Translation already exists");
        return false;
    }

    public void Add(IEnumerable<RuneSentence> translations)
    {
        bool updated = false;
        foreach (RuneSentence translation in translations)
        {
            if (Add(translation, false))
            {
                updated = true;
            }
        }

        if (updated)
        {
            OnPropertyChanged(nameof(TranslationsFiltered));
        }
    }

    [RelayCommand]
    private void DeleteSentence(string sentence)
    {
        // Remove words with no added info
        foreach (string se in sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            Vm.ViewModelRuneChain.Remove(se);
        }
        
        _translations.Remove(sentence);
        OnPropertyChanged(nameof(TranslationsFiltered));
    }

    private bool IsSentenceOkay()
    {
        return !Translations
            .Select(runeSentence => runeSentence.Sentence)
            .Contains(SentenceEntry.Trim()) && SentenceEntry.Trim().Length != 0;
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(FilterText))
        {
            OnPropertyChanged(nameof(ShowFilterCancel));
            OnPropertyChanged(nameof(TranslationsFiltered));
        }
        
        if (e.PropertyName is nameof(SentenceEntry))
        {
            IsSentenceValid = IsSentenceOkay();
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(TranslationsFiltered));
    }
}