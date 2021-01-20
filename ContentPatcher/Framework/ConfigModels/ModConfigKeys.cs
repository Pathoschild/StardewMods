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
        public KeybindList ToggleDebug { get; set; } = KeybindList.ForSingle(SButton.F3);

        /// <summary>The keys which switch to the previous texture.</summary>
        public KeybindList DebugPrevTexture { get; set; } = KeybindList.ForSingle(SButton.LeftControl);

        /// <summary>The keys which switch to the next texture.</summary>
        public KeybindList DebugNextTexture { get; set; } = KeybindList.ForSingle(SButton.RightControl);
    }
}
