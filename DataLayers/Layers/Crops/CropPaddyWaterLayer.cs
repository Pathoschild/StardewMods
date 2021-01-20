using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops
{
    /// <summary>A data layer which shows the water range for paddy crops.</summary>
    internal class CropPaddyWaterLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for tiles within range of water for paddy crops.</summary>
        private readonly LegendEntry InRange;

        /// <summary>The legend entry for tiles not within range of water for paddy crops.</summary>
        private readonly LegendEntry NotInRange;

        /// <summary>The previous location for which <see cref="TilesInRange"/> was cached.</summary>
        private GameLocation LastLocation;

        /// <summary>The cached tiles in range of open water for the current location.</summary>
        private readonly IDictionary<Vector2, bool> TilesInRange = new Dictionary<Vector2, bool>();

        /// <summary>A sample paddy crop.</summary>
        private readonly Lazy<Crop> PaddyCrop = new(() => new SamplePaddyCrop());


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        public CropPaddyWaterLayer(LayerConfig config)
            : base(I18n.CropPaddyWater_Name(), config)
        {
            this.Legend = new[]
            {
                this.InRange = new LegendEntry(I18n.Keys.CropPaddyWater_InRange, Color.Green),
                this.NotInRange = new LegendEntry(I18n.Keys.CropPaddyWater_NotInRange, Color.Red)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            // update cache on location change
            if (this.LastLocation == null || !object.ReferenceEquals(location, this.LastLocation))
            {
                this.LastLocation = location;
                this.TilesInRange.Clear();
            }

            // get paddy tiles
            HashSet<Vector2> tilesInRange = new HashSet<Vector2>(this.GetTilesInRange(location, visibleTiles));
            return new[]
            {
                new TileGroup(tilesInRange.Select(pos => new TileData(pos, this.InRange)), outerBorderColor: this.InRange.Color),
                new TileGroup(visibleTiles.Where(pos => !tilesInRange.Contains(pos)).Select(pos => new TileData(pos, this.NotInRange)))
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get tiles within range of open water for paddy crops.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <remarks>Derived from <see cref="HoeDirt.paddyWaterCheck"/>.</remarks>
        private IEnumerable<Vector2> GetTilesInRange(GameLocation location, Vector2[] visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                if (!this.TilesInRange.TryGetValue(tile, out bool inRange))
                    this.TilesInRange[tile] = inRange = this.RecalculateTileInRange(location, tile, this.PaddyCrop);

                if (inRange)
                    yield return tile;
            }
        }

        /// <summary>Get whether the tile is in range, without caching.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="samplePaddyCrop">A sample paddy crop.</param>
        private bool RecalculateTileInRange(GameLocation location, Vector2 tile, Lazy<Crop> samplePaddyCrop)
        {
            // water tile
            if (location.isWaterTile((int)tile.X, (int)tile.Y))
                return false;

            // dirt tile
            HoeDirt dirt = this.GetDirt(location, tile, ignorePot: true);
            if (dirt == null && this.IsTillable(location, tile) && location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
                dirt = new HoeDirt(HoeDirt.watered, samplePaddyCrop.Value);
            return dirt?.paddyWaterCheck(location, tile) ?? false;
        }

        /// <summary>A sample paddy crop.</summary>
        private class SamplePaddyCrop : Crop
        {
            /// <inheritdoc />
            public override bool isPaddyCrop()
            {
                return true;
            }
        }
    }
}
