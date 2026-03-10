using System.Collections.Generic;
using System.Text.Json.Serialization;
using Descript.Utils;

namespace Descript.Models;

[method:JsonConstructor]
public class RuneChain(int id)
{
    public int Id { get; set; } = id;
    public string Translation { get; set => field = value.ToTitleCase(); } = string.Empty;
    public ConfidenceLevel Confidence { get; set; } = ConfidenceLevel.Low;
    public List<int> RuneIds { get; set; } = [];
}