using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework.Models
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the automation overlay.</summary>
        public string ToggleOverlay { get; set; } = SButton.U.ToString();


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                toggleOverlay: CommonHelper.ParseButtons(this.ToggleOverlay, monitor, nameof(this.ToggleOverlay))
            );
        }
    }
}
