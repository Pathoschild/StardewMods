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
        /// <summary>The keys which toggle the data layer overlay.</summary>
        public string ToggleLayer { get; set; } = SButton.F2.ToString();

        /// <summary>The keys which cycle backwards through data layers.</summary>
        public string PrevLayer { get; set; } = $"{SButton.LeftControl}, {SButton.LeftShoulder}";

        /// <summary>The keys which cycle forward through data layers.</summary>
        public string NextLayer { get; set; } = $"{SButton.RightControl}, {SButton.RightShoulder}";


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IInputHelper input, IMonitor monitor)
        {
            return new ModConfigKeys(
                toggleLayer: CommonHelper.ParseButtons(this.ToggleLayer, input, monitor, nameof(this.ToggleLayer)),
                prevLayer: CommonHelper.ParseButtons(this.PrevLayer, input, monitor, nameof(this.PrevLayer)),
                nextLayer: CommonHelper.ParseButtons(this.NextLayer, input, monitor, nameof(this.NextLayer))
            );
        }
    }
}
