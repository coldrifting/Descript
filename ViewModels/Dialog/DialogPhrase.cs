using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.ViewModels.Base;

namespace Descript.ViewModels.Dialog;

public partial class DialogPhrase(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; set; } = mainWindowViewModel;
    
    public bool IsOpen { get; set => SetField(ref field, value); }
    public string Title { get; set => SetField(ref field, value); } = string.Empty;
    
    public bool IsValid => Translation.Trim() != string.Empty || Confidence == ConfidenceLevel.Low;

    private string _glyphs = string.Empty;
    
    public string Translation { get; set => SetField(ref field, value); } = string.Empty;
    public ConfidenceLevel Confidence { get; set => SetField(ref field, value); } = ConfidenceLevel.Low;

    [RelayCommand]
    public void Open(string glyphs)
    {
        Vm.ViewModelPhrases.TryGet(glyphs, out Phrase? runeChain);
        runeChain ??= new Phrase {Elements = []};
        
        _glyphs = runeChain.Glyphs;
        Translation = runeChain.Translation;
        Confidence = runeChain.Confidence;
        
        Title = $"Input Phrase Translation Guess - {runeChain.Glyphs}";
        IsOpen = true;
    }
    
    [RelayCommand]
    private void Submit()
    {
        Vm.ViewModelPhrases.Edit(_glyphs, Translation, Confidence);
        
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