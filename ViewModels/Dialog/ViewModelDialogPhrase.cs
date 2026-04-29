using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.ViewModels.Base;

namespace Descript.ViewModels.Dialog;

public partial class ViewModelDialogPhrase(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;
    
    public bool IsOpen { get; set => SetField(ref field, value); }
    public string Glyphs { get; private set => SetField(ref field, value); } = string.Empty;
    
    public string Translation { get; set => SetField(ref field, value); } = string.Empty;

    [RelayCommand]
    public void Open(string glyphs)
    {
        Phrase phrase = Vm.ViewModelPhrases[glyphs] ?? new Phrase { Elements = [] };
        
        Glyphs = phrase.Glyphs;
        Translation = phrase.Translation;
        
        IsOpen = true;
    }
    
    [RelayCommand]
    private void Submit()
    {
        Vm.ViewModelPhrases.Edit(Glyphs, Translation);
        
        IsOpen = false;
    }
    
    [RelayCommand]
    private void Cancel()
    {
        IsOpen = false;
    }
}