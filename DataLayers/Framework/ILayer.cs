using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>Provides metadata to display in the overlay.</summary>
    internal interface ILayer
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The layer display name.</summary>
        string Name { get; }

        /// <summary>The number of ticks between each update.</summary>
        int UpdateTickRate { get; }

        /// <summary>Whether to update the layer when the set of visible tiles changes.</summary>
        bool UpdateWhenVisibleTilesChange { get; }

        /// <summary>The legend entries to display.</summary>
        LegendEntry[] Legend { get; }

        /// <summary>The buttons to activate the layer.</summary>
        SButton[] LayerButtons { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile);

        /// <summary>Parses the shortcuts from the config.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        void ParseShortcuts(IMonitor monitor);
    }
}
