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

        /// <summary>The key binding which switches to this layer when the overlay is open.</summary>
        public string ShortcutKey { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Whether the data layer should be enabled.</summary>
        public bool IsEnabled()
        {
            return this.Enabled && this.UpdatesPerSecond > 0;
        }
    }
}
