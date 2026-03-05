using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.ViewModels.Core;

namespace Descript.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        AddTestRunes();
    }

    // TODO - Replace with proper serialized json test data
    private void AddTestRunes()
    {
        for (int i = 1; i <= 48; i++)
        {
            SelectionFilter = i;
            AddRune();

            string translation = "";
            int length = Random.Shared.Next(1, 3);
            for (int j = 0; j < length; j++)
            {
                char character = (char)(Random.Shared.Next(0, 26) + 'A');
                translation += $"{character}";
            }

            _savedRunes[i].Translation = translation;
            _savedRunes[i].Confidence = (ConfidenceLevel)Random.Shared.Next(3);
        }
        
        SelectionFilter = 0;
    }
    
    private readonly Dictionary<int, Rune> _savedRunes = new();
    private bool _shouldDialogBeSubmitted;
    private int _runeEditId = -1;
    
    [ObservableProperty]
    public partial int SelectionFilter { get; set; }
    
    [ObservableProperty]
    public partial RuneSortMode RuneSortMode { get; private set; } = RuneSortMode.ById;

    [ObservableProperty]
    public partial string RuneSortModeString { get; set; } = "By Id";

    [ObservableProperty]
    public partial string RuneFilterText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<Rune> FilteredRunes { get; set; } = [];
    
    [ObservableProperty]
    public partial bool CanAddRune { get; private set; }
    
    [ObservableProperty]
    public partial bool CanClearSelection { get; private set; }
    
    [ObservableProperty]
    public partial bool IsDialogOpen { get; set; }

    [ObservableProperty]
    public partial bool IsDialogFocused { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ConfidenceLevel> ConfidenceLevels { get; set; } = new(Enum
        .GetValues(typeof(ConfidenceLevel))
        .Cast<ConfidenceLevel>());
    
    [ObservableProperty]
    public partial string RuneEditNewTranslation { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool RuneEditNewTranslationOkay { get; private set; }

    [ObservableProperty]
    public partial ConfidenceLevel RuneEditNewConfidenceLevel { get; set; }

    [RelayCommand]
    private void OpenRuneEditDialog(Rune rune)
    {
        _runeEditId = rune.Id;
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
            if (_savedRunes.TryGetValue(_runeEditId, out Rune? rune))
            {
                rune.Translation = RuneEditNewTranslation.Trim();
                rune.Confidence = RuneEditNewConfidenceLevel;
            }
        }
        IsDialogOpen = false;
    }
    
    [RelayCommand]
    private void ClearSelection()
    {
        SelectionFilter = 0;
        RuneFilterText = string.Empty;
    }
    
    [RelayCommand]
    private void AddRune()
    {
        if (_savedRunes.TryGetValue(SelectionFilter, out Rune? rune))
        {
            Console.WriteLine($"Rune {rune.Id} ({rune.Glyph}) already exists!");
            return;
        }
        
        _savedRunes.Add(SelectionFilter, new Rune(SelectionFilter));
        
        ApplyRuneFilter();
    }

    [RelayCommand]
    private void DeleteRune(Rune rune)
    {
        _savedRunes.Remove(rune.Id);
        
        ApplyRuneFilter();
    }

    [RelayCommand]
    private void ToggleSortMode()
    {
        RuneSortMode = RuneSortMode switch
        {
            RuneSortMode.ByTranslation => RuneSortMode.ByConfidence,
            RuneSortMode.ByConfidence => RuneSortMode.ById,
            RuneSortMode.ById => RuneSortMode.ByTranslation,
            _ => RuneSortMode
        };
        
        RuneSortModeString = RuneSortMode.ToString().Replace("By", "By ");
    }
    
    [RelayCommand]
    private void ApplyRuneFilter()
    {
        IComparer<Rune> sortMethod = RuneSortMode switch
        {
            RuneSortMode.ByTranslation => new RuneByTranslationComparer(SelectionFilter),
            RuneSortMode.ByConfidence => new RuneByConfidenceComparer(SelectionFilter),
            _ => new RuneByIdComparer(SelectionFilter)
        };
        
        List<Rune> runes = _savedRunes.Values
            .Where(r => IsFilterMatch(r.Id, SelectionFilter) && 
                        r.Translation.Contains(RuneFilterText.Trim(), StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(r => r, sortMethod)
            .ToList();
        
        FilteredRunes = new ObservableCollection<Rune>(runes);
        CanClearSelection = CanClearSelectionCondition();
        CanAddRune = CanAddRuneCondition();
    }
    
    // Where the magic happens
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(RuneFilterText):
                if (!RuneFilterText.Equals(RuneFilterText, StringComparison.CurrentCultureIgnoreCase))
                {
                    RuneFilterText = RuneEditNewTranslation.ToUpper();
                }
                
                ApplyRuneFilter();
                break;
            
            case nameof(SelectionFilter):
            case nameof(RuneSortMode):
                ApplyRuneFilter();
                break;

            case nameof(IsDialogOpen) when !IsDialogOpen:
                CloseDialog();
                break;
            
            case nameof(IsDialogOpen) when IsDialogOpen:
            case nameof(RuneEditNewTranslation) or nameof(RuneEditNewConfidenceLevel):
                if (!RuneEditNewTranslation.Equals(RuneEditNewTranslation.ToUpper(), StringComparison.CurrentCulture))
                {
                    RuneEditNewTranslation = RuneEditNewTranslation.ToUpper();
                }
                
                RuneEditNewTranslationOkay = RuneEditNewTranslation.Trim() != string.Empty || RuneEditNewConfidenceLevel == ConfidenceLevel.Low;
                break;
        }
    }
    
    // Helpers
    private static bool IsFilterMatch(int num, int filter)
    {
        for (int i = 0; i < 12; i++)
        {
            if ((filter & (1 << i)) != 1 << i) 
                continue;
            if ((num & (1 << i)) != 1 << i)
            {
                return false;
            }
        }

        return true;
    }
    
    private bool CanClearSelectionCondition()
    {
        return SelectionFilter != 0 || RuneFilterText != string.Empty;
    }
    
    private bool CanAddRuneCondition()
    {
        return !_savedRunes.ContainsKey(SelectionFilter) && SelectionFilter != 0;
    }
}

