using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Descript.Models;

namespace Descript.Utils;

public class ConfidenceLevelToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ConfidenceLevel level = (ConfidenceLevel)(value ?? ConfidenceLevel.Low);
        return level switch
        {
            ConfidenceLevel.High => Brushes.MediumSeaGreen,
            ConfidenceLevel.Medium => Brushes.DarkGoldenrod,
            _ => Brushes.IndianRed
        };
    }

    // Not Supported
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}