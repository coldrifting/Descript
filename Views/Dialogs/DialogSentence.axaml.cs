using Avalonia.Controls;
using Avalonia.Input;
using Descript.ViewModels.Dialog;

namespace Descript.Views.Dialogs;

public partial class DialogSentence : UserControl
{
    public DialogSentence()
    {
        InitializeComponent();
    }
    
    private void SentenceInput_OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        ((ViewModelDialogSentence?)DataContext)?.Vm.ViewModelElement.SetMatchShownCommand.Execute(true);
    }

    private void SentenceInput_OnLostFocus(object? sender, FocusChangedEventArgs e)
    {
        ((ViewModelDialogSentence?)DataContext)?.Vm.ViewModelElement.SetMatchShownCommand.Execute(false);
    }
}