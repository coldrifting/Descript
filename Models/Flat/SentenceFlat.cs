using System;
using Descript.ViewModels;

namespace Descript.Models.Flat;

/// Used for Loading, Saving, and Editing
public record SentenceFlat
{
    public required string Sentence { get; init; }
    
    public string Category { get; init; } = string.Empty;
    public string SubCategory { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;

    public static Func<Sentence, SentenceFlat> FromSentence =>
        sentence => new SentenceFlat
        {
            Sentence = sentence.SentenceOriginal, 
            Category = sentence.Category, 
            SubCategory = sentence.SubCategory,
            Context = sentence.Context
        };

    public Sentence ToSentence(ViewModelPhrases vm) =>
        new()
        {
            SentenceOriginal = Sentence,
            Phrases = [..vm.GetPhrases(Sentence)],
            Category = Category,
            SubCategory = SubCategory,
            Context = Context,
        };
}