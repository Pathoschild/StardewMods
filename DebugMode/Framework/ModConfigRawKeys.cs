using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DebugMode.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles debug mode.</summary>
        public string ToggleDebug { get; set; } = SButton.OemTilde.ToString();


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                toggleDebug: CommonHelper.ParseButtons(this.ToggleDebug, monitor, nameof(this.ToggleDebug))
            );
        }
    }
}
