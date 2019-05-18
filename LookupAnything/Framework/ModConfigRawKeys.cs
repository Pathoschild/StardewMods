using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the lookup UI for something under the cursor.</summary>
        public string ToggleLookup { get; set; } = SButton.F1.ToString();

        /// <summary>The key which toggles the lookup UI for something in front of the player.</summary>
        public string ToggleLookupInFrontOfPlayer { get; set; } = "";

        /// <summary>The key which scrolls up long content.</summary>
        public string ScrollUp { get; set; } = SButton.Up.ToString();

        /// <summary>The key which scrolls down long content.</summary>
        public string ScrollDown { get; set; } = SButton.Down.ToString();

        /// <summary>The key which toggles the display of debug information.</summary>
        public string ToggleDebug { get; set; } = "";


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                toggleLookup: CommonHelper.ParseButtons(this.ToggleLookup, monitor, nameof(this.ToggleLookup)),
                toggleLookupInFrontOfPlayer: CommonHelper.ParseButtons(this.ToggleLookupInFrontOfPlayer, monitor, nameof(this.ToggleLookupInFrontOfPlayer)),
                scrollUp: CommonHelper.ParseButtons(this.ScrollUp, monitor, nameof(this.ScrollUp)),
                scrollDown: CommonHelper.ParseButtons(this.ScrollDown, monitor, nameof(this.ScrollDown)),
                toggleDebug: CommonHelper.ParseButtons(this.ToggleDebug, monitor, nameof(this.ToggleDebug))
            );
        }
    }
}
