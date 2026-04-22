using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Descript.Data;
using Descript.Interfaces;
using Descript.Models;
using Descript.Utils;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public class ViewModelRuneChain(ViewModelMainWindow viewModelMainWindow) : ViewModelBase, ILoadSave
{
    private ViewModelMainWindow Vm { get; } = viewModelMainWindow;

    private readonly Dictionary<string, RuneChain> _allWords = new();

    public IEnumerable<RuneChain> Words => _allWords.Values.OrderBy(word => word.Glyphs);
    public IList<RuneChain> WordsFiltered => Words
        .Where(word => word.Translation.ContainsTrimmed(FilterText))
        .OrderBy(word => word.Translation == "" ? "Ω" : word.Translation)
        .ThenBy(word => word.Confidence)
        .ToList();

    public string FilterText { get; set => SetField(ref field, value); } = string.Empty;
    
    public void Load()
    {
        Add(DataManagement.Load<RuneChain>());
    }

    public void Save()
    {
        DataManagement.Save(Words);
    }
    
    public RuneChain? this[string index] => _allWords.GetValueOrDefault(index);

    public bool TryGet(string word, [MaybeNullWhen(false)] out RuneChain runeChain)
    {
        runeChain = _allWords.Values.FirstOrDefault(runeChain => runeChain.Equals(word));
        return runeChain is not null;
    }
    
    public void Add(IEnumerable<RuneChain> words)
    {
        bool updated = false;
        foreach (RuneChain word in words)
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
    
    public bool Add(RuneChain runeChain, bool update = true)
    {
        if (_allWords.TryAdd(runeChain.Glyphs, runeChain))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Words));
            }

            return true;
        }

        Console.WriteLine($"Word {runeChain.Glyphs} already exists");
        return false;
    }

    public bool Add(string rawWord)
    {
        if (TryGet(rawWord, out RuneChain? _))
        {
            return false;
        }

        return IsValidWord(rawWord) && Add(RuneChain.FromString(rawWord));
    }

    public void Edit(string wordRaw, string newTranslation, ConfidenceLevel newConfidence)
    {
        if (_allWords.TryGetValue(wordRaw, out RuneChain? word))
        {
            string oldTranslation = word.Translation;
            ConfidenceLevel oldConfidence = word.Confidence;

            if (oldTranslation != newTranslation || oldConfidence != newConfidence)
            {
                _allWords[word.Glyphs] = word with { Translation = newTranslation, Confidence = newConfidence };

                OnPropertyChanged(nameof(Words));
            }
        }
        else
        {
            Console.WriteLine($"Word {word?.Glyphs ?? "(null)"} does not exist");
        }
    }

    public void Remove(string rawWord)
    {
        if (TryGet(rawWord, out RuneChain? chain))
        {
            if (chain.Translation.Trim() == "")
            {
                _allWords.Remove(rawWord);
                OnPropertyChanged(nameof(Words));
            }
        }
    }

    public static bool IsValidWord(string str)
    {
        return str.ToCharArray().All(c => Rune.CodePointStart <= c && c <= Rune.CodePointStart + 4096);
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(FilterText):
                OnPropertyChanged(nameof(WordsFiltered));
                break;
            
            case nameof(Words):
                Vm.ViewModelRuneSentence.Refresh();
                break;
        }
    }
}