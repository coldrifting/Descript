using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.ViewModels.Base;

namespace Descript.ViewModels.Dialog;

public partial class DialogElement(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    
    public bool IsOpen { get; set => SetField(ref field, value); }
    public string Title { get; set => SetField(ref field, value); } = string.Empty;
    
    public bool IsValid => Translation.Trim() != string.Empty || Confidence == ConfidenceLevel.Low;
    
    private char _glyph;
    
    public string Translation { get; set => SetField(ref field, value.ToLower()); } = string.Empty;
    public ConfidenceLevel Confidence { get; set => SetField(ref field, value); } = ConfidenceLevel.Low;

    [RelayCommand]
    public void Open(char glyph)
    {
        Vm.ViewModelElement.TryGet(glyph, out Element? element);
        element ??= new Element { Glyph = glyph };
        Console.WriteLine(element);
        
        _glyph = element.Glyph;
        Translation = element.Translation;
        Confidence = element.Confidence;
        
        Console.WriteLine(Translation);
        
        Title = $"Input Element Translation Guess - {element.Glyph}";
        IsOpen = true;
    }
    
    [RelayCommand]
    private void Submit()
    {
        Vm.ViewModelElement.Edit(_glyph, Translation, Confidence);
        
        IsOpen = false;
    }
    
    [RelayCommand]
    private void Cancel()
    {
        IsOpen = false;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsOpen) || e.PropertyName == nameof(Confidence) || e.PropertyName == nameof(Translation))
        {
            OnPropertyChanged(nameof(IsValid));
        }
    }
}