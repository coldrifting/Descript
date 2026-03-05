using Avalonia.Controls;
using Descript.ViewModels;

namespace Descript.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel DataContextNotNull => DataContext as MainWindowViewModel ?? new MainWindowViewModel();
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}