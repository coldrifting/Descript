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

    private void SentenceInput_OnKeyDown(object? sender, KeyEventArgs e)
    {
        ((ViewModelDialogSentence?)DataContext)?.ViewModelElementInput.OnKeyDown(e.Key);
    }

    private void SentenceInput_OnKeyUp(object? sender, KeyEventArgs e)
    {
        ((ViewModelDialogSentence?)DataContext)?.ViewModelElementInput.OnKeyUp(e.Key);
    }
}