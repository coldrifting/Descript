using Avalonia.Media;

namespace Descript.Utils;

public static class SolidColorBrushExtensions
{
    public static ISolidColorBrush WithOpacity(this ISolidColorBrush brush, double opacity)
    {
        return new SolidColorBrush(brush.Color, opacity);
    }
}