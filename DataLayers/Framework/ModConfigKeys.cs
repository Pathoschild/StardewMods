using Pathoschild.Stardew.Common.Input;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the data layer overlay.</summary>
        public KeyBinding ToggleLayer { get; }

        /// <summary>The keys which cycle backwards through data layers.</summary>
        public KeyBinding PrevLayer { get; }

        /// <summary>The keys which cycle forward through data layers.</summary>
        public KeyBinding NextLayer { get; }



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggleLayer">The keys which toggle the data layer overlay.</param>
        /// <param name="prevLayer">The keys which cycle backwards through data layers.</param>
        /// <param name="nextLayer">The keys which cycle forward through data layers.</param>
        public ModConfigKeys(KeyBinding toggleLayer, KeyBinding prevLayer, KeyBinding nextLayer)
        {
            this.ToggleLayer = toggleLayer;
            this.PrevLayer = prevLayer;
            this.NextLayer = nextLayer;
        }
    }
}
