using System.Collections.Generic;

namespace Descript.Models;

public class RuneChain
{
    public int Id { get; set; }
    public List<int> RuneIds { get; set; } = [];
    public string Translation { get; set; } = string.Empty;
    public ConfidenceLevel Confidence { get; set; }
}