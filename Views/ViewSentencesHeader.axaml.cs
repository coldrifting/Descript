using Avalonia.Controls;
using Avalonia.Input;
using Descript.ViewModels;

namespace Descript.Views;

public partial class ViewSentencesHeader : UserControl
{
    public ViewSentencesHeader()
    {
        InitializeComponent();
    }

    private void InputElement_OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        ((ViewModelSentences?)DataContext)?.Vm.ViewModelElement.SetMatchShownCommand.Execute(true);
    }

    private void InputElement_OnLostFocus(object? sender, FocusChangedEventArgs e)
    {
        ((ViewModelSentences?)DataContext)?.Vm.ViewModelElement.SetMatchShownCommand.Execute(false);
    }
}