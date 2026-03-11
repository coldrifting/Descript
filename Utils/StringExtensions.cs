using System.Globalization;
using System.Threading;

namespace Descript.Utils;

public static class StringExtensions {
    extension(string str)
    {
        public string ToTitleCase()
        {
            TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        
            return str.Length switch
            {
                0 => str,
                1 => str.ToUpper(),
                _ => textInfo.ToTitleCase(str)
            };
        }

        public bool ContainsTrimmed(string b)
        {
            return str.Trim().Contains(b.Trim(), System.StringComparison.CurrentCultureIgnoreCase);
        }
    }
}