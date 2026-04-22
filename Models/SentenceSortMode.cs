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