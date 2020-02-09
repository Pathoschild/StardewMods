using Pathoschild.Stardew.Common.Input;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the display of debug information.</summary>
        public KeyBinding ToggleDebug { get; }

        /// <summary>The keys which switch to the previous texture.</summary>
        public KeyBinding DebugPrevTexture { get; }

        /// <summary>The keys which switch to the next texture.</summary>
        public KeyBinding DebugNextTexture { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleDebug">The keys which toggle the display of debug information.</param>
        /// <param name="debugPrevTexture">The keys which switch to the previous texture.</param>
        /// <param name="debugNextTexture">The keys which switch to the next texture.</param>
        public ModConfigKeys(KeyBinding toggleDebug, KeyBinding debugPrevTexture, KeyBinding debugNextTexture)
        {
            this.ToggleDebug = toggleDebug;
            this.DebugPrevTexture = debugPrevTexture;
            this.DebugNextTexture = debugNextTexture;
        }
    }
}
