using System.Collections.Generic;
using Avalonia.Input;

namespace Descript.Models;

public record KeyInfo
{
    public int Id { get; init; }
    public int? IdModified { get; init; }
}

public static class InputKeys
{
    public static Dictionary<Key, KeyInfo> Keys => new()
    {
        { Key.M, new KeyInfo { Id = 0 } }, // Left Bar
        { Key.OemSemicolon, new KeyInfo { Id = 11 } }, // Dot

        { Key.U, new KeyInfo { Id = 3, IdModified = 7 } }, // Top Left 
        { Key.I, new KeyInfo { Id = 1, IdModified = 2 } }, // Middle Bar 
        { Key.K, new KeyInfo { Id = 1, IdModified = 2 } }, // Middle Bar 
        { Key.O, new KeyInfo { Id = 4, IdModified = 8 } }, // Top Right 

        { Key.J, new KeyInfo { Id = 5, IdModified = 9 } }, // Bottom Left 
        { Key.L, new KeyInfo { Id = 6, IdModified = 10 } } // Bottom Right 
    };
}