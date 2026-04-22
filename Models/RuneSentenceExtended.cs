using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Descript.Models;

public record RuneSentenceExtended : RuneSentence
{
    public static RuneSentenceExtended FromRuneSentence(RuneSentence sentence, IEnumerable<RuneChainExtended> runeChains)
    {
        return new RuneSentenceExtended
        {
            Sentence = sentence.Sentence,
            Category = sentence.Category,
            SubCategory = sentence.SubCategory,
            Context =  sentence.Context,
            RuneChains = runeChains.ToImmutableList()
        };
    }
    
    public required ImmutableList<RuneChainExtended> RuneChains { get; init; }

    public int NumTranslatedRuneChains => RuneChains.Count(r => r.Translation != "");
    public int NumUntranslatedRuneChains => RuneChains.Count(r => r.Translation == "");

    public string SentenceTranslated => RuneChains
        .Select(r => r.Translation != "" ? r.Translation : r.Glyphs)
        .Aggregate((a, b) => a + ' ' + b);
}