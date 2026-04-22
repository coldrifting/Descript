using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Descript.Data;
using Descript.Interfaces;
using Descript.Models;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public sealed class ViewModelRune(ViewModelMainWindow viewModelMainWindow) : ViewModelBase, ILoadSave
{
    private ViewModelMainWindow Vm { get; set; } = viewModelMainWindow;
    
    private readonly Dictionary<char, Rune> _runes = new();

    public char CurrentSelection { get; set => SetField(ref field, value); } = (char)0;
    public string FilterText    { get; set => SetField(ref field, value); } = string.Empty;
    
    public bool CanClearFilters => CurrentSelection > 0 || FilterText.Length > 0;
    public bool CanAddRune      => CurrentSelection != 0 && !_runes.ContainsKey(CurrentSelection);
    
    public IEnumerable<Rune> Runes => _runes.Values.OrderBy(r => r.Glyph);
    
    // Returns a filtered and sorted subset of all stored runes
    public List<Rune> RunesFiltered => _runes.Values
        .OrderBy(OrderByConfidence)
        .ThenBy(OrderByTranslation)
        .ThenBy(OrderByGlyph)
        .Append(Rune.FromId(CurrentSelection) with { Confidence = ConfidenceLevel.Confirmed })
        .Where(IsMatch)
        .ToList();
    
    public void Load()
    {
        Add(DataManagement.Load<Rune>());
    }

    public void Save()
    {
        DataManagement.Save(Runes);
    }
    
    public Rune? this[char index] => _runes.GetValueOrDefault(index);

    public bool Add(int id, bool update = true)
    {
        return Add(Rune.FromId(id), update);
    }
    
    public bool Add(Rune rune, bool update = true)
    {
        if (_runes.TryAdd(rune.Glyph, rune))
        {
            if (update)
            {
                OnPropertyChanged(nameof(RunesFiltered));
            }

            return true;
        }

        Console.WriteLine($"Rune {rune.Glyph} already exists");
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
            OnPropertyChanged(nameof(RunesFiltered));
        }
    }

    public void Edit(char glyph, string newTranslation, ConfidenceLevel newConfidence)
    {
        if (_runes.TryGetValue(glyph, out Rune? rune))
        {
            string oldTranslation = rune.Translation;
            ConfidenceLevel oldConfidence = rune.Confidence;

            if (oldTranslation != newTranslation || oldConfidence != newConfidence)
            {
                _runes[glyph] = rune with { Translation = newTranslation, Confidence = newConfidence };
                
                OnPropertyChanged(nameof(RunesFiltered));
            }
        }
        else
        {
            Console.WriteLine($"Rune {glyph} does not exist");
        }
    }

    public void Delete(char glyph)
    {
        if (_runes.Remove(glyph, out Rune? _))
        {
            OnPropertyChanged(nameof(RunesFiltered));
        }
        else
        {
            Console.WriteLine($"Rune {glyph} does not exist");
        }
    }

    public bool TryGet(char glyph, [MaybeNullWhen(false)] out Rune rune)
    {
        if (_runes.TryGetValue(glyph, out rune))
        {
            return true;
        }
        Console.WriteLine($"Rune {glyph} does not exist");
        return false;
    }

    public void ClearFilters()
    {
        CurrentSelection = (char)0;
        FilterText = string.Empty;
        
        OnPropertyChanged(nameof(FilterText));
        OnPropertyChanged(nameof(CurrentSelection));
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

    private static int OrderByGlyph(Rune rune)
    {
        return rune.Glyph;
    }

    private bool IsMatch(Rune r)
    {
        if (r.Id == 0)
        {
            return false;
        }
        
        if (r.Confidence == ConfidenceLevel.Confirmed && _runes.ContainsKey(r.Glyph))
        {
            return false;
        }
        
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
        
        if (e.PropertyName is nameof(FilterText) or nameof(CurrentSelection))
        {
            OnPropertyChanged(nameof(RunesFiltered));
        }

        if (e.PropertyName is nameof(FilterText) or nameof(CurrentSelection))
        {
            OnPropertyChanged(nameof(CanClearFilters));
        }

        if (e.PropertyName is nameof(RunesFiltered))
        {
            OnPropertyChanged(nameof(CanAddRune));
            Vm.ViewModelRuneSentence.Refresh();
        }
    }
}