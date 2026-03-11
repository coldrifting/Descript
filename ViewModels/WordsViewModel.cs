using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Descript.Models;
using Descript.Utils;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public class WordsViewModel(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;

    private readonly Dictionary<int, Word> _allWords = new();

    public List<WordFlat> Words => _allWords.Values.Select(w => new WordFlat(w.Id)
        {
            Confidence = w.Confidence,
            Translation = w.Translation,
            Glyphs = GetAsString(w.Id)
        })
        .Where(w => w.Translation.ContainsTrimmed(FilterText))
        .OrderBy(w => w.Translation == "" ? "Ω" : w.Translation)
        .ThenBy(w => w.Confidence)
        .ToList();

    public string FilterText { get; set => SetField(ref field, value); } = string.Empty;

    public string GetAsString(int wordId)
    {
        return _allWords.TryGetValue(wordId, out Word? word) 
            ? ToString(word.RuneIds)
            : string.Empty;
    }

    public bool TryGet(int wordId, [MaybeNullWhen(false)] out Word word)
    {
        return _allWords.TryGetValue(wordId, out word);
    }

    public bool TryGet(string wordAsString, [MaybeNullWhen(false)] out Word word)
    {
        foreach (Word candidateWord in _allWords.Values
                     .Where(candidateWord => string.Equals(wordAsString, GetAsString(candidateWord.Id), StringComparison.Ordinal)))
        {
            word = candidateWord;
            return true;
        }

        word = null;
        return false;
    }
    
    public Word this[int wordId]
    {
        get => _allWords[wordId];
        set => _allWords[wordId] = value;
    }
    
    public void Add(IEnumerable<Word> words)
    {
        bool updated = false;
        foreach (Word word in words)
        {
            if (Add(word, false))
            {
                updated = true;
            }
        }

        if (updated)
        {
            OnPropertyChanged(nameof(Words));
        }
    }
    
    public bool Add(Word word, bool update = true)
    {
        if (word.Id == -1)
        {
            word.Id = GetNextWordIndex();
        }
        
        if (_allWords.TryAdd(word.Id, word))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Words));
            }

            return true;
        }

        Console.WriteLine($"Word with id {word.Id} already exists");
        return false;
    }

    public bool Add(string rawWord)
    {
        if (TryGet(rawWord, out Word? _))
        {
            return false;
        }
        
        return Add(new Word(GetNextWordIndex())
        {
            RuneIds = ToRuneIds(rawWord).ToList()
        });
    }

    public void Edit(int wordId, string newTranslation, ConfidenceLevel newConfidence)
    {
        if (_allWords.TryGetValue(wordId, out Word? word))
        {
            string oldTranslation = word.Translation;
            ConfidenceLevel oldConfidence = word.Confidence;
            
            word.Translation = newTranslation;
            word.Confidence = newConfidence;

            if (oldTranslation != newTranslation || oldConfidence != newConfidence)
            {
                OnPropertyChanged(nameof(Words));
            }
        }
        else
        {
            Console.WriteLine($"Word with id {wordId} does not exist");
        }
    }

    public static bool IsValidWord(string str)
    {
        return str.ToCharArray().All(c => Rune.CodePointStart <= c && c <= Rune.CodePointStart + 4096);
    }
    
    private static int[] ToRuneIds(string str)
    {
        return str.ToCharArray().Select(c => c - Rune.CodePointStart).ToArray();
    }
    
    private static string ToString(IEnumerable<int> runeIds)
    {
        return runeIds.Aggregate("", (s, i) => s + (char)(Rune.CodePointStart + i));
    }

    private int GetNextWordIndex()
    {
        return _allWords.Keys.OrderBy(k => k).LastOrDefault(-1) + 1;
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(FilterText):
                OnPropertyChanged(nameof(Words));
                break;
            
            case nameof(Words):
                Vm.Translations.UpdateTranslations();
                break;
        }
    }
}