using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.ViewModels.Base;

namespace Descript.ViewModels.Dialog;

public partial class DialogElement(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    
    public bool IsOpen { get; set => SetField(ref field, value); }
    
    public bool IsValid => Translation.Trim() != string.Empty || Confidence == ConfidenceLevel.Low;
    
    public char Glyph { get; private set => SetField(ref field, value); }
    
    public string Translation { get; set => SetField(ref field, value.ToLower()); } = string.Empty;
    public ConfidenceLevel Confidence { get; set => SetField(ref field, value); } = ConfidenceLevel.Low;

    [RelayCommand]
    public void Open(char glyph)
    {
        Element element = Vm.ViewModelElement[glyph] ?? new Element { Glyph = glyph };
        
        Glyph = element.Glyph;
        Translation = element.Translation;
        Confidence = element.Confidence;
        
        IsOpen = true;
    }
    
    [RelayCommand]
    private void Submit()
    {
        Vm.ViewModelElement.AddOrEdit(Glyph, Translation, Confidence);
        
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