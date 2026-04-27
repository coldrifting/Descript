namespace Descript.Models.Flat;

public record Translations
{
    public ElementFlat[] Elements { get; init; } = [];
    public PhraseFlat[] Phrases { get; init; } = [];
    public SentenceFlat[] Sentences { get; init; } = [];
}