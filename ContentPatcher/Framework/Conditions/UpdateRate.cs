using System;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>Indicates when a patch should be updated.</summary>
    [Flags]
    internal enum UpdateRate
    {
        /// <summary>The patch updates once at the start of each day.</summary>
        OnDayStart = 1,

        /// <summary>The patch updates each time the current player changes location, and at the start of each day.</summary>
        OnLocationChange = 2,

        /// <summary>The patch updates each time the in-game clock changes.</summary>
        OnTimeChange = 4
    }
}
