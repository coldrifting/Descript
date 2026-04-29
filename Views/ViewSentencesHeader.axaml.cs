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

    private void FilterText_OnKeyDown(object? sender, KeyEventArgs e)
    {
        ((ViewModelSentences?)DataContext)?.ViewModelElementInput.OnKeyDown(e.Key);
    }

    private void FilterText_OnKeyUp(object? sender, KeyEventArgs e)
    {
        ((ViewModelSentences?)DataContext)?.ViewModelElementInput.OnKeyUp(e.Key);
    }
}