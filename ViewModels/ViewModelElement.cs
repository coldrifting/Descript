using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public ViewModelDialogElement ViewModelDialog { get; } = new(mainWindowViewModel);

    private readonly Dictionary<char, Element> _elements = new();
    
    public int CurrentSelection { get; set => SetField(ref field, value); }
    public string FilterText { get; set => SetField(ref field, value.ToLower()); } = string.Empty;

    public bool CanClearFilters => CurrentSelection > 0 || FilterText.Length > 0;
    public bool CanAddRune      => CurrentSelection != 0 && !_elements.ContainsKey(Element.GlyphFromId(CurrentSelection));

    public bool IsShown => Vm.IsElementsListShown;
    public void UpdateIsShown() => OnPropertyChanged(nameof(IsShown));
    
    public Element? this[char glyph] => _elements.GetValueOrDefault(glyph);
    
    public IEnumerable<Element> Elements => _elements.Values.OrderBy(r => r.Glyph);
    public List<ElementGroup> ElementsFilteredAndGrouped => _elements.Values
        .WithSelection(CurrentSelection)
        .Ordered(FilterText)
        .Matching(FilterText, CurrentSelection)
        .Chunk(4)
        .Select(batch => new ElementGroup
        {
            Element1 = batch[0], 
            Element2 = batch.Length > 1 ? batch[1] : null, 
            Element3 = batch.Length > 2 ? batch[2] : null, 
            Element4 = batch.Length > 3 ? batch[3] : null, 
        })
        .WithMatch(FilterText.Trim() == "" ? -1 : CurrentMatchIndex)
        .ToList();
    
    public int NumFilteredElements => ElementsFilteredAndGrouped.Count * 4 - 4 + (ElementsFilteredAndGrouped.LastOrDefault()?.Length ?? 0);

    public int CurrentMatchIndex { get; set => SetField(ref field, value); }

    public int CurrentMatch => ElementsFilteredAndGrouped.GetIndex(CurrentMatchIndex).Id;
    
    [RelayCommand]
    private void Primary(char glyph)
    {
        if (Vm.ViewModelSentences.ViewModelDialog.IsOpen)
        {
            Vm.ViewModelSentences.ViewModelDialog.InsertAtCursor(glyph.ToString());
            if (_elements.TryGetValue(glyph, out Element? _))
            {
                return;
            }

            _elements.Add(glyph, new Element { Glyph = glyph });
            OnPropertyChanged(nameof(Elements));
        }
        else
        {
            ViewModelDialog.Open(glyph);
        }
    }
    
    [RelayCommand]
    private void Edit(char glyph)
    {
        ViewModelDialog.Open(glyph);
    }
    
    [RelayCommand]
    private static void Copy(char glyph)
    {
        IClipboard? clipboard = ClipboardHelper.GetClipboard();
        clipboard?.SetTextAsync(glyph.ToString());
    }
    
    [RelayCommand]
    private void ClearFilters()
    {
        CurrentSelection = 0;
        FilterText = string.Empty;
        
        OnPropertyChanged(nameof(FilterText));
        OnPropertyChanged(nameof(CurrentSelection));
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

    public Element AddOrEdit(char glyph, string newTranslation = "", ConfidenceLevel newConfidence = ConfidenceLevel.Low)
    {
        if (_elements.TryGetValue(glyph, out Element? element))
        {
            string oldTranslation = element.Translation;
            ConfidenceLevel oldConfidence = element.Confidence;

            if (oldTranslation == newTranslation && oldConfidence == newConfidence)
            {
                return element;
            }
            
            element.Translation = newTranslation;
            element.Confidence = newConfidence;
        }
        else
        {
            _elements[glyph] = new Element { Glyph = glyph, Translation = newTranslation, Confidence = newConfidence };
        }
        
        OnPropertyChanged(nameof(Elements));
        
        return _elements[glyph];
    }

    // Workaround for resetting one filter type when the other changes
    private bool _switch;
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(FilterText) && FilterText != "" && !_switch)
        {
            _switch = true;
            CurrentSelection = 0;
        }

        if (e.PropertyName is nameof(CurrentSelection) && CurrentSelection != 0 && !_switch)
        {
            _switch = true;
            FilterText = "";
        }
        
        if (e.PropertyName is nameof(FilterText) or nameof(CurrentSelection))
        {
            CurrentMatchIndex = 0;
            OnPropertyChanged(nameof(ElementsFilteredAndGrouped));
            OnPropertyChanged(nameof(CanClearFilters));
            OnPropertyChanged(nameof(CurrentMatch));
        }

        if (e.PropertyName is nameof(CurrentMatchIndex))
        {
            CurrentMatchIndex = Math.Clamp(CurrentMatchIndex, 0, NumFilteredElements - 1);
            
            OnPropertyChanged(nameof(ElementsFilteredAndGrouped));
        }

        if (e.PropertyName is nameof(Elements))
        {
            OnPropertyChanged(nameof(ElementsFilteredAndGrouped));
            Vm.ViewModelSentences.Refresh();
        }

        if (e.PropertyName is nameof(ElementsFilteredAndGrouped))
        {
            OnPropertyChanged(nameof(CanAddRune));
            OnPropertyChanged(nameof(NumFilteredElements));
            OnPropertyChanged(nameof(CurrentMatch));
            
            _switch = false;
        }
    }
}