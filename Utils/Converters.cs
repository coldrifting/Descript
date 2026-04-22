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
            ConfidenceLevel.Confirmed => Brushes.White,
            ConfidenceLevel.High => Brushes.MediumSeaGreen,
            ConfidenceLevel.Medium => Brushes.DarkGoldenrod,
            _ => Brushes.IndianRed
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class SentenceSortModeDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        SentenceSortMode sortMode = (SentenceSortMode)(value ?? SentenceSortMode.ByCategory);
        return sortMode.GetDescription();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    } 
}

public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value?.ToString());
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    } 
}

public class EmptyStringToQuestionMarkConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value?.ToString() == string.Empty)
        {
            return "?";
        }
        
        return value?.ToString() ?? "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    } 
}