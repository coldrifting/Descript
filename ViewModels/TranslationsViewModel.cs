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

public partial class TranslationsViewModel(MainWindowViewModel mainWindowViewModel) : ViewModelBase, ILoadSave
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;

    private readonly Dictionary<string, RuneSentence> _allTranslations = new();

    public IEnumerable<RuneSentenceExtended> Translations => _allTranslations.Values.Select(sentence => 
        new RuneSentenceExtended
        {
            Sentence = sentence.Sentence,
            Category = sentence.Category,
            SubCategory = sentence.SubCategory,
            Context = sentence.Context,
            RuneChains = GetRuneChains(sentence.Sentence)
        });
    private IEnumerable<RuneSentence> TranslationsOrdered => _allTranslations.Values.OrderBy(s => s.Sentence);
    
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
            .Select(s => Vm.Words.Words.FirstOrDefault(runeChain => s.Equals(runeChain.Glyphs), new RuneChain { Glyphs = s, Confidence = ConfidenceLevel.Confirmed }))
            .Select(r => new RuneChainExtended
            {
                Glyphs = r.Glyphs,
                Confidence = r.Confidence,
                Translation = r.Translation,
                Runes = r.Glyphs
                    .ToCharArray()
                    .Select(c => Vm.Runes.Runes.FirstOrDefault(rx => c.Equals(rx.Glyph), new Rune { Glyph = c, Confidence = ConfidenceLevel.Confirmed } ))
                    .ToImmutableList()
            })
            .ToImmutableList();
    }

    public void Save()
    {
        DataManagement.Save(TranslationsOrdered);
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

        _allTranslations.Add(t.Sentence, t);
        
        // Remove old sentence if editing
        if (_sentenceId != t.Sentence)
        {
            _allTranslations.Remove(_sentenceId);
            foreach (string se in _sentenceId.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!t.Sentence.Contains(se))
                {
                    Vm.Words.Remove(se);
                }
            }
        }
        
        OnPropertyChanged(nameof(Translations));
    }
    
    private RuneSentence ConvertFromSentence(string sentence)
    {
        foreach (string str in sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (WordsViewModel.IsValidWord(str))
            {
                Vm.Words.Add(str);
            }

        }

        return RuneSentence.FromString(sentence);
    }
    
    public bool Add(RuneSentence runeSentence, bool update = true)
    {
        if (_allTranslations.TryAdd(runeSentence.Sentence, runeSentence))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Translations));
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
            OnPropertyChanged(nameof(Translations));
        }
    }

    [RelayCommand]
    private void DeleteSentence(string sentence)
    {
        // Remove words with no added info
        foreach (string se in sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            Vm.Words.Remove(se);
        }
        
        _allTranslations.Remove(sentence);
        OnPropertyChanged(nameof(Translations));
    }

    private bool IsSentenceOkay()
    {
        return !Translations
            .Select(runeSentence =>  runeSentence.Sentence)
            .Contains(SentenceEntry.Trim()) && SentenceEntry.Trim().Length != 0;
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(SentenceEntry))
        {
            IsSentenceValid = IsSentenceOkay();
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(Translations));
    }
}