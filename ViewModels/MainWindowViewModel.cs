using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Descript.Data;
using Descript.Models;
using Descript.Utils;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public RunesViewModel Runes { get; }
    public WordsViewModel Words { get; }
    public TranslationsViewModel Translations { get; }
    
    public MainWindowViewModel()
    {
        Runes = new RunesViewModel(this);
        Words = new WordsViewModel(this);
        Translations = new TranslationsViewModel(this);
        LoadData();
    }

    private void LoadData()
    {
        Runes.Add(DataManagement.Load<Rune>());
        Words.Add(DataManagement.Load<Word>());
        Translations.Add(DataManagement.Load<Translation>());
    }

    public void SaveData()
    {
        DataManagement.Save(Runes.GetOrdered());
    }
    
    // Edit Rune Modal Dialog
    private bool _shouldDialogBeSubmitted;
    private int _dialogId = -1;
    
    public List<ConfidenceLevel> ConfidenceLevels { get; } = [
        ConfidenceLevel.High,
        ConfidenceLevel.Medium,
        ConfidenceLevel.Low
    ];
    
    public bool IsDialogOpen { get; set => SetField(ref field, value); }
    public bool IsDialogFocused { get; set => SetField(ref field, value); }
    
    public DialogType DialogType { get; set; } = DialogType.None;
    public string DialogTitle { get; set => SetField(ref field, value); } = string.Empty;
    
    public string DialogEntryTranslation { get; set => SetField(ref field, value.ToLower()); } = string.Empty;
    public ConfidenceLevel DialogEntryConfidence { get; set => SetField(ref field, value); }
    public bool IsDialogEntryValid { get; set => SetField(ref field, value); }
    
    public CaseConversion TranslationCase { get; set => SetField(ref field, value); }

    [RelayCommand]
    private void OpenRuneEditDialog(int runeId)
    {
        Runes.TryGet(runeId, out Rune? rune);
        if (rune == null)
        {
            return;
        }

        DialogTitle = "Input Rune Translation Guess";
        DialogType = DialogType.RuneEdit;
        TranslationCase = CaseConversion.Lowercase;
        _dialogId = runeId;
        _shouldDialogBeSubmitted = false;
        
        DialogEntryTranslation = rune.Translation;
        DialogEntryConfidence = rune.Confidence;
        IsDialogOpen = true;
        IsDialogFocused = true;
    }
    
    [RelayCommand]
    private void OpenWordEditDialog(int wordId)
    {
        Words.TryGet(wordId, out Word? rune);
        if (rune == null)
        {
            return;
        }

        DialogTitle = "Input Word Translation Guess";
        DialogType = DialogType.WordEdit;
        TranslationCase = CaseConversion.Titlecase;
        _dialogId = wordId;
        _shouldDialogBeSubmitted = false;
        
        DialogEntryTranslation = rune.Translation;
        DialogEntryConfidence = rune.Confidence;
        IsDialogOpen = true;
        IsDialogFocused = true;
    }
    
    [RelayCommand]
    private void SubmitDialog()
    {
        _shouldDialogBeSubmitted = true;
        IsDialogOpen = false;
    }
    
    private void CloseDialog()
    {
        if (_shouldDialogBeSubmitted)
        {
            _shouldDialogBeSubmitted = false;
            switch (DialogType)
            {
                case DialogType.RuneEdit:
                    Runes.Edit(_dialogId, DialogEntryTranslation, DialogEntryConfidence);
                    break;
                case DialogType.WordEdit:
                    Words.Edit(_dialogId, DialogEntryTranslation, DialogEntryConfidence);
                    break;
                case DialogType.None:
                default:
                    break;
            }
        }
        
        DialogType = DialogType.None;
        IsDialogOpen = false;
    }
    
    // Runes List 
    
    [RelayCommand]
    private void ClearRuneListFilters()
    {
        Runes.ClearFilters();
    }

    [RelayCommand]
    private void ToggleRuneListSortMode()
    {
        Runes.ToggleSortMode();
    }

    [RelayCommand]
    private void AddRune(int runeId)
    {
        Runes.Add(runeId);
    }
    
    [RelayCommand]
    private void DeleteRune(int runeId)
    {
        Runes.Delete(runeId);
    }

    [RelayCommand]
    private void CopyTextToClipboard(string text)
    {
        ClipboardHelper.GetClipboard()?.SetTextAsync(text);
    }

    [RelayCommand]
    private void Primary(string text)
    {
        if (Translations.IsSentenceDialogOpen)
        {
            Translations.InsertIntoSentenceInput(text);
        }
        else
        {
            OpenRuneEditDialog(text[0] - Rune.CodePointStart);
        }
    }

    private bool IsDialogValid()
    {
        return DialogEntryTranslation.Trim() != string.Empty || DialogEntryConfidence == ConfidenceLevel.Low;
    }

    public bool IsRuneListShown { get; set => SetField(ref field, value); } = true;
    
    [RelayCommand]
    private void ShowRunesList(bool shouldShowRunesList)
    {
        IsRuneListShown = shouldShowRunesList;
    }
    
    // Keep everything synced
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(IsDialogOpen) when !IsDialogOpen:
                CloseDialog();
                break;
            
            case nameof(IsDialogOpen) when IsDialogOpen:
            case nameof(DialogEntryTranslation) or nameof(DialogEntryConfidence):
                IsDialogEntryValid = IsDialogValid();
                break;
        }
    }
}

