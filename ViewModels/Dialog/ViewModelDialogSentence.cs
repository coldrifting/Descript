using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.Models.Flat;
using Descript.Utils;
using Descript.ViewModels.Base;

namespace Descript.ViewModels.Dialog;

public partial class ViewModelDialogSentence(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    
    public bool IsOpen { get; set => SetField(ref field, value); }
    public string Title { get; set => SetField(ref field, value); } = string.Empty;
    public string ErrorMessage { get; private set => SetField(ref field, value); } = string.Empty;

    private IEnumerable<string> _allSentences = [];

    public bool IsValid => ValidateSentence();

    private string? _originalSentence;
    private string? _originalCategory;
    private string? _originalSubCategory;
    private string? _originalContext;

    public string Sentence { get; set => SetField(ref field, value); } = string.Empty;
    public string Category { get; set => SetField(ref field, value); } = string.Empty;
    public string SubCategory { get; set => SetField(ref field, value); } = string.Empty;
    public string Context { get; set => SetField(ref field, value); } = string.Empty;
    
    public int SelectionStart { get; set => SetField(ref field, value); }
    public int SelectionEnd { get; set => SetField(ref field, value); }
    
    public string SubmitButtonText => _originalSentence is null ? "Add" : "Update";
    
    public ElementInputMode ElementInputMode { get; set => SetField(ref field, value); }
    public Action<string> InsertAtCursor => 
        input => CursorHelper.InsertAtCursor(input, 
            SelectionStart, 
            SelectionEnd, 
            Sentence, 
            i => SelectionStart = i, 
            i => SelectionEnd = i, 
            s => Sentence = s);
    
    [RelayCommand]
    private void Open(string? sentenceRaw)
    {
        _allSentences = Vm.ViewModelSentences.AllOriginalSentences;
        
        if (sentenceRaw is null)
        {
            _originalSentence = null;
            _originalCategory = null;
            _originalSubCategory = null;
            _originalContext = null;
            
            Sentence = "";
            Category = "";
            SubCategory = "";
            Context = "";
        }
        else
        {
            SentenceFlat? sentenceFlat = Vm.ViewModelSentences.GetFlattened(sentenceRaw);
            
            _originalSentence = sentenceFlat?.Sentence;
            _originalCategory = sentenceFlat?.Category;
            _originalSubCategory = sentenceFlat?.SubCategory;
            _originalContext = sentenceFlat?.Context;
            
            Sentence = sentenceFlat?.Sentence ?? "";
            Category = sentenceFlat?.Category ?? "";
            SubCategory = sentenceFlat?.SubCategory ?? "";
            Context = sentenceFlat?.Context ?? "";
        }
        
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
        
        Vm.ViewModelSentences.Edit(sentence, _originalSentence);
        
        IsOpen = false;
    }
    
    [RelayCommand]
    private void Cancel()
    {
        IsOpen = false;
    }
    
    [RelayCommand]
    private void ToggleElementInputModeByRune()
    {
        ElementInputMode = ElementInputMode != ElementInputMode.Rune 
            ? ElementInputMode.Rune 
            : ElementInputMode.None;

        if (ElementInputMode is ElementInputMode.Rune or ElementInputMode.Translation)
        {
            Vm.ShowRunesListCommand.Execute(true);
        }
    }
    
    [RelayCommand]
    private void ToggleElementInputModeByTranslation()
    {
        ElementInputMode = ElementInputMode != ElementInputMode.Translation
            ? ElementInputMode.Translation
            : ElementInputMode.None;
               
        if (ElementInputMode is ElementInputMode.Rune or ElementInputMode.Translation)
        {
            Vm.ShowRunesListCommand.Execute(true);
        }
    }
    
    [RelayCommand]
    private void ToggleElementInputModeByWord()
    {
        ElementInputMode = ElementInputMode != ElementInputMode.Word 
            ? ElementInputMode.Word 
            : ElementInputMode.None;
        
        if (ElementInputMode is ElementInputMode.Word)
        {
            Vm.ShowRunesListCommand.Execute(false);
        }
    }

    private bool ValidateSentence()
    {
        if (Sentence.Trim().Length == 0)
        {
            ErrorMessage = "Sentence cannot be empty";
            return false;
        }
        
        bool sentenceEqual = string.Equals(_originalSentence?.Trim(), Sentence.Trim(), StringComparison.Ordinal);
        bool categoryEqual = string.Equals(_originalCategory?.Trim(), Category.Trim(), StringComparison.Ordinal);
        bool subCategoryEqual = string.Equals(_originalSubCategory?.Trim(), SubCategory.Trim(), StringComparison.Ordinal);
        bool contextEqual = string.Equals(_originalContext?.Trim(), Context.Trim(), StringComparison.Ordinal);

        switch (sentenceEqual)
        {
            case true when categoryEqual && subCategoryEqual && contextEqual:
                ErrorMessage = "Sentence Info has not changed";
                return false;
            case false when _allSentences.Contains(Sentence.Trim()):
                ErrorMessage = "Sentence Already Exists";
                return false;
            default:
                ErrorMessage = "";
                return true;
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