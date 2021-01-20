using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI.Utilities;
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
        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public int UpdateTickRate { get; }

        /// <inheritdoc />
        public bool UpdateWhenVisibleTilesChange { get; }

        /// <inheritdoc />
        public KeybindList ShortcutKey { get; }

        /// <inheritdoc />
        public LegendEntry[] Legend { get; protected set; }

        /// <inheritdoc />
        public bool AlwaysShowGrid { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public abstract TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The data layer name.</param>
        /// <param name="config">The data layers settings.</param>
        protected BaseLayer(string name, LayerConfig config)
        {
            this.Id = this.GetType().FullName;
            this.Name = name;
            this.UpdateTickRate = (int)(60 / config.UpdatesPerSecond);
            this.UpdateWhenVisibleTilesChange = config.UpdateWhenViewChange;
            this.ShortcutKey = config.ShortcutKey;
        }

        /// <summary>Get the dirt instance for a tile, if any.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="ignorePot">Whether to ignore dirt in indoor pots.</param>
        protected HoeDirt GetDirt(GameLocation location, Vector2 tile, bool ignorePot = false)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && terrain is HoeDirt dirt)
                return dirt;
            if (!ignorePot && location.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot pot)
                return pot.hoeDirt.Value;

            return null;
        }

        /// <summary>Get whether a tile is tillable.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="StardewValley.Tools.Hoe.DoFunction"/>.</remarks>
        protected bool IsTillable(GameLocation location, Vector2 tile)
        {
            return location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
        }

        /// <summary>Get whether dirt contains a dead crop.</summary>
        /// <param name="dirt">The dirt to check.</param>
        protected bool IsDeadCrop(HoeDirt dirt)
        {
            return dirt?.crop?.dead.Value == true;
        }
    }
}
