using System;
using System.Collections.Generic;
using System.Linq;

namespace Descript.Models;

public enum ConfidenceLevel
{
    Confirmed,
    High,
    Medium,
    Low
}

public static class ConfidenceLevelEx
{
    public static IEnumerable<ConfidenceLevel> All => 
        Enum.GetValues<ConfidenceLevel>().Except([ConfidenceLevel.Confirmed]);
}