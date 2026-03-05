using System.Collections.Generic;

namespace Descript.Models;

public class Sentence
{
    public List<int> WordIds { get; set; } = [];
    public string Context { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
}