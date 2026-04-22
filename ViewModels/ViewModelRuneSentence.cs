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
    public string FilterContextText { get; set => SetField(ref field, value); } = string.Empty;
    public bool ShowFilterCancel => FilterText.Length > 0;
    public bool ShowFilterContextCancel => FilterContextText.Length > 0;
    
    private IEnumerable<RuneSentence> Translations => _translations.Values.OrderBy(s => s.Sentence);
    public IEnumerable<RuneSentenceExtended> TranslationsFiltered => _translations.Values
        .Where(rs => IsSentenceMatch(rs, FilterText))
        .Where(rs => IsContextMatch(rs, FilterContextText))
        .Select(sentence => new RuneSentenceExtended
        {
            Sentence = sentence.Sentence,
            Category = sentence.Category,
            SubCategory = sentence.SubCategory,
            Context = sentence.Context,
            RuneChains = GetRuneChains(sentence.Sentence)
        });
    
    // Add Sentence Dialog
    public RuneSentenceEdit SentenceEntry { get; set => SetField(ref field, value); } = new();
    
    public bool IsSentenceDialogOpen { get; set => SetField(ref field, value); }
    public int SelectionStart { get; set => SetField(ref field, value); }
    public int SelectionEnd { get; set => SetField(ref field, value); }

    private const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

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
        SentenceEntry = new RuneSentenceEdit
        {
            AllSentences = _translations.Values.Select(rs => rs.Sentence)
        };
        IsSentenceDialogOpen = true;
    }
    
    [RelayCommand]
    private void OpenEditSentenceDialog(string sentence)
    {
        SentenceEntry = new RuneSentenceEdit
        {
            AllSentences = _translations.Values.Select(rs => rs.Sentence),
            
            OriginalSentence = sentence,
            OriginalCategory = _translations[sentence].Category,
            OriginalSubCategory = _translations[sentence].SubCategory,
            OriginalContext = _translations[sentence].Context,
            
            Sentence = sentence,
            Category = _translations[sentence].Category,
            SubCategory = _translations[sentence].SubCategory,
            Context = _translations[sentence].Context
        };
        
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

    [RelayCommand]
    private void ClearFilterContextText()
    {
        FilterContextText = string.Empty;
        OnPropertyChanged(nameof(TranslationsFiltered));
    }

    public void InsertIntoSentenceInput(string input)
    {
        if (SelectionStart == SelectionEnd)
        {
            SentenceEntry.Sentence =
                SentenceEntry.Sentence.Insert(Math.Min(SelectionStart, SentenceEntry.Sentence.Length), input);
            
            SelectionEnd = SelectionStart + 1;
            SelectionStart = SelectionEnd;
        }
        else
        {
            SentenceEntry.Sentence = SelectionEnd > SelectionStart
                ? SentenceEntry.Sentence.Remove(SelectionStart, SelectionEnd - SelectionStart).Insert(SelectionStart, input)
                : SentenceEntry.Sentence.Remove(SelectionEnd, SelectionStart - SelectionEnd).Insert(SelectionEnd, input);

            SelectionStart = Math.Min(SelectionStart, SelectionEnd) + 1;
            SelectionEnd = SelectionStart;
        }
    }

    [RelayCommand]
    private void SubmitSentence()
    {
        IsSentenceDialogOpen = false;

        RuneSentence runeSentence = SentenceEntry.ToRuneSentence();

        // Just update category and context
        if (SentenceEntry.OriginalSentence.Trim() == SentenceEntry.Sentence.Trim())
        {
            _translations[runeSentence.Sentence] = runeSentence;
        }
        else
        {
            _translations.Add(runeSentence.Sentence, runeSentence);
            foreach (string str in runeSentence.Sentence.Split(' ', SplitOptions))
            {
                Vm.ViewModelRuneChain.Add(str);
            }

            // Remove old sentence and unused rune chains if editing
            _translations.Remove(SentenceEntry.OriginalSentence);
            foreach (string se in SentenceEntry.OriginalSentence.Split(' ', SplitOptions))
            {
                if (!runeSentence.Sentence.Contains(se))
                {
                    Vm.ViewModelRuneChain.Remove(se);
                }
            }
        }

        OnPropertyChanged(nameof(TranslationsFiltered));
    }

    private bool IsSentenceMatch(RuneSentence runeSentence, string sentence)
    {
        string currentTranslations = runeSentence.Sentence
            .Split(' ', SplitOptions)
            .Select(s => Vm.ViewModelRuneChain[s]?.Translation ?? "")
            .Aggregate((a, b) => a + b);
        
        return sentence.ToLower()
            .Split(' ', SplitOptions)
            .All(filterEntry => 
                runeSentence.Sentence.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) || 
                currentTranslations.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase));
    }

    private static bool IsContextMatch(RuneSentence runeSentence, string context)
    {
        return context.ToLower()
            .Split(' ', SplitOptions)
            .All(filterEntry =>
                runeSentence.Category.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) ||
                runeSentence.SubCategory.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase) ||
                runeSentence.Context.ToLower().Contains(filterEntry, StringComparison.CurrentCultureIgnoreCase));
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
        foreach (string se in sentence.Split(' ', SplitOptions))
        {
            Vm.ViewModelRuneChain.Remove(se);
        }
        
        _translations.Remove(sentence);
        OnPropertyChanged(nameof(TranslationsFiltered));
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(FilterText):
                OnPropertyChanged(nameof(ShowFilterCancel));
                OnPropertyChanged(nameof(TranslationsFiltered));
                break;
            case nameof(FilterContextText):
                OnPropertyChanged(nameof(ShowFilterContextCancel));
                OnPropertyChanged(nameof(TranslationsFiltered));
                break;
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(TranslationsFiltered));
    }
}