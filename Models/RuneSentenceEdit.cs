using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Descript.Models;

public class RuneSentenceEdit: ObservableObject
{
    public IEnumerable<string> AllSentences { get; init; } = [];

    public string OriginalSentence { get; init; } = string.Empty;
    public string OriginalCategory { get; init; } = string.Empty;
    public string OriginalSubCategory { get; init; } = string.Empty;
    public string OriginalContext { get; init; } = string.Empty;
    
    public string Sentence { get; set => SetField(ref field, value); } = string.Empty;
    public string Category { get; set => SetField(ref field, value); } = string.Empty;
    public string SubCategory { get; set => SetField(ref field, value); } = string.Empty;
    public string Context { get; set => SetField(ref field, value); } = string.Empty;

    public string SubmitButtonText => OriginalSentence.Trim() == "" ? "Add" : "Update";
    
    public bool IsValid =>
        Sentence.Trim().Length > 0 &&
        (OriginalSentence.Trim() != Sentence.Trim()
            ? !AllSentences.Contains(Sentence.Trim())
            : OriginalSentence.Trim() != Sentence.Trim() ||
              OriginalCategory.Trim() != Category.Trim() ||
              OriginalSubCategory.Trim() != SubCategory.Trim() ||
              OriginalContext.Trim() != Context);

    public RuneSentence ToRuneSentence()
    {
        return new RuneSentence
        {
            Sentence = Sentence.Trim(),
            Category = Category.Trim(),
            SubCategory = SubCategory.Trim(),
            Context = Context.Trim()
        };
    }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(Sentence) or nameof(Category) or nameof(SubCategory) or nameof(Context))
        {
            OnPropertyChanged(nameof(IsValid));
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }
        field = value;
        OnPropertyChanged(propertyName);
    }
}