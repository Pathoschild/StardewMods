using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the display of debug information.</summary>
        public string ToggleDebug { get; set; } = SButton.F3.ToString();

        /// <summary>The key which switches to the previous texture.</summary>
        public string DebugPrevTexture { get; set; } = SButton.LeftControl.ToString();

        /// <summary>The key which switches to the next texture.</summary>
        public string DebugNextTexture { get; set; } = SButton.RightControl.ToString();


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                toggleDebug: CommonHelper.ParseButtons(this.ToggleDebug, monitor, nameof(this.ToggleDebug)),
                debugPrevTexture: CommonHelper.ParseButtons(this.DebugPrevTexture, monitor, nameof(this.DebugPrevTexture)),
                debugNextTexture: CommonHelper.ParseButtons(this.DebugNextTexture, monitor, nameof(this.DebugNextTexture))
            );
        }
    }
}
