using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Configures the settings for a data layer.</summary>
    internal class LayerConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to enable this data layer.</summary>
        public bool Enabled { get; }

        /// <summary>The number of updates needed per second.</summary>
        public decimal UpdatesPerSecond { get; }

        /// <summary>Whether to update the layer when the player's tile view changes.</summary>
        public bool UpdateWhenViewChange { get; }

        /// <summary>The key binding which switches to this layer when the overlay is open.</summary>
        public KeybindList ShortcutKey { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="enabled">Whether to enable this data layer.</param>
        /// <param name="updatesPerSecond">The number of updates needed per second.</param>
        /// <param name="updateWhenViewChange">Whether to update the layer when the player's tile view changes.</param>
        /// <param name="shortcutKey">The key binding which switches to this layer when the overlay is open.</param>
        public LayerConfig(bool? enabled = null, decimal? updatesPerSecond = null, bool? updateWhenViewChange = null, KeybindList? shortcutKey = null)
        {
            this.Enabled = enabled ?? true;
            this.UpdatesPerSecond = updatesPerSecond ?? 60;
            this.UpdateWhenViewChange = updateWhenViewChange ?? true;
            this.ShortcutKey = shortcutKey ?? new();
        }

        /// <summary>Whether the data layer should be enabled.</summary>
        public bool IsEnabled()
        {
            return this.Enabled && this.UpdatesPerSecond > 0;
        }
    }
}
