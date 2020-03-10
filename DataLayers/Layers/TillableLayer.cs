using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>A data layer which shows whether tiles are tillable.</summary>
    internal class TillableLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for tiles already tilled but not planted.</summary>
        private readonly LegendEntry Tilled;

        /// <summary>The legend entry for tillable tiles.</summary>
        private readonly LegendEntry Tillable;

        /// <summary>The legend entry for tillable but occupied tiles.</summary>
        private readonly LegendEntry Occupied;

        /// <summary>The legend entry for non-tillable tiles.</summary>
        private readonly LegendEntry NonTillable;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        public TillableLayer(ITranslationHelper translations, LayerConfig config, IInputHelper input, IMonitor monitor)
            : base(translations.Get("tillable.name"), config, input, monitor)
        {
            this.Legend = new[]
            {
                this.Tilled = new LegendEntry(translations, "tillable.tilled", Color.DarkMagenta),
                this.Tillable = new LegendEntry(translations, "tillable.tillable", Color.Green),
                this.Occupied = new LegendEntry(translations, "tillable.occupied", Color.Orange),
                this.NonTillable = new LegendEntry(translations, "tillable.not-tillable", Color.Red)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            var tiles = this.GetTiles(location, visibleArea.GetTiles());
            return new[]
            {
                new TileGroup(tiles[this.Tilled]),
                new TileGroup(tiles[this.Tillable], outerBorderColor: this.Tillable.Color),
                new TileGroup(tiles[this.Occupied]),
                new TileGroup(tiles[this.NonTillable])
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IDictionary<LegendEntry, List<TileData>> GetTiles(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            IDictionary<LegendEntry, List<TileData>> tiles = new[] { this.Tillable, this.Tilled, this.Occupied, this.NonTillable }.ToDictionary(p => p, p => new List<TileData>());

            foreach (Vector2 tile in visibleTiles)
            {
                LegendEntry type;
                if (!this.IsTillable(location, tile))
                    type = this.NonTillable;
                else if (this.IsOccupied(location, tile))
                    type = this.Occupied;
                else if (this.GetDirt(location, tile) != null && !location.isCropAtTile((int)tile.X, (int)tile.Y))
                    type = this.Tilled;
                else
                    type = this.Tillable;

                tiles[type].Add(new TileData(tile, type));
            }

            return tiles;
        }


        /// <summary>Get whether a tile is tillable.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="StardewValley.Tools.Hoe.DoFunction"/>.</remarks>
        private bool IsTillable(GameLocation location, Vector2 tile)
        {
            return location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
        }

        /// <summary>Get whether a tile is blocked due to something it contains.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="StardewValley.Tools.Hoe.DoFunction"/>.</remarks>
        private bool IsOccupied(GameLocation location, Vector2 tile)
        {
            // impassable tiles (e.g. water)
            if (!location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
                return true;

            // objects & large terrain features
            if (location.objects.ContainsKey(tile) || location.largeTerrainFeatures.Any(p => p.tilePosition.Value == tile))
                return true;

            // non-dirt terrain features
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
            {
                HoeDirt dirt = feature as HoeDirt;
                if (dirt == null || dirt.crop != null)
                    return true;
            }

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                if (buildableLocation.buildings.Any(building => building.occupiesTile(tile)))
                    return true;
            }

            return false;
        }
    }
}
