using System.Collections.Generic;
using System.Text.Json.Serialization;
using Descript.Utils;

namespace Descript.Models;

[method:JsonConstructor]
public class Word(int id)
{
    public int Id { get; set; } = id;
    public string Translation { get; set => field = value.ToTitleCase(); } = string.Empty;
    public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Low;
    public List<int> RuneIds { get; set; } = [];
}

public class WordFlat(int id)
{
    public int Id { get; set; } = id;
    public string Glyphs { get; set; } = string.Empty;
    public string Translation { get; set => field = value.ToTitleCase(); } = string.Empty;
    public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Low;
}