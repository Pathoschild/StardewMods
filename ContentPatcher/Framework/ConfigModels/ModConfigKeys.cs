using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the display of debug information.</summary>
        public KeybindList ToggleDebug { get; }

        /// <summary>The keys which switch to the previous texture.</summary>
        public KeybindList DebugPrevTexture { get; }

        /// <summary>The keys which switch to the next texture.</summary>
        public KeybindList DebugNextTexture { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ModConfigKeys()
            : this(null, null, null) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="toggleDebug">The keys which toggle the display of debug information.</param>
        /// <param name="debugPrevTexture">The keys which switch to the previous texture.</param>
        /// <param name="debugNextTexture">The keys which switch to the next texture.</param>
        [JsonConstructor]
        public ModConfigKeys(KeybindList? toggleDebug, KeybindList? debugPrevTexture, KeybindList? debugNextTexture)
        {
            this.ToggleDebug = toggleDebug ?? new(SButton.F3);
            this.DebugPrevTexture = debugPrevTexture ?? new(SButton.LeftControl);
            this.DebugNextTexture = debugNextTexture ?? new(SButton.RightControl);
        }
    }
}
