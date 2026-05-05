using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.Models.Flat;
using Descript.Utils;
using Descript.ViewModels.Base;
using Descript.ViewModels.Dialog;

namespace Descript.ViewModels;

public partial class ViewModelSentences(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    public MainWindowViewModel Vm { get; } = mainWindowViewModel;
    public ViewModelDialogSentence ViewModelDialog { get; } = new(mainWindowViewModel);
    
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
    
    public ElementInputMode ElementInputMode { get; set => SetField(ref field, value); }
    public string ElementInputModeText =>
        ElementInputMode switch
        {
            ElementInputMode.Shape => "Shape",
            ElementInputMode.Element => "Rune",
            ElementInputMode.Phrase => "Phrase",
            _ => ""
        };
    public IImmutableSolidColorBrush ElementInputModeColor =>
        ElementInputMode switch
        {
            ElementInputMode.Shape => Brushes.Orange,
            ElementInputMode.Element => Brushes.DarkCyan,
            ElementInputMode.Phrase => Brushes.DodgerBlue,
            _ => Brushes.Gray
        };

    
    public Action<string> InsertAtCursor => 
        input => CursorHelper.InsertAtCursor(input, 
            FilterTextSelectionStart, 
            FilterTextSelectionEnd, 
            FilterText, 
            i => FilterTextSelectionStart = i, 
            i => FilterTextSelectionEnd = i, 
            s => FilterText = s);
    
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
    
    [RelayCommand]
    private void ToggleElementInputModeByShape()
    {
        ElementInputMode = ElementInputMode != ElementInputMode.Shape 
            ? ElementInputMode.Shape 
            : ElementInputMode.None;

        if (ElementInputMode is ElementInputMode.Shape)
        {
            Vm.ShowElementsListCommand.Execute(true);
        }
    }
    
    [RelayCommand]
    private void ToggleElementInputModeByElement()
    {
        ElementInputMode = ElementInputMode != ElementInputMode.Element
            ? ElementInputMode.Element
            : ElementInputMode.None;
               
        if (ElementInputMode is ElementInputMode.Element)
        {
            Vm.ShowElementsListCommand.Execute(true);
        }
    }
    
    [RelayCommand]
    private void ToggleElementInputModeByPhrase()
    {
        ElementInputMode = ElementInputMode != ElementInputMode.Phrase 
            ? ElementInputMode.Phrase 
            : ElementInputMode.None;
        
        if (ElementInputMode is ElementInputMode.Phrase)
        {
            Vm.ShowElementsListCommand.Execute(false);
        }
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
            case nameof(ElementInputMode):
                OnPropertyChanged(nameof(ElementInputModeText));
                OnPropertyChanged(nameof(ElementInputModeColor));
                break;
            
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