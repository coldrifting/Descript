using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Descript.Data;
using Descript.Models;
using Descript.Models.Flat;
using Descript.ViewModels.Base;

namespace Descript.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ViewModelElement ViewModelElement { get; }
    public ViewModelPhrases ViewModelPhrases { get; }
    public ViewModelSentences ViewModelSentences { get; }
    
    public MainWindowViewModel()
    {
        ViewModelElement = new ViewModelElement(this);
        ViewModelPhrases = new ViewModelPhrases(this);
        ViewModelSentences = new ViewModelSentences(this);
        
        LoadData();
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

