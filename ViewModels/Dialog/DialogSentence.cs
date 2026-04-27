using System;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.Models.Flat;
using Descript.ViewModels.Base;

namespace Descript.ViewModels.Dialog;

public partial class DialogSentence(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    
    public bool IsOpen { get; set => SetField(ref field, value); }
    public string Title { get; set => SetField(ref field, value); } = string.Empty;

    private string[] _allSentences = [];

    public bool IsValid =>
        Sentence.Trim().Length > 0 &&
        (_originalSentence != null && _originalSentence.Trim() != Sentence.Trim()
            ? !Enumerable.Contains(_allSentences, Sentence.Trim())
            : (_originalSentence != null && _originalSentence.Trim() != Sentence.Trim()) ||
              _originalCategory.Trim() != Category.Trim() ||
              _originalSubCategory.Trim() != SubCategory.Trim() ||
              _originalContext.Trim() != Context.Trim());

    private string? _originalSentence = string.Empty;
    private string _originalCategory = string.Empty;
    private string _originalSubCategory = string.Empty;
    private string _originalContext = string.Empty;
    
    public string Sentence { get; set => SetField(ref field, value); } = string.Empty;
    public string Category { get; set => SetField(ref field, value); } = string.Empty;
    public string SubCategory { get; set => SetField(ref field, value); } = string.Empty;
    public string Context { get; set => SetField(ref field, value); } = string.Empty;
    
    public int SelectionStart { get; set => SetField(ref field, value); }
    public int SelectionEnd { get; set => SetField(ref field, value); }

    public string SubmitButtonText => _originalSentence is null ? "Add" : "Update";
    
    [RelayCommand]
    private void Open(string? sentenceRaw)
    {
        _allSentences = Vm.ViewModelSentences.Sentences.Select(s => s.OriginalSentence).ToArray();
        Vm.ViewModelSentences.TryGet(sentenceRaw ?? "", out Sentence? sentence);
        sentence ??= new Sentence { OriginalSentence = "", Phrases = []};
        
        _originalSentence = sentence.OriginalSentence;
        _originalCategory = sentence.Category;
        _originalSubCategory = sentence.SubCategory;
        _originalContext = sentence.Context;
        
        Sentence = sentence.OriginalSentence;
        Category = sentence.Category;
        SubCategory = sentence.SubCategory;
        Context = sentence.Context;
        
        Title = "Input Sentence Translation Guess";
        IsOpen = true;
    }
    
    [RelayCommand]
    private void Submit()
    {
        SentenceFlat sentence = new()
        {
            Sentence = Sentence.Trim(),
            Category = Category.Trim(),
            SubCategory = SubCategory.Trim(),
            Context = Context.Trim()
        };
        
        Vm.ViewModelSentences.Edit(_originalSentence ?? Sentence, sentence);
        
        IsOpen = false;
    }
    
    [RelayCommand]
    private void Cancel()
    {
        IsOpen = false;
    }
    
    public void InsertAtCursor(string input)
    {
        if (SelectionStart == SelectionEnd)
        {
            Sentence =
                Sentence.Insert(Math.Min(SelectionStart, Sentence.Length), input);
            
            SelectionEnd = SelectionStart + 1;
            SelectionStart = SelectionEnd;
        }
        else
        {
            Sentence = SelectionEnd > SelectionStart
                ? Sentence.Remove(SelectionStart, SelectionEnd - SelectionStart).Insert(SelectionStart, input)
                : Sentence.Remove(SelectionEnd, SelectionStart - SelectionEnd).Insert(SelectionEnd, input);

            SelectionStart = Math.Min(SelectionStart, SelectionEnd) + 1;
            SelectionEnd = SelectionStart;
        }
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(Sentence) or nameof(Category) or nameof(SubCategory) or nameof(Context))
        {
            OnPropertyChanged(nameof(IsValid));
        }
    }
}