using StardewModdingAPI;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the display of debug information.</summary>
        public SButton[] ToggleDebug { get; }

        /// <summary>The key which switches to the previous texture.</summary>
        public SButton[] DebugPrevTexture { get; }

        /// <summary>The key which switches to the next texture.</summary>
        public SButton[] DebugNextTexture { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleDebug">The key which toggles the display of debug information.</param>
        /// <param name="debugPrevTexture">The key which switches to the previous texture.</param>
        /// <param name="debugNextTexture">The key which switches to the next texture.</param>
        public ModConfigKeys(SButton[] toggleDebug, SButton[] debugPrevTexture, SButton[] debugNextTexture)
        {
            this.ToggleDebug = toggleDebug;
            this.DebugPrevTexture = debugPrevTexture;
            this.DebugNextTexture = debugNextTexture;
        }
    }
}
