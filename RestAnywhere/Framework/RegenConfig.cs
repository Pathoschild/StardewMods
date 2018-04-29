namespace RestAnywhere.Framework
{
    /// <summary>The regen configuration for each action in a given period.</summary>
    internal class RegenConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The regeneration per time when resting.</summary>
        public float Rest { get; set; }

        /// <summary>The regeneration per time when running.</summary>
        public float Run { get; set; }
    }
}
