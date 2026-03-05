using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace Descript.Utils;

public static class ClipboardHelper
{
    public static IClipboard? GetClipboard()
    {
        return Application.Current?.ApplicationLifetime switch
        {
            // Desktop lifetime
            IClassicDesktopStyleApplicationLifetime { MainWindow: { } window } =>
                window.Clipboard,
            // Mobile/SingleView lifetime
            ISingleViewApplicationLifetime { MainView: { } mainView } when mainView.GetVisualRoot() is TopLevel topLevel =>
                topLevel.Clipboard,
            _ => null
        };
    }
}