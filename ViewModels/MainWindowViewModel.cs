using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
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
        Runes.Load();
        Words.Load();
        Translations.Load();
    }

    public void SaveData()
    {
        Runes.Save();
        Words.Save();
        Translations.Save();
    }
    
    // Edit Rune Modal Dialog
    private bool _shouldDialogBeSubmitted;
    private char _dialogRuneEdit = (char)0;
    private string _dialogWordEdit = "";
    
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
    private void OpenRuneEditDialog(char glyph)
    {
        Runes.TryGet(glyph, out Rune? rune);
        if (rune == null)
        {
            return;
        }

        DialogTitle = "Input Rune Translation Guess";
        DialogType = DialogType.RuneEdit;
        TranslationCase = CaseConversion.Lowercase;
        _dialogRuneEdit = glyph;
        _shouldDialogBeSubmitted = false;
        
        DialogEntryTranslation = rune.Translation;
        DialogEntryConfidence = rune.Confidence;
        IsDialogOpen = true;
        IsDialogFocused = true;
    }
    
    [RelayCommand]
    private void OpenWordEditDialog(string wordRaw)
    {
        Words.TryGet(wordRaw, out RuneChain? rune);
        if (rune == null)
        {
            return;
        }

        DialogTitle = "Input Word Translation Guess";
        DialogType = DialogType.WordEdit;
        TranslationCase = CaseConversion.Titlecase;
        _dialogWordEdit = wordRaw;
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

    [RelayCommand]
    private void CancelDialog()
    {
        DialogType = DialogType.None;
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
                    Runes.Edit(_dialogRuneEdit, DialogEntryTranslation, DialogEntryConfidence);
                    break;
                case DialogType.WordEdit:
                    Words.Edit(_dialogWordEdit, DialogEntryTranslation, DialogEntryConfidence);
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
    private void AddRune(char glyph)
    {
        Runes.Add(glyph);
    }
    
    [RelayCommand]
    private void DeleteRune(char glyph)
    {
        Runes.Delete(glyph);
    }

    [RelayCommand]
    private void CopyTextToClipboard(char glyph)
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        if (clipboard is not null)
        {
             clipboard.SetTextAsync(glyph.ToString());
        }
    }

    [RelayCommand]
    private void Primary(char glyph)
    {
        if (Translations.IsSentenceDialogOpen)
        {
            Translations.InsertIntoSentenceInput(glyph.ToString());
        }
        else
        {
            OpenRuneEditDialog(glyph);
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

