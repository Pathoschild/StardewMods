using System;

namespace Pathoschild.Stardew.CentralStation;

/// <summary>The interconnected networks that join all the stops of a given type.</summary>
[Flags]
public enum StopNetworks
{
    /// <summary>The stop can be reached by train.</summary>
    Train = 1,

    /// <summary>The stop can be reached by bus.</summary>
    Bus = 2,

    /// <summary>The stop can be reached by boat.</summary>
    Boat = 4
}
