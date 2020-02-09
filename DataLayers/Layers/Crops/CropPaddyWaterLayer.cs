using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops
{
    /// <summary>A data layer which shows the water range for paddy crops.</summary>
    internal class CropPaddyWaterLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The number of land tiles </summary>
        private const int PaddyWaterRange = 3;

        /// <summary>The legend entry for tiles within range of water for paddy crops.</summary>
        private readonly LegendEntry InRange;

        /// <summary>The legend entry for tiles not within range of water for paddy crops.</summary>
        private readonly LegendEntry NotInRange;

        /// <summary>The previous location for which the <see cref="WaterTiles"/> were cached.</summary>
        private GameLocation LastLocation;

        /// <summary>The cached water tiles in the current location.</summary>
        private readonly HashSet<Vector2> WaterTiles = new HashSet<Vector2>();

        /// <summary>The cached tiles in range of open water for the current location.</summary>
        private readonly IDictionary<Vector2, bool> TilesInRange = new Dictionary<Vector2, bool>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        public CropPaddyWaterLayer(ITranslationHelper translations, LayerConfig config, IInputHelper input, IMonitor monitor)
            : base(translations.Get("crop-paddy-water.name"), config, input, monitor)
        {
            this.Legend = new[]
            {
                this.InRange = new LegendEntry(translations, "crop-paddy-water.in-range", Color.Green),
                this.NotInRange = new LegendEntry(translations, "crop-paddy-water.not-in-range", Color.Red)
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
                this.WaterTiles.Clear();
                this.TilesInRange.Clear();

                int mapWidth = location.Map.Layers.Max(p => p.LayerWidth);
                int mapHeight = location.Map.Layers.Max(p => p.LayerHeight);
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        if (location.isOpenWater(x, y))
                            this.WaterTiles.Add(new Vector2(x, y));
                    }
                }
            }

            // get paddy tiles
            HashSet<Vector2> tilesInRange = new HashSet<Vector2>(this.GetTilesInRange(visibleTiles));
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
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <remarks>Derived from <see cref="HoeDirt.paddyWaterCheck"/>.</remarks>
        private IEnumerable<Vector2> GetTilesInRange(Vector2[] visibleTiles)
        {
            if (!this.WaterTiles.Any())
                yield break;

            foreach (Vector2 tile in visibleTiles)
            {
                if (!this.TilesInRange.TryGetValue(tile, out bool inRange))
                    this.TilesInRange[tile] = inRange = this.WaterTiles.Any(water => Math.Abs(tile.X - water.X) <= PaddyWaterRange && Math.Abs(tile.Y - water.Y) <= PaddyWaterRange);

                if (inRange)
                    yield return tile;
            }
        }
    }
}
