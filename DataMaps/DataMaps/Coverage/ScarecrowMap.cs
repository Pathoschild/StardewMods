using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.DataMaps.DataMaps.Coverage
{
    /// <summary>A data map which shows scarecrow coverage.</summary>
    internal class ScarecrowMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for tiles protected by a scarecrow.</summary>
        private readonly Color CoveredColor = Color.Green;

        /// <summary>The color for tiles not protected by a scarecrow.</summary>
        private readonly Color ExposedColor = Color.Red;

        /// <summary>The border color for the scarecrow under the cursor.</summary>
        private readonly Color SelectedColor = Color.Blue;

        /// <summary>The maximum number of tiles from the center a scarecrow can protect.</summary>
        private readonly int MaxRadius = 8;


        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }

        /// <summary>The legend entries to display.</summary>
        public LegendEntry[] Legend { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        public ScarecrowMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.scarecrows.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.scarecrows.protected"), this.CoveredColor),
                new LegendEntry(translations.Get("maps.scarecrows.exposed"), this.ExposedColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            // get scarecrows
            Vector2[] searchTiles = visibleArea.Expand(this.MaxRadius).GetTiles().ToArray();
            Object[] scarecrows =
                (
                    from Vector2 tile in searchTiles
                    where location.objects.ContainsKey(tile)
                    let scarecrow = location.objects[tile]
                    where this.IsScarecrow(scarecrow)
                    select scarecrow
                )
                .ToArray();

            // yield scarecrow coverage
            HashSet<Vector2> covered = new HashSet<Vector2>();
            foreach (Object scarecrow in scarecrows)
            {
                TileData[] tiles = this.GetCoverage(scarecrow.TileLocation).Select(pos => new TileData(pos, this.CoveredColor)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorderColor: scarecrow.TileLocation == cursorTile ? this.SelectedColor : this.CoveredColor);
            }

            // yield exposed crops
            TileData[] exposedCrops = this.GetExposedCrops(location, visibleTiles, covered).Select(pos => new TileData(pos, this.ExposedColor)).ToArray();
            yield return new TileGroup(exposedCrops, outerBorderColor: this.ExposedColor);

            // yield scarecrow being placed
            Object heldObj = Game1.player.ActiveObject;
            if (this.IsScarecrow(heldObj))
            {
                TileData[] tiles = this.GetCoverage(cursorTile).Select(pos => new TileData(pos, this.CoveredColor * 0.75f)).ToArray();
                yield return new TileGroup(tiles, outerBorderColor: this.SelectedColor);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a map object is a scarecrow.</summary>
        /// <param name="obj">The map object.</param>
        /// <remarks>Derived from <see cref="Farm.addCrows"/>.</remarks>
        private bool IsScarecrow(Object obj)
        {
            return obj != null && obj.bigCraftable && obj.Name.Contains("arecrow");
        }

        /// <summary>Get whether a map terrain feature is a crop.</summary>
        /// <param name="terrain">The map terrain feature.</param>
        private bool IsCrop(TerrainFeature terrain)
        {
            return terrain is HoeDirt dirt && dirt.crop != null;
        }

        /// <summary>Get a scarecrow tile radius.</summary>
        /// <param name="origin">The scarecrow's tile.</param>
        /// <remarks>Derived from <see cref="Farm.addCrows"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(Vector2 origin)
        {
            for (int x = (int)origin.X - this.MaxRadius; x <= origin.X + this.MaxRadius; x++)
            {
                for (int y = (int)origin.Y - this.MaxRadius; y <= origin.Y + this.MaxRadius; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    if (Vector2.Distance(tile, origin) < this.MaxRadius + 1)
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
