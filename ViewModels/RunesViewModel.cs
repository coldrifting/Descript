using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Descript.Models;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public sealed class RunesViewModel(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; set; } = mainWindowViewModel;
    
    private readonly Dictionary<int, Rune> _allRunes = new();

    public int CurrentSelection { get; set => SetField(ref field, value); } = 0;
    public string FilterText    { get; set => SetField(ref field, value); } = string.Empty;
    
    public RuneSortMode SortMode { get; set => SetField(ref field, value); } = RuneSortMode.ByConfidence;
    public string SortModeString => SortMode.ToString().Replace("By", "By ");

    public bool CanClearFilters => CurrentSelection > 0 || FilterText.Length > 0;
    public bool CanAddRune      => CurrentSelection != 0 && !_allRunes.ContainsKey(CurrentSelection);
    
    // Returns a filtered and sorted subset of all stored runes
    public List<Rune> Runes => (SortMode switch
        {
            RuneSortMode.ByConfidence => _allRunes.Values.OrderBy(OrderByConfidence)
                .ThenBy(OrderByTranslation)
                .ThenBy(OrderById),
            RuneSortMode.ByTranslation => _allRunes.Values.OrderBy(OrderByTranslation)
                .ThenBy(OrderByConfidence)
                .ThenBy(OrderById),
            _ => _allRunes.Values.OrderBy(OrderById)
                .ThenBy(OrderByConfidence)
                .ThenBy(OrderByTranslation)
        })
        .Where(IsMatch)
        .ToList();

    public bool Add(int id, bool update = true)
    {
        return Add(new Rune(id), update);
    }
    
    public bool Add(Rune rune, bool update = true)
    {
        if (_allRunes.TryAdd(rune.Id, rune))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Runes));
            }

            return true;
        }

        Console.WriteLine($"Rune of id {rune.Id} already exists");
        return false;
    }

    public void Add(IEnumerable<Rune> runes)
    {
        bool updated = false;
        foreach (Rune rune in runes)
        {
            if (Add(rune, false))
            {
                updated = true;
            }
        }

        if (updated)
        {
            OnPropertyChanged(nameof(Runes));
        }
    }

    public void Edit(int runeId, string newTranslation, ConfidenceLevel newConfidence)
    {
        if (_allRunes.TryGetValue(runeId, out Rune? rune))
        {
            string oldTranslation = rune.Translation;
            ConfidenceLevel oldConfidence = rune.Confidence;
            
            rune.Translation = newTranslation;
            rune.Confidence = newConfidence;

            if (oldTranslation != newTranslation || oldConfidence != newConfidence)
            {
                OnPropertyChanged(nameof(Runes));
            }
        }
        else
        {
            Console.WriteLine($"Rune of id {runeId} does not exist");
        }
    }

    public void Delete(int runeId)
    {
        if (_allRunes.Remove(runeId, out Rune? _))
        {
            OnPropertyChanged(nameof(Runes));
        }
        else
        {
            Console.WriteLine($"Rune of id {runeId} does not exist");
        }
    }

    public bool TryGet(int runeId, [MaybeNullWhen(false)] out Rune rune)
    {
        if (_allRunes.TryGetValue(runeId, out rune))
        {
            return true;
        }
        Console.WriteLine($"Rune of id {runeId} does not exist");
        return false;
    }

    // For saving to file in a consistent order
    public Rune[] GetOrdered()
    {
        return _allRunes.Values.OrderBy(r => r.Id).ToArray();
    }

    public void ClearFilters()
    {
        CurrentSelection = 0;
        FilterText = string.Empty;
        
        OnPropertyChanged(nameof(FilterText));
        OnPropertyChanged(nameof(CurrentSelection));
    }
    
    public void ToggleSortMode()
    {
        SortMode = SortMode switch
        {
            RuneSortMode.ByTranslation => RuneSortMode.ByConfidence,
            RuneSortMode.ByConfidence => RuneSortMode.ById,
            RuneSortMode.ById => RuneSortMode.ByTranslation,
            _ => SortMode
        };
    }

    // Helpers
    private static ConfidenceLevel OrderByConfidence(Rune rune)
    {
        return rune.Confidence;
    }

    private static string OrderByTranslation(Rune rune)
    {
        return rune.Translation == "" ? "ZZZZZZZ" : rune.Translation;
    }

    private static int OrderById(Rune rune)
    {
        return rune.Id;
    }

    private bool IsMatch(Rune r)
    {
        return r.Translation.Trim().Contains(FilterText.Trim(), StringComparison.CurrentCultureIgnoreCase) 
               && IsFilterMatch(r.Id, CurrentSelection);
    }
    
    private static bool IsFilterMatch(int num, int filter)
    {
        return (filter & num) == filter;
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        if (e.PropertyName is nameof(FilterText))
        {
            FilterText = FilterText.ToLower();
        }
        
        if (e.PropertyName is nameof(SortMode) or nameof(FilterText) or nameof(CurrentSelection))
        {
            OnPropertyChanged(nameof(Runes));
        }

        if (e.PropertyName is nameof(SortMode))
        {
            OnPropertyChanged(nameof(SortModeString));
        }

        if (e.PropertyName is nameof(FilterText) or nameof(CurrentSelection))
        {
            OnPropertyChanged(nameof(CanClearFilters));
        }

        if (e.PropertyName is nameof(Runes))
        {
            OnPropertyChanged(nameof(CanAddRune));
            Vm.Translations.UpdateTranslations();
        }
    }
}