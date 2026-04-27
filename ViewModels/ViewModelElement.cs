using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.Models.Flat;
using Descript.Utils;
using Descript.ViewModels.Base;
using Descript.ViewModels.Dialog;

namespace Descript.ViewModels;

public partial class ViewModelElement(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    public DialogElement Dialog { get; } = new(mainWindowViewModel);

    private readonly Dictionary<char, Element> _elements = new();

    public char CurrentSelection { get; set => SetField(ref field, value); } = (char)0;
    public string FilterText    { get; set => SetField(ref field, value.ToLower()); } = string.Empty;
    
    public bool CanClearFilters => CurrentSelection > 0 || FilterText.Length > 0;
    public bool CanAddRune      => CurrentSelection != 0 && !_elements.ContainsKey(CurrentSelection);

    public bool IsShown => Vm.IsRuneListShown;
    public void UpdateIsShown() => OnPropertyChanged(nameof(IsShown));
    
    public IEnumerable<Element> Elements => _elements.Values.OrderBy(r => r.Glyph);
    public List<ElementGroup> ElementsFilteredAndGrouped => _elements.Values
        .OrderBy(OrderByConfidence)
        .ThenBy(OrderByTranslation)
        .ThenBy(OrderByGlyph)
        .Select(element => element.Id == CurrentSelection ? Element.Select.Invoke(element) : element)
        .Append(Element.FromIdSelected(CurrentSelection))
        .Where(IsMatch)
        .Chunk(4)
        .Select(batch => new ElementGroup
        {
            Element1 = batch[0], 
            Element2 = batch.Length > 1 ? batch[1] : null, 
            Element3 = batch.Length > 2 ? batch[2] : null, 
            Element4 = batch.Length > 3 ? batch[3] : null, 
        })
        .ToList();
    
    public Element this[char glyph]
    {
        get
        {
            Element? element = _elements.GetValueOrDefault(glyph);
            if (element == null)
            {
                _elements[glyph] = new Element { Glyph = glyph };
            }

            return _elements[glyph];
        }
    }

    public bool Add(int id, bool update = true)
    {
        return Add(Element.FromId(id), update);
    }
    
    public bool Add(Element element, bool update = true)
    {
        if (_elements.TryAdd(element.Glyph, element))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Elements));
            }

            return true;
        }

        Console.WriteLine($"Element {element.Glyph} already exists");
        return false;
    }

    public void Add(IEnumerable<ElementFlat> elements)
    {
        foreach (ElementFlat elementFlat in elements)
        {
            Add(ElementFlat.ToElement.Invoke(elementFlat), false);
        }

        OnPropertyChanged(nameof(Elements));
    }

    public void Edit(char glyph, string newTranslation, ConfidenceLevel newConfidence)
    {
        if (_elements.TryGetValue(glyph, out Element? element))
        {
            string oldTranslation = element.Translation;
            ConfidenceLevel oldConfidence = element.Confidence;

            if (oldTranslation == newTranslation && oldConfidence == newConfidence)
            {
                return;
            }
            
            element.Translation = newTranslation;
            element.Confidence = newConfidence;
        }
        else
        {
            _elements[glyph] = new Element { Glyph = glyph, Translation = newTranslation, Confidence = newConfidence };
        }
        OnPropertyChanged(nameof(Elements));
    }

    public void Delete(char glyph)
    {
        if (_elements.Remove(glyph, out Element? _))
        {
            OnPropertyChanged(nameof(Elements));
        }
        else
        {
            Console.WriteLine($"Element {glyph} does not exist");
        }
    }

    public bool TryGet(char glyph, [MaybeNullWhen(false)] out Element element)
    {
        if (_elements.TryGetValue(glyph, out element))
        {
            return true;
        }
        Console.WriteLine($"Element {glyph} does not exist");
        return false;
    }

    private void ClearFilters()
    {
        CurrentSelection = (char)0;
        FilterText = string.Empty;
        
        OnPropertyChanged(nameof(FilterText));
        OnPropertyChanged(nameof(CurrentSelection));
    }

    // Helpers
    private static ConfidenceLevel OrderByConfidence(Element rune)
    {
        return rune.Confidence;
    }

    private static string OrderByTranslation(Element rune)
    {
        return rune.Translation == "" ? "ZZZZZZZ" : rune.Translation;
    }

    private static int OrderByGlyph(Element rune)
    {
        return rune.Glyph;
    }

    private bool IsMatch(Element r)
    {
        if (r.Id == 0)
        {
            return false;
        }
        
        if (r.Confidence == ConfidenceLevel.Confirmed && _elements.ContainsKey(r.Glyph))
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

    [RelayCommand]
    private void AddRune(char glyph)
    {
        Add(glyph);
    }
    
    [RelayCommand]
    private void EditRune(char glyph)
    {
        Dialog.Open(glyph);
    }

    [RelayCommand]
    private void Primary(char glyph)
    {
        if (Vm.ViewModelSentences.Dialog.IsOpen)
        {
            Vm.ViewModelSentences.Dialog.InsertAtCursor(glyph.ToString());
        }
        else
        {
            Dialog.Open(glyph);
        }
    }
    
    [RelayCommand]
    private void Copy(char glyph)
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        clipboard?.SetTextAsync(glyph.ToString());
    }
    
    [RelayCommand]
    private void ClearRuneListFilters()
    {
        ClearFilters();
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        if (e.PropertyName is nameof(FilterText) or nameof(CurrentSelection))
        {
            OnPropertyChanged(nameof(ElementsFilteredAndGrouped));
            OnPropertyChanged(nameof(CanClearFilters));
        }

        if (e.PropertyName is nameof(ElementsFilteredAndGrouped))
        {
            OnPropertyChanged(nameof(CanAddRune));
        }

        if (e.PropertyName is nameof(Elements))
        {
            OnPropertyChanged(nameof(ElementsFilteredAndGrouped));
            Vm.ViewModelSentences.Refresh();
        }

        if (e.PropertyName is nameof(CurrentSelection))
        {
            OnPropertyChanged(nameof(ElementsFilteredAndGrouped));
        }
    }
}