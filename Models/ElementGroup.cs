using System;
using Descript.ViewModels.Base;

namespace Descript.Models;

public class ElementGroup : ViewModelBase
{
    public required Element Element1 { get; init; }
    public Element? Element2 { get; init; }
    public Element? Element3 { get; init; }
    public Element? Element4 { get; init; }

    public int Length => Element2 is null
        ? 1
        : Element3 is null
            ? 2
            : Element4 is null
                ? 3
                : 4;

    public Element this[int index] => index switch
    {
        0 => Element1,
        1 => Element2,
        2 => Element3,
        3 => Element4,
        _ => null
    } ?? throw new IndexOutOfRangeException("ElementGroup index out of range.");
    
    public int MatchId => Element1.IsCurrentMatch
        ? Element1.Id
        : Element2?.IsCurrentMatch == true
            ? Element2.Id
            : Element3?.IsCurrentMatch == true
                ? Element3.Id
                : Element4?.IsCurrentMatch == true
                    ? Element4.Id
                    : 0;

    public ElementGroup WithMatch(int matchIndex)
    {
        return matchIndex switch
        {
            0 => new ElementGroup
            {
                Element1 = Element1.WithMatch, 
                Element2 = Element2, 
                Element3 = Element3, 
                Element4 = Element4
            },
            1 when Element2 is not null => new ElementGroup
            {
                Element1 = Element1, 
                Element2 = Element2.WithMatch, 
                Element3 = Element3, 
                Element4 = Element4
            },
            2 when Element3 is not null => new ElementGroup
            {
                Element1 = Element1, 
                Element2 = Element2, 
                Element3 = Element3.WithMatch, 
                Element4 = Element4
            },
            3 when Element4 is not null => new ElementGroup
            {
                Element1 = Element1, 
                Element2 = Element2, 
                Element3 = Element3, 
                Element4 = Element4.WithMatch
            },
            _ => this
        };
    }
}