using System.Text.Json.Serialization;

namespace Descript.Models;

[method: JsonConstructor]
public class Rune(int id, string translation = "", ConfidenceLevel confidence = ConfidenceLevel.Low)
{
    [JsonIgnore]
    public const int CodePointStart = 0xE000;

    public int Id { get; } = id;
    public string Translation { get; set; } = translation.ToLower();
    public ConfidenceLevel Confidence { get; set; } = confidence;

    [JsonIgnore]
    public string Glyph { get; } = ((char)(CodePointStart + id)).ToString();

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