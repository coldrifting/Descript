using System;
using System.Linq;
using System.Timers;
using CommunityToolkit.Mvvm.Input;
using Descript.Data;
using Descript.Models;
using Descript.Models.Flat;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Timer _saveTimer = new()
    {
        Interval = 1000 * 60 * 5 // Save every 5 Minutes just in case
    };
    
    public ViewModelElement ViewModelElement { get; }
    public ViewModelPhrases ViewModelPhrases { get; }
    public ViewModelSentences ViewModelSentences { get; }
    
    public MainWindowViewModel()
    {
        ViewModelElement = new ViewModelElement(this);
        ViewModelPhrases = new ViewModelPhrases(this);
        ViewModelSentences = new ViewModelSentences(this);
        
        LoadData();
        
        _saveTimer.Elapsed += (_, _) => SaveData();
        _saveTimer.Enabled = true;
    }

    private void LoadData()
    {
        Translations translations = DataManagement.Load();

        ViewModelElement.Add(translations.Elements);
        ViewModelPhrases.Add(translations.Phrases);
        ViewModelSentences.Add(translations.Sentences);
    }

    public void SaveData()
    {
        Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Saving Data to File...");
        Translations translations = new()
        {
            Elements = ViewModelElement.Elements.Where(Element.ShouldSave).Select(ElementFlat.FromElement).ToArray(),
            Phrases = ViewModelPhrases.Phrases.Where(phrase => phrase.HasTranslation).Select(PhraseFlat.FromPhrase).ToArray(),
            Sentences = ViewModelSentences.Sentences.OrderBy(sentence => sentence.SentenceOriginal).Select(SentenceFlat.FromSentence).ToArray()
        };
        
        DataManagement.Save(translations);
    }
    
    public bool IsRuneListShown { get; set => SetField(ref field, value); } = true;
    
    [RelayCommand]
    private void ShowRunesList(bool shouldShowRunesList)
    {
        IsRuneListShown = shouldShowRunesList;
        
        ViewModelElement.UpdateIsShown();
        ViewModelPhrases.UpdateIsShown();
    }
}

