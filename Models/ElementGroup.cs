using Descript.ViewModels.Base;

namespace Descript.Models;

public class ElementGroup : ViewModelBase
{
    public required Element Element1 { get; init; }
    public Element? Element2 { get; init; }
    public Element? Element3 { get; init; }
    public Element? Element4 { get; init; }
}