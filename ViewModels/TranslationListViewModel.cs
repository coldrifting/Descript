using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using Descript.Models;
using Rune = Descript.Models.Rune;

namespace Descript.ViewModels;

public enum DialogType
{
    EditRune,
    EditWord,
}

public enum CaseConversion
{
    Uppercase,
    Lowercase,
    Titlecase,
    None
}

public partial class TranslationListViewModel(MainWindowViewModel mainWindowViewModel) : INotifyPropertyChanged
{
    private MainWindowViewModel MainWindowViewModel { get; set; } = mainWindowViewModel;

    private readonly Dictionary<int, Translation> _allTranslations = new();
    private readonly Dictionary<int, RuneChain> _allWords = new();

    private List<Translation> Translations => _allTranslations.Values.ToList();
    public List<TranslationBlocks> Blocks { get; private set => SetField(ref field, value); } = [];

    // Edit Rune/Word Dialog
    public bool IsDialogOpen { get; set => SetField(ref field, value); } = false;
    public bool IsDialogOkay { get; set => SetField(ref field, value); } = false;

    private DialogType DialogType { get; set => SetField(ref field, value); } = DialogType.EditRune;

    public string DialogTitle => DialogType switch
    {
        DialogType.EditRune => "Edit Rune",
        DialogType.EditWord => "Edit Word",
        _ => "Dialog"
    };
    public CaseConversion TranslationCase => DialogType == DialogType.EditRune 
        ? CaseConversion.Lowercase 
        : CaseConversion.Titlecase;

    public string DialogEntry1 { get; set => SetField(ref field, value); } = "";
    public ConfidenceLevel DialogEntry2 { get; set => SetField(ref field, value); } = ConfidenceLevel.Medium;
    private int _dialogId = -1;

    [RelayCommand]
    private void OpenRuneDialog(int runeId)
    {
        _dialogId = runeId;

        if (MainWindowViewModel.RunesList.TryGet(runeId, out Rune? rune))
        {
            DialogType = DialogType.EditRune;
            DialogEntry1 = rune.Translation;
            DialogEntry2 = rune.Confidence;
        }
        
        IsDialogOpen = true;
    }

    [RelayCommand]
    private void OpenWordDialog(int wordId)
    {
        if (wordId < 0)
        {
            return;
        }
        
        _dialogId = wordId;

        if (_allWords.TryGetValue(wordId, out RuneChain? word))
        {
            DialogType = DialogType.EditWord;
            DialogEntry1 = word.Translation;
            DialogEntry2 = word.Confidence;
        }
        else
        {
            DialogEntry2 = ConfidenceLevel.Low;
        }
        
        IsDialogOpen = true;
    }
    
    [RelayCommand]
    private void CloseDialog()
    {
        switch (DialogType)
        {
            case DialogType.EditWord:
                if (_allWords.TryGetValue(_dialogId, out RuneChain? word))
                {
                    word.Translation = DialogEntry1;
                    word.Confidence = DialogEntry2;
                }
                break;
            
            case DialogType.EditRune:
                MainWindowViewModel.RunesList.Edit(_dialogId, DialogEntry1, DialogEntry2);
                break;
        }

        OnPropertyChanged(nameof(Translations));
        IsDialogOpen = false;
    }
    
    // Add Sentence Dialog
    private int _sentenceId = -1;
    public bool IsSentenceDialogOpen { get; set => SetField(ref field, value); }
    public string SentenceEntry { get; set => SetField(ref field, value); } = "";
    
    public bool IsSentenceValid { get; set => SetField(ref field, value); } = false;

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

    private string GetSentence(int id)
    {
        StringBuilder sb = new();
        if (_allTranslations.TryGetValue(id, out Translation? translation))
        {
            foreach (int runeChainId in translation.RuneChainIds)
            {
                if (runeChainId < 0)
                {
                    sb.Append(translation.PlainWords[(runeChainId * -1) - 1]);
                }
                else
                {
                    // Build rune chain
                    if (_allWords.TryGetValue(runeChainId, out RuneChain? word))
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
        
        Translation t = ConvertFromSentence(SentenceEntry.Trim(), _sentenceId);

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
    
    private Translation ConvertFromSentence(string sentence, int id)
    {
        // Index, isPlain, plainWord/RuneChainIndex
        List<int> indices = [];

        List<string> plainWordStrings = [];
        
        int plainWordIndex = 0;
        foreach (string str in sentence.Split(' '))
        {
            if (str.ToCharArray().All(c => Rune.CodePointStart <= c && c <= Rune.CodePointStart + 4096))
            {
                int[] ids = str.ToCharArray().Select(c => c - Rune.CodePointStart).ToArray();

                RuneChain? word = _allWords.Values.FirstOrDefault(w => w.RuneIds.SequenceEqual(ids));

                if (word != null)
                {
                    indices.Add(word.Id);
                }
                else
                {
                    int indexToInsert = _allWords.Keys.OrderBy(k => k).LastOrDefault(-1) + 1;
                    _allWords.Add(indexToInsert, new RuneChain(indexToInsert)
                    {
                        RuneIds = ids.ToList()
                    });
                    indices.Add(indexToInsert);
                }
            }
            else
            {
                plainWordIndex--;
                plainWordStrings.Add(str);
                indices.Add(plainWordIndex);
            }

        }

        if (_sentenceId != -1)
        {
            return new Translation(_sentenceId, indices, plainWordStrings);
        }
        
        // Get next id
        int translationId = _allTranslations.Keys.OrderBy(k => k).LastOrDefault(-1) + 1;
        return new Translation(translationId, indices, plainWordStrings);
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
    
    
    public bool AddWord(RuneChain word, bool update = true)
    {
        if (_allWords.TryAdd(word.Id, word))
        {
            if (update)
            {
                OnPropertyChanged(nameof(Translations));
            }

            return true;
        }

        Console.WriteLine($"Word with id {word.Id} already exists");
        return false;
    }

    public void AddWord(IEnumerable<RuneChain> words)
    {
        bool updated = false;
        foreach (RuneChain word in words)
        {
            if (AddWord(word, false))
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
        List<TranslationBlocks> allBlocks = new();
        foreach (Translation translation in Translations)
        {
            TranslationBlocks blocks = new(translation.Id);
            foreach (int translationRuneChainId in translation.RuneChainIds)
            {
                // Raw english words
                if (translationRuneChainId < 0)
                {
                    string plainText = translation.PlainWords[(translationRuneChainId * -1) - 1];
                    blocks.Blocks.Add(new TranslationBlock(-1, ConfidenceLevel.Confirmed, plainText, plainText, [
                        new TranslationBlockItem(-1, ConfidenceLevel.Low, plainText, plainText)
                    ]));
                }
                else
                {
                    string rawText = "";
                    List<TranslationBlockItem> symbols = [];
                    
                    foreach (int runeId in _allWords[translationRuneChainId].RuneIds)
                    {
                        rawText += ((char)(runeId + Rune.CodePointStart)).ToString();

                        symbols.Add(MainWindowViewModel.RunesList.TryGet(runeId, out Rune? rune)
                            ? new TranslationBlockItem(rune.Id, rune.Confidence, rune.Glyph, rune.Translation == "" ? "?" : rune.Translation)
                            : new TranslationBlockItem(-1, ConfidenceLevel.Low, "", ""));
                    }
                    
                    var translatedText = _allWords[translationRuneChainId].Translation;
                    if (translatedText == "")
                    {
                        translatedText = new string('?', _allWords[translationRuneChainId].RuneIds.Count);
                    }
                    
                    blocks.Blocks.Add(new TranslationBlock(translationRuneChainId, _allWords[translationRuneChainId].Confidence ,rawText, translatedText, symbols));
                }
            }
            allBlocks.Add(blocks);
        }
        Blocks = allBlocks;
    }

    private bool IsEditOkay()
    {
        return !(DialogEntry1.Trim().Length == 0 && DialogEntry2 != ConfidenceLevel.Low);
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
    
    // INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName == nameof(Translations))
        {
            UpdateTranslations();
        }

        if (propertyName is nameof(DialogEntry1) or nameof(DialogEntry2))
        {
            IsDialogOkay = IsEditOkay();
        }

        if (propertyName is nameof(SentenceEntry))
        {
            IsSentenceValid = IsSentenceOkay();
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}