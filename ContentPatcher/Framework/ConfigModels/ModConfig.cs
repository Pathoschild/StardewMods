namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to enable debug features.</summary>
        public bool EnableDebugFeatures { get; set; }

        /// <summary>Whether to apply changes from each content pack in a separate operation.</summary>
        public bool GroupEditsByMod { get; set; } = true;

        /// <summary>The key bindings.</summary>
        public ModConfigKeys Controls { get; set; } = new();
    }
}
