using System;

namespace Descript.Utils;

public static class CursorHelper
{
    public static void InsertAtCursor(string input, 
        int start, int end, string current, 
        Action<int> setStart, Action<int> setEnd, Action<string> setCurrent)
    {
        if (start == end)
        {
            setCurrent.Invoke(current.Insert(Math.Min(start, current.Length), input));
            
            setStart.Invoke(start + input.Length);
            setEnd.Invoke(start + input.Length);
        }
        else
        {
            setCurrent.Invoke(end > start
                ? current.Remove(start, end - start).Insert(start, input)
                : current.Remove(end, start - end).Insert(end, input));

            setStart.Invoke(Math.Min(start, end) + input.Length);
            setEnd.Invoke(Math.Min(start, end) + input.Length);
        }
    }
}