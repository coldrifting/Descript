using Descript.ViewModels.Base;

namespace Descript.Models;

public class PhraseBase(string glyphs) : ViewModelBase
{
    public string Glyphs { get; set => SetField(ref field, value); } = glyphs;
}