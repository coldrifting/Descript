using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using Descript.Data;
using Descript.Models;
using Descript.Utils;
using Descript.ViewModels.Core;

namespace Descript.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public RunesListViewModel RunesList { get; }
    public TranslationListViewModel TranslationList { get; }
    
    public MainWindowViewModel()
    {
        RunesList = new RunesListViewModel(this);
        TranslationList = new TranslationListViewModel(this);
        LoadData();
    }

    private void LoadData()
    {
        RunesList.Add(DataManagement.Load<Rune>());
        TranslationList.AddWord(DataManagement.Load<RuneChain>());
        TranslationList.Add(DataManagement.Load<Translation>());
    }

    public void SaveData()
    {
        DataManagement.Save(RunesList.GetOrdered());
    }
    
    // Edit Rune Modal Dialog
    private bool _shouldDialogBeSubmitted;
    private int _runeEditId = -1;
    
    public List<ConfidenceLevel> ConfidenceLevels { get; } = [
        ConfidenceLevel.High,
        ConfidenceLevel.Medium,
        ConfidenceLevel.Low
    ];
    
    public bool IsDialogOpen { get; set => SetField(ref field, value); }
    public bool IsDialogFocused { get; set => SetField(ref field, value); }
    public string RuneEditNewTranslation { get; set => SetField(ref field, value.ToLower()); } = string.Empty;
    public ConfidenceLevel RuneEditNewConfidenceLevel { get; set => SetField(ref field, value); }
    public bool RuneEditNewTranslationOkay { get; set => SetField(ref field, value); }
    public CaseConversion TranslationCase => CaseConversion.Lowercase;

    [RelayCommand]
    private void OpenRuneEditDialog(int runeId)
    {
        RunesList.TryGet(runeId, out Rune? rune);
        if (rune == null)
        {
            return;
        }
        
        _runeEditId = runeId;
        _shouldDialogBeSubmitted = false;
        
        RuneEditNewTranslation = rune.Translation;
        RuneEditNewConfidenceLevel = rune.Confidence;
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
            RunesList.Edit(_runeEditId,  RuneEditNewTranslation, RuneEditNewConfidenceLevel);
        }
        IsDialogOpen = false;
    }
    
    // Runes List 
    
    [RelayCommand]
    private void ClearRuneListFilters()
    {
        RunesList.ClearFilters();
    }

    [RelayCommand]
    private void ToggleRuneListSortMode()
    {
        RunesList.ToggleSortMode();
    }

    [RelayCommand]
    private void AddRune(int runeId)
    {
        RunesList.Add(runeId);
    }
    
    [RelayCommand]
    private void DeleteRune(int runeId)
    {
        RunesList.Delete(runeId);
    }

    [RelayCommand]
    private static void CopyTextToClipboard(string text)
    {
        ClipboardHelper.GetClipboard()?.SetTextAsync(text);
    }

    private bool IsRuneEditOkay()
    {
        return RuneEditNewTranslation.Trim() != string.Empty || RuneEditNewConfidenceLevel == ConfidenceLevel.Low;
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
            case nameof(RuneEditNewTranslation) or nameof(RuneEditNewConfidenceLevel):
                RuneEditNewTranslationOkay = IsRuneEditOkay();
                break;
        }
    }
    

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }
        field = value;
        OnPropertyChanged(propertyName);
    }
}

