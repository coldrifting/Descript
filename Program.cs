using Avalonia;
using System;
using Avalonia.Media;
using Avalonia.Media.Fonts;

namespace Descript;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    // ReSharper disable once MemberCanBePrivate.Global
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .ConfigureFonts(fm => fm.AddFontCollection(new EmbeddedFontCollection(
                new Uri("fonts:MyFonts", UriKind.Absolute),
                new Uri("avares://Descript/Assets/Fonts#Tunic Runes", UriKind.Absolute)
                )))
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "avares://Descript/Assets/Fonts#Tunic Runes",
                FontFallbacks =
                [
                    new FontFallback
                    {
                        FontFamily = new FontFamily("avares://Descript/Assets/Fonts#Tunic Runes"), 
                        UnicodeRange = UnicodeRange.Parse("U+E000-U+EFFF")
                    }
                ],
                
            })
            .LogToTrace();
}