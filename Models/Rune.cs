using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Descript.Utils;

namespace Descript.Models;

public partial class Rune(int id) : ObservableObject
{
    private const int CodePointStart = 0xF2000;
    
    public int Id { get; } = id;
    public string Glyph { get; } = new System.Text.Rune(CodePointStart + id).ToString();

    [ObservableProperty] 
    public partial string Translation { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial bool ShowLabel { get; private set; }

    [ObservableProperty]
    public partial ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Low;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(Translation) or nameof(Confidence))
        {
            ShowLabel = Translation != string.Empty;
        }
    }

    [RelayCommand]
    private async Task Copy()
    {
        if (ClipboardHelper.GetClipboard() is not {} clipboard)
        {
            return;
        }

        await clipboard.SetTextAsync(Glyph);
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Rune rune && rune.Id == Id;
    }

    public override string ToString()
    {
        return $"Rune {Id}: ({Glyph}) - {Translation} : {Confidence}";
    }
}


public class RuneByIdComparer(int target) : IComparer<Rune>
{
    public int Compare(Rune? x, Rune? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (y is null) return 1;
        if (y.Id == target) return 1;
        if (x is null) return -1;
        if (x.Id == target) return -1;
        if (x.Id == y.Id) return 0;
        
        int xXor = x.Id ^ target;
        int yXor = y.Id ^ target;
        int xNumDifferingBits = BitOperations.PopCount((uint)xXor);
        int yNumDifferingBits = BitOperations.PopCount((uint)yXor);
        
        int bitsComparison = xNumDifferingBits.CompareTo(yNumDifferingBits);
        if (bitsComparison != 0) return bitsComparison;
        
        int idComparison = x.Id.CompareTo(y.Id);
        return idComparison;
    }
}

public class RuneByTranslationComparer(int target) : IComparer<Rune>
{
    public int Compare(Rune? x, Rune? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (y is null) return 1;
        if (y.Id == target) return 1;
        if (x is null) return -1;
        if (x.Id == target) return -1;
        if (x.Id == y.Id) return 0;
        
        int translationComparison = string.Compare(x.Translation, y.Translation, StringComparison.InvariantCultureIgnoreCase);
        if (translationComparison != 0) return translationComparison;
        
        int confidenceComparison = y.Confidence.CompareTo(x.Confidence);
        if (confidenceComparison != 0) return confidenceComparison;

        int xXor = x.Id ^ target;
        int yXor = y.Id ^ target;
        int xNumDifferingBits = BitOperations.PopCount((uint)xXor);
        int yNumDifferingBits = BitOperations.PopCount((uint)yXor);
        
        int bitsComparison = xNumDifferingBits.CompareTo(yNumDifferingBits);
        if (bitsComparison != 0) return bitsComparison;
        
        int idComparison = x.Id.CompareTo(y.Id);
        return idComparison;
    }
}

public class RuneByConfidenceComparer(int target) : IComparer<Rune>
{
    public int Compare(Rune? x, Rune? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (y is null) return 1;
        if (y.Id == target) return 1;
        if (x is null) return -1;
        if (x.Id == target) return -1;
        if (x.Id == y.Id) return 0;
        
        int confidenceComparison = y.Confidence.CompareTo(x.Confidence);
        if (confidenceComparison != 0) return confidenceComparison;
        
        int translationComparison = string.Compare(x.Translation, y.Translation, StringComparison.InvariantCultureIgnoreCase);
        if (translationComparison != 0) return translationComparison;

        int xXor = x.Id ^ target;
        int yXor = y.Id ^ target;
        int xNumDifferingBits = BitOperations.PopCount((uint)xXor);
        int yNumDifferingBits = BitOperations.PopCount((uint)yXor);
        
        int bitsComparison = xNumDifferingBits.CompareTo(yNumDifferingBits);
        if (bitsComparison != 0) return bitsComparison;
        
        int idComparison = x.Id.CompareTo(y.Id);
        return idComparison;
    }
}