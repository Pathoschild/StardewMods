using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage
{
    /// <summary>A data layer which shows scarecrow coverage.</summary>
    internal class ScarecrowLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for tiles protected by a scarecrow.</summary>
        private readonly LegendEntry Covered;

        /// <summary>The legend entry for tiles not protected by a scarecrow.</summary>
        private readonly LegendEntry Exposed;

        /// <summary>The border color for the scarecrow under the cursor.</summary>
        private readonly Color SelectedColor;

        /// <summary>The maximum number of tiles from the center to search for scarecrows.</summary>
        private readonly int MaxSearchRadius = 20;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        /// <param name="colors">The colors to render.</param>
        public ScarecrowLayer(LayerConfig config, ColorScheme colors)
            : base(I18n.Scarecrows_Name(), config)
        {
            const string layerId = "ScarecrowCoverage";

            this.SelectedColor = colors.Get(layerId, "Selected", Color.Blue);
            this.Legend = new[]
            {
                this.Covered = new LegendEntry(I18n.Keys.Scarecrows_Protected, colors.Get(layerId, "Covered", Color.Green)),
                this.Exposed = new LegendEntry(I18n.Keys.Scarecrows_Exposed, colors.Get(layerId, "NotCovered", Color.Red))
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            // get scarecrows
            Vector2[] searchTiles = visibleArea.Expand(this.MaxSearchRadius).GetTiles().ToArray();
            SObject[] scarecrows =
                (
                    from Vector2 tile in searchTiles
                    where location.objects.ContainsKey(tile)
                    let scarecrow = location.objects[tile]
                    where scarecrow.IsScarecrow()
                    select scarecrow
                )
                .ToArray();

            // yield scarecrow coverage
            var covered = new HashSet<Vector2>();
            var groups = new List<TileGroup>();
            foreach (SObject scarecrow in scarecrows)
            {
                TileData[] tiles = this
                    .GetCoverage(scarecrow)
                    .Select(pos => new TileData(pos, this.Covered))
                    .ToArray();

                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);

                groups.Add(new TileGroup(tiles, outerBorderColor: scarecrow.TileLocation == cursorTile ? this.SelectedColor : this.Covered.Color));
            }

            // yield exposed crops
            var exposedCrops = this
                .GetExposedCrops(location, visibleTiles, covered)
                .Select(pos => new TileData(pos, this.Exposed));
            groups.Add(new TileGroup(exposedCrops, outerBorderColor: this.Exposed.Color));

            // yield scarecrow being placed
            SObject heldObj = Game1.player.ActiveObject;
            if (heldObj?.IsScarecrow() == true)
            {
                var tiles = this
                    .GetCoverage(heldObj, cursorTile)
                    .Select(pos => new TileData(pos, this.Covered, this.Covered.Color * 0.75f));
                groups.Add(new TileGroup(tiles, outerBorderColor: this.SelectedColor, shouldExport: false));
            }

            return groups.ToArray();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a map terrain feature is a crop.</summary>
        /// <param name="terrain">The map terrain feature.</param>
        private bool IsCrop(TerrainFeature terrain)
        {
            return terrain is HoeDirt { crop: not null };
        }

        /// <summary>Get a scarecrow tile radius.</summary>
        /// <param name="scarecrow">The scarecrow to check.</param>
        /// <param name="overrideOrigin">The tile position to check from, if different from <see cref="Object.TileLocation"/>.</param>
        /// <remarks>Derived from <see cref="Farm.addCrows"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(SObject scarecrow, Vector2? overrideOrigin = null)
        {
            Vector2 origin = overrideOrigin ?? scarecrow.TileLocation;
            int radius = scarecrow.GetRadiusForScarecrow();

            for (int x = (int)origin.X - radius; x <= origin.X + radius; x++)
            {
                for (int y = (int)origin.Y - radius; y <= origin.Y + radius; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    if (Vector2.Distance(tile, origin) < radius)
                        yield return tile;
                }
            }
        }

        /// <summary>Get tiles containing crops not protected by a scarecrow.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="coveredTiles">The tiles protected by a scarecrow.</param>
        private IEnumerable<Vector2> GetExposedCrops(GameLocation location, Vector2[] visibleTiles, HashSet<Vector2> coveredTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                if (coveredTiles.Contains(tile))
                    continue;

                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && this.IsCrop(terrain))
                    yield return tile;
            }
        }
    }
}
