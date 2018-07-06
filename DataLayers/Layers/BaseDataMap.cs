using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>The base implementation for a data map.</summary>
    internal abstract class BaseDataMap : IDataMap
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }

        /// <summary>The number of ticks between each update.</summary>
        public int UpdateTickRate { get; }

        /// <summary>Whether to update the map when the set of visible tiles changes.</summary>
        public bool UpdateWhenVisibleTilesChange { get; }

        /// <summary>The legend entries to display.</summary>
        public LegendEntry[] Legend { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public abstract IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The data map name.</param>
        /// <param name="config">The data map settings.</param>
        protected BaseDataMap(string name, MapConfig config)
        {
            this.Name = name;
            this.UpdateTickRate = (int)(60 / config.UpdatesPerSecond);
            this.UpdateWhenVisibleTilesChange = config.UpdateWhenViewChange;
        }
    }
}
