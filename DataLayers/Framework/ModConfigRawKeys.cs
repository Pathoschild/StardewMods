using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the data layer overlay.</summary>
        public string ToggleLayer { get; set; } = SButton.F2.ToString();

        /// <summary>The key which cycles backwards through data layers.</summary>
        public string PrevLayer { get; set; } = $"{SButton.LeftControl}, {SButton.LeftShoulder}";

        /// <summary>The key which cycles forward through data layers.</summary>
        public string NextLayer { get; set; } = $"{SButton.RightControl}, {SButton.RightShoulder}";


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                toggleLayer: CommonHelper.ParseButtons(this.ToggleLayer, monitor, nameof(this.ToggleLayer)),
                prevLayer: CommonHelper.ParseButtons(this.PrevLayer, monitor, nameof(this.PrevLayer)),
                nextLayer: CommonHelper.ParseButtons(this.NextLayer, monitor, nameof(this.NextLayer))
            );
        }
    }
}
