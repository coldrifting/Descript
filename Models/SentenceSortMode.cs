using System.Collections.Generic;
using System.ComponentModel;

namespace Descript.Models;

public enum SentenceSortMode
{
    [Description("By Category")]
    ByCategory,
    
    [Description("By Least Translated")]
    ByLeastTranslated,
    
    [Description("By Most Translated")]
    ByMostTranslated
}

public static class SentenceSortModeEx
{
    public static List<SentenceSortMode> SortModes =>
    [
        SentenceSortMode.ByCategory,
        SentenceSortMode.ByLeastTranslated,
        SentenceSortMode.ByMostTranslated
    ];
}