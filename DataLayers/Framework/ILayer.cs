using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Input;
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

        /// <summary>The keys which activate the layer.</summary>
        KeyBinding ShortcutKey { get; }

        /// <summary>Whether to always show the tile grid.</summary>
        bool AlwaysShowGrid { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile);
    }
}
