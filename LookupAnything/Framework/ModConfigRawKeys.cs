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
        /// <summary>The keys which toggle the lookup UI for something under the cursor.</summary>
        public string ToggleLookup { get; set; } = SButton.F1.ToString();

        /// <summary>The keys which toggle the search UI.</summary>
        public string ToggleSearch { get; set; } = $"{SButton.LeftShift} + {SButton.F1}";

        /// <summary>The keys which scroll up long content.</summary>
        public string ScrollUp { get; set; } = SButton.Up.ToString();

        /// <summary>The keys which scroll down long content.</summary>
        public string ScrollDown { get; set; } = SButton.Down.ToString();

        /// <summary>The keys which toggle the display of debug information.</summary>
        public string ToggleDebug { get; set; } = "";


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IInputHelper input, IMonitor monitor)
        {
            return new ModConfigKeys(
                toggleLookup: CommonHelper.ParseButtons(this.ToggleLookup, input, monitor, nameof(this.ToggleLookup)),
                toggleSearch: CommonHelper.ParseButtons(this.ToggleSearch, input, monitor, nameof(this.ToggleSearch)),
                scrollUp: CommonHelper.ParseButtons(this.ScrollUp, input, monitor, nameof(this.ScrollUp)),
                scrollDown: CommonHelper.ParseButtons(this.ScrollDown, input, monitor, nameof(this.ScrollDown)),
                toggleDebug: CommonHelper.ParseButtons(this.ToggleDebug, input, monitor, nameof(this.ToggleDebug))
            );
        }
    }
}
