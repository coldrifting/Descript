using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Descript.ViewModels.Base;
using Rune = Descript.Models.Rune;

namespace Descript.ViewModels;

public partial class TranslationsViewModel(MainWindowViewModel mainWindowViewModel) : ViewModelBase
{
    private MainWindowViewModel Vm { get; } = mainWindowViewModel;

    private readonly Dictionary<int, Translation> _allTranslations = new();

    private List<Translation> Translations => _allTranslations.Values.ToList();
    public List<TranslationBlocks> Blocks { get; private set => SetField(ref field, value); } = [];
    
    // Add Sentence Dialog
    private int _sentenceId = -1;
    public bool IsSentenceDialogOpen { get; set => SetField(ref field, value); }
    public string SentenceEntry { get; set => SetField(ref field, value); } = "";
    public bool IsSentenceValid { get; set => SetField(ref field, value); }
    public int SelectionStart { get; set => SetField(ref field, value); }
    public int SelectionEnd { get; set => SetField(ref field, value); }

    [RelayCommand]
    private void OpenAddSentenceDialog()
    {
        _sentenceId = -1;
        SentenceEntry = "";
        IsSentenceDialogOpen = true;
    }
    
    [RelayCommand]
    private void OpenEditSentenceDialog(int id)
    {
        _sentenceId = id;
        SentenceEntry = GetSentence(id);
        IsSentenceDialogOpen = true;
    }

    public void InsertIntoSentenceInput(string input)
    {
        if (SelectionStart == SelectionEnd)
        {
            SentenceEntry = SentenceEntry.Insert(Math.Min(SelectionStart, SentenceEntry.Length), input);
            SelectionEnd = SelectionStart + 1;
            SelectionStart = SelectionEnd;
        }
        else
        {
            SentenceEntry = SelectionEnd > SelectionStart
                ? SentenceEntry.Remove(SelectionStart, SelectionEnd - SelectionStart).Insert(SelectionStart, input)
                : SentenceEntry.Remove(SelectionEnd, SelectionStart - SelectionEnd).Insert(SelectionEnd, input);
            
            SelectionStart = Math.Min(SelectionStart, SelectionEnd) + 1;
            SelectionEnd = SelectionStart;
        }
    }

    private string GetSentence(int id)
    {
        StringBuilder sb = new();
        if (_allTranslations.TryGetValue(id, out Translation? translation))
        {
            foreach (int wordId in translation.WordIds)
            {
                if (wordId < 0)
                {
                    sb.Append(translation.PlainWords[wordId * -1 - 1]);
                }
                else
                {
                    // Build rune chain
                    if (Vm.Words.TryGet(wordId, out Word? word))
                    {
                        foreach (int valueRuneId in word.RuneIds)
                        {
                            sb.Append((char)(valueRuneId + Rune.CodePointStart));
                        }
                    }
                }

                sb.Append(' ');
            }
        }

        if (sb.Length > 0)
        {
            sb.Length--;
        }
        
        return sb.ToString();
    }

    [RelayCommand]
    private void SubmitSentence()
    {
        IsSentenceDialogOpen = false;
        
        Translation t = ConvertFromSentence(SentenceEntry.Trim());

        if (_sentenceId == -1)
        {
            _allTranslations.Add(t.Id, t);
        }
        else
        {
            _allTranslations[_sentenceId] = t;
        }
        
        OnPropertyChanged(nameof(Translations));
    }
    
    private Translation ConvertFromSentence(string sentence)
    {
        List<int> indices = [];
        List<string> plainWordStrings = [];
        
        int plainWordIndex = 0;
        foreach (string str in sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (WordsViewModel.IsValidWord(str))
            {
                Vm.Words.Add(str);
                
                if (Vm.Words.TryGet(str, out Word? word))
                {
                    indices.Add(word.Id);
                }
            }
            else
            {
                plainWordIndex--;
                plainWordStrings.Add(str);
                indices.Add(plainWordIndex);
            }

        }

        return _sentenceId != -1 
            ? new Translation(_sentenceId, indices, plainWordStrings) 
            : new Translation(GetNextTranslationIndex(), indices, plainWordStrings);
    }
    
    public bool Add(Translation translation, bool update = true)
    {
        if (_allTranslations.TryAdd(translation.Id, translation))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Translations));
            }

            return true;
        }

        Console.WriteLine($"Translation with id {translation.Id} already exists");
        return false;
    }

    public void Add(IEnumerable<Translation> translations)
    {
        bool updated = false;
        foreach (Translation translation in translations)
        {
            if (Add(translation, false))
            {
                updated = true;
            }
        }

        if (updated)
        {
            OnPropertyChanged(nameof(Translations));
        }
    }

    [RelayCommand]
    private void DeleteSentence(int translationId)
    {
        _allTranslations.Remove(translationId);
        OnPropertyChanged(nameof(Translations));
    }
    
    public void UpdateTranslations()
    {
        List<TranslationBlocks> allBlocks = [];
        foreach (Translation translation in Translations)
        {
            TranslationBlocks blocks = new(translation.Id);
            foreach (int translationWordId in translation.WordIds)
            {
                // Raw english words
                if (translationWordId < 0)
                {
                    string plainText = translation.PlainWords[(translationWordId * -1) - 1];
                    blocks.Blocks.Add(new TranslationBlock(-1, ConfidenceLevel.Confirmed, plainText, plainText, [
                        new TranslationBlockItem(-1, ConfidenceLevel.Low, plainText, plainText)
                    ]));
                }
                else
                {
                    string rawText = "";
                    List<TranslationBlockItem> symbols = [];
                    
                    foreach (int runeId in Vm.Words[translationWordId].RuneIds)
                    {
                        rawText += ((char)(runeId + Rune.CodePointStart)).ToString();

                        symbols.Add(Vm.Runes.TryGet(runeId, out Rune? rune)
                            ? new TranslationBlockItem(rune.Id, rune.Confidence, rune.Glyph, rune.Translation == "" ? "?" : rune.Translation)
                            : new TranslationBlockItem(-1, ConfidenceLevel.Low, "", ""));
                    }
                    
                    var translatedText = Vm.Words[translationWordId].Translation;
                    if (translatedText == "")
                    {
                        translatedText = new string('?', Vm.Words[translationWordId].RuneIds.Count);
                    }
                    
                    blocks.Blocks.Add(new TranslationBlock(translationWordId, Vm.Words[translationWordId].Confidence ,rawText, translatedText, symbols));
                }
            }
            allBlocks.Add(blocks);
        }
        Blocks = allBlocks;
    }

    private bool IsSentenceOkay()
    {
        foreach (Translation translation in Translations)
        {
            if (GetSentence(translation.Id) == SentenceEntry.Trim())
            {
                return false;
            }
        }
        
        return SentenceEntry.Trim().Length != 0;
    }

    private int GetNextTranslationIndex()
    {
        return _allTranslations.Keys.OrderBy(k => k).LastOrDefault(-1) + 1;
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        if (e.PropertyName == nameof(Translations))
        {
            UpdateTranslations();
        }

        if (e.PropertyName is nameof(SentenceEntry))
        {
            IsSentenceValid = IsSentenceOkay();
        }
    }
}