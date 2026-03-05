using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Descript.Data;
using Descript.Models;
using Descript.Utils;
using Descript.ViewModels.Core;

namespace Descript.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public RunesListViewModel RunesList { get; } = new();
    
    public MainWindowViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        RunesList.Add(DataManagement.Load<Rune>());
    }

    public void SaveData()
    {
        DataManagement.Save(RunesList.GetOrdered());
    }
    
    // Edit Rune Modal Dialog
    private bool _shouldDialogBeSubmitted;
    private int _runeEditId = -1;
    
    public List<ConfidenceLevel> ConfidenceLevels { get; } = [ ..Enum.GetValues<ConfidenceLevel>() ];
    
    [ObservableProperty]
    public partial bool IsDialogOpen { get; set; }

    [ObservableProperty]
    public partial bool IsDialogFocused { get; set; }
    
    [ObservableProperty]
    public partial string RuneEditNewTranslation { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool RuneEditNewTranslationOkay { get; private set; }

    [ObservableProperty]
    public partial ConfidenceLevel RuneEditNewConfidenceLevel { get; set; }
    
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
                if (!RuneEditNewTranslation.Equals(RuneEditNewTranslation.ToUpper(), StringComparison.CurrentCulture))
                {
                    RuneEditNewTranslation = RuneEditNewTranslation.ToUpper();
                }

                RuneEditNewTranslationOkay = RuneEditNewTranslation.Trim() != string.Empty ||
                                             RuneEditNewConfidenceLevel == ConfidenceLevel.Low;
                break;
        }
    }
}

