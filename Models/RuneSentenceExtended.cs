using System.Collections.Immutable;

namespace Descript.Models;

public record RuneSentenceExtended : RuneSentence
{
    public required ImmutableList<RuneChainExtended> RuneChains { get; init; }
}