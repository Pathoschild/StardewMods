namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A context update type.</summary>
    public enum ContextUpdateType
    {
        /// <summary>All patches should be updated.</summary>
        All = -1,

        /// <summary>The in-game clock changed.</summary>
        OnTimeChange = UpdateRate.OnTimeChange,

        /// <summary>The current player changed location.</summary>
        OnLocationChange = UpdateRate.OnLocationChange
    }
}
