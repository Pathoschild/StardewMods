using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Input;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>The base implementation for a data layer.</summary>
    internal abstract class BaseLayer : ILayer
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The layer's display name.</summary>
        public string Name { get; }

        /// <summary>The number of ticks between each update.</summary>
        public int UpdateTickRate { get; }

        /// <summary>Whether to update the layer when the set of visible tiles changes.</summary>
        public bool UpdateWhenVisibleTilesChange { get; }

        /// <summary>The keys which activate the layer.</summary>
        public KeyBinding ShortcutKey { get; }

        /// <summary>The legend entries to display.</summary>
        public LegendEntry[] Legend { get; protected set; }

        /// <summary>Whether to always show the tile grid.</summary>
        public bool AlwaysShowGrid { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public abstract TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The data layer name.</param>
        /// <param name="config">The data layers settings.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        protected BaseLayer(string name, LayerConfig config, IInputHelper input, IMonitor monitor)
        {
            this.Name = name;
            this.UpdateTickRate = (int)(60 / config.UpdatesPerSecond);
            this.UpdateWhenVisibleTilesChange = config.UpdateWhenViewChange;
            this.ShortcutKey = CommonHelper.ParseButtons(config.ShortcutKey, input, monitor, this.Name);
        }

        /// <summary>Get the dirt instance for a tile, if any.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        protected HoeDirt GetDirt(GameLocation location, Vector2 tile)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && terrain is HoeDirt dirt)
                return dirt;
            if (location.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot pot)
                return pot.hoeDirt.Value;

            return null;
        }
    }
}
