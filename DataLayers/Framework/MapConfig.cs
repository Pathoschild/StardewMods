namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Configures the settings for a data map.</summary>
    internal class MapConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to enable this data map.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>The number of updates per second for each map.</summary>
        public decimal UpdatesPerSecond { get; set; } = 60;

        /// <summary>Whether to update the map when the player's tile view changes.</summary>
        public bool UpdateWhenViewChange { get; set; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Whether the data map should be enabled.</summary>
        public bool IsEnabled()
        {
            return this.Enabled && this.UpdatesPerSecond > 0;
        }
    }
}
