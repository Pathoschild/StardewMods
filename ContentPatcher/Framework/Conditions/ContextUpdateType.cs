using System;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>Indicates a context update type.</summary>
    [Flags]
    public enum ContextUpdateType
    {
        /// <summary>The current player changed location.</summary>
        OnLocationChange = UpdateRate.OnLocationChange,

        /// <summary>All update types.</summary>
        All = UpdateRate.OnDayStart | UpdateRate.OnLocationChange
    }
}
