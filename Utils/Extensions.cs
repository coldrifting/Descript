using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace Descript.Utils;

public static class Extensions {
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
            return str.Trim().Contains(b.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
    
    extension(Enum value) 
    {
        public string GetDescription()
        {
            Type type = value.GetType();
            FieldInfo? fieldInfo = type.GetField(value.ToString());
            if (fieldInfo is null)
            {
                return value.ToString();
            }
            
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}