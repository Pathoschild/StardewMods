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

        /// <summary>The key bindings.</summary>
        public ModConfigRawKeys Controls { get; set; } = new ModConfigRawKeys();
    }
}
