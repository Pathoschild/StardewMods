using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Configures the settings for a data layer.</summary>
    internal class LayerConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to enable this data layer.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>The number of updates needed per second.</summary>
        public decimal UpdatesPerSecond { get; set; } = 60;

        /// <summary>Whether to update the layer when the player's tile view changes.</summary>
        public bool UpdateWhenViewChange { get; set; } = true;

        /// <summary>The buttons to activate the layer.</summary>
        public string LayerShortcutButtons { get; set; }

        /*********
        ** Public methods
        *********/
        /// <summary>Whether the data layer should be enabled.</summary>
        public bool IsEnabled()
        {
            return this.Enabled && this.UpdatesPerSecond > 0;
        }

        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public SButton[] ParseControls(IMonitor monitor, string layerName)
        {
            return CommonHelper.ParseButtons(this.LayerShortcutButtons, monitor, layerName);
        }
    }
}
