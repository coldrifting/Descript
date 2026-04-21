using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Descript.ViewModels;
using Descript.Views;

namespace Descript;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ViewModelMainWindow dataContext = new();
            desktop.MainWindow = new MainWindow
            {
                DataContext = dataContext
            };
            
            desktop.Exit += (_, _) =>
            {
                dataContext.SaveData();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}