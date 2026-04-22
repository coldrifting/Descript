using System.Collections.Immutable;

namespace Descript.Models;

public record RuneChainExtended : RuneChain
{
    public required ImmutableList<Rune> Runes { get; init; }

    public bool ShowRunes => Confidence == ConfidenceLevel.Low && Translation == "";
}