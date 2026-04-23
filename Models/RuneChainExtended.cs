using System.Collections.Immutable;
using System.Linq;

namespace Descript.Models;

public record RuneChainExtended : RuneChain
{
    public required ImmutableList<Rune> Runes { get; init; }

    public bool ShowRunes => Confidence == ConfidenceLevel.Low && Translation == "";

    public bool ShowWordDialogButton => Confidence == ConfidenceLevel.Low && Translation == "" && Runes.All(s => s.Confidence == ConfidenceLevel.High);
}