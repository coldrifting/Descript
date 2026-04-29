using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.Models.Flat;
using Descript.Utils;
using Descript.ViewModels.Base;
using Descript.ViewModels.Dialog;
using Descript.ViewModels.Input;

namespace Descript.ViewModels;

public partial class ViewModelSentences : ViewModelBase
{
    public ViewModelSentences(MainWindowViewModel mainWindowViewModel)
    {
        Vm = mainWindowViewModel;
        ViewModelDialog = new ViewModelDialogSentence(mainWindowViewModel);
        ViewModelElementInput = new ViewModelElementInput(mainWindowViewModel, InsertAtCursor);
    }
    
    private MainWindowViewModel Vm { get; }
    public ViewModelDialogSentence ViewModelDialog { get; }

    public ViewModelElementInput ViewModelElementInput { get; }
    
    private readonly Dictionary<string, Sentence> _sentences = new();

    public string FilterText { get; set => SetField(ref field, value); } = string.Empty;
    public int FilterTextSelectionStart { get; set => SetField(ref field, value); }
    public int FilterTextSelectionEnd { get; set => SetField(ref field, value); }
    
    public string FilterContextText { get; set => SetField(ref field, value); } = string.Empty;
    
    public bool ShowFilterCancel => FilterText.Length > 0;
    public bool ShowFilterContextCancel => FilterContextText.Length > 0;
    
    public SentenceSortMode SortMode { get; set => SetField(ref field, value); } = SentenceSortMode.ByCategory;

    public IEnumerable<string> AllOriginalSentences => _sentences.Values.Select(s => s.SentenceOriginal);
    
    public IEnumerable<Sentence> Sentences => _sentences.Values;
    public IEnumerable<Sentence> SentencesFiltered => _sentences.Values
        .Where(sentence => Sentence.Matches(sentence, FilterText, FilterContextText))
        .OrderBy(SortMode);
    
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

    [RelayCommand]
    private void DeleteSentence(string sentenceRaw)
    {
        // Remove words with no added info
        RemovePhrases(sentenceRaw);
        _sentences.Remove(sentenceRaw);
        
        OnPropertyChanged(nameof(Sentences));
    }

    public void Add(IEnumerable<SentenceFlat> sentences)
    {
        foreach (SentenceFlat sentenceFlat in sentences)
        {
            Add(sentenceFlat, false);
        }

        OnPropertyChanged(nameof(Sentences));
    }

    public void Edit(SentenceFlat sentenceFlat, string? originalRawSentence)
    {
        if (originalRawSentence == sentenceFlat.Sentence)
        {
            _sentences[sentenceFlat.Sentence].Category = sentenceFlat.Category;
            _sentences[sentenceFlat.Sentence].SubCategory = sentenceFlat.SubCategory;
            _sentences[sentenceFlat.Sentence].Context = sentenceFlat.Context;
        }
        else
        {
            if (originalRawSentence is not null)
            {
                RemovePhrases(originalRawSentence);
                _sentences.Remove(originalRawSentence);
            }
            Add(sentenceFlat);
        }
        
        OnPropertyChanged(nameof(Sentences));
    }

    public SentenceFlat? GetFlattened(string sentenceRaw)
    {
        return _sentences.TryGetValue(sentenceRaw, out Sentence? sentence) 
            ? SentenceFlat.FromSentence(sentence) 
            : null;
    }
    
    public void InsertAtCursor(string input)
    {
        CursorHelper.InsertAtCursor(input, 
            FilterTextSelectionStart, 
            FilterTextSelectionEnd, 
            FilterText, 
            i => FilterTextSelectionStart = i, 
            i => FilterTextSelectionEnd = i, 
            s => FilterText = s);
    }

    private void Add(SentenceFlat sentenceFlat, bool update = true)
    {
        if (_sentences.TryAdd(sentenceFlat.Sentence, sentenceFlat.ToSentence(Vm.ViewModelPhrases)))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Sentences));
            }

            return;
        }

        Console.WriteLine($"Sentence {sentenceFlat.Sentence} already exists");
    }

    private void RemovePhrases(string sentenceRaw)
    {
        if (!_sentences.TryGetValue(sentenceRaw, out Sentence? sentence))
        {
            return;
        }

        IEnumerable<string> glyphsSet = sentence.Phrases
            .OfType<Phrase>()
            .Where(oldPhrase => oldPhrase is { Translation: "" })
            .Select(s => s.Glyphs);
        
        Vm.ViewModelPhrases.Remove(glyphsSet);
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