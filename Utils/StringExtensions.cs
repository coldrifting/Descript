using System.Globalization;
using System.Threading;

namespace Descript.Utils;

public static class StringExtensions {
    public static string ToTitleCase(this string str)
    {
        TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        
        return str.Length switch
        {
            0 => str,
            1 => str.ToUpper(),
            _ => textInfo.ToTitleCase(str)
        };
    }
}