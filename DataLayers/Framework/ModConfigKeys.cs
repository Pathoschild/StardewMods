using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the data layer overlay.</summary>
        public SButton[] ToggleLayer { get; }

        /// <summary>The key which cycles backwards through data layers.</summary>
        public SButton[] PrevLayer { get; }

        /// <summary>The key which cycles forward through data layers.</summary>
        public SButton[] NextLayer { get; }



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleLayer">The key which toggles the data layer overlay.</param>
        /// <param name="prevLayer">The key which cycles backwards through data layers.</param>
        /// <param name="nextLayer">The key which cycles forward through data layers.</param>
        public ModConfigKeys(SButton[] toggleLayer, SButton[] prevLayer, SButton[] nextLayer)
        {
            this.ToggleLayer = toggleLayer;
            this.PrevLayer = prevLayer;
            this.NextLayer = nextLayer;
        }
    }
}
