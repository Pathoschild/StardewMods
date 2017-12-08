using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows sprinkler coverage.</summary>
    internal class SprinklerMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for sprinkled tiles.</summary>
        private readonly Color WetColor = Color.Green;

        /// <summary>The color for unsprinkled tiles.</summary>
        private readonly Color DryColor = Color.Red;

        /// <summary>The maximum number of tiles from the center a sprinkler can protect.</summary>
        private readonly int MaxRadius = 2;


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
        public SprinklerMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.sprinklers.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.sprinklers.covered"), this.WetColor),
                new LegendEntry(translations.Get("maps.sprinklers.dry-crops"), this.DryColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea)
        {
            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            // get sprinklers
            Vector2[] searchTiles = visibleArea.Expand(this.MaxRadius).GetTiles().ToArray();
            Object[] sprinklers =
                (
                    from Vector2 tile in searchTiles
                    where location.objects.ContainsKey(tile)
                    let sprinkler = location.objects[tile]
                    where this.IsSprinkler(sprinkler)
                    select sprinkler
                )
                .ToArray();

            // yield sprinkler coverage
            HashSet<Vector2> covered = new HashSet<Vector2>();
            foreach (Object sprinkler in sprinklers)
            {
                TileData[] tiles = this.GetCoverage(sprinkler).Select(pos => new TileData(pos, this.WetColor)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorders: true);
            }

            // yield dry crops
            TileData[] dryCrops = this.GetDryCrops(location, visibleTiles, covered).Select(pos => new TileData(pos, this.DryColor)).ToArray();
            yield return new TileGroup(dryCrops, outerBorders: true);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a map object is a sprinkler.</summary>
        /// <param name="obj">The map object.</param>
        private bool IsSprinkler(Object obj)
        {
            return
                obj.parentSheetIndex == 599 // basic sprinkler
                || obj.parentSheetIndex == 621 // quality
                || obj.parentSheetIndex == 645; // iridium
        }

        /// <summary>Get whether a map terrain feature is a crop.</summary>
        /// <param name="terrain">The map terrain feature.</param>
        private bool IsCrop(TerrainFeature terrain)
        {
            return terrain is HoeDirt dirt && dirt.crop != null;
        }

        /// <summary>Get a sprinkler tile radius.</summary>
        /// <param name="sprinkler">The sprinkler whose radius to get.</param>
        /// <remarks>Derived from <see cref="Object.DayUpdate"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(Object sprinkler)
        {
            Vector2 origin = sprinkler.TileLocation;
            switch (sprinkler.parentSheetIndex)
            {
                // basic sprinkler
                case 599:
                    yield return origin;
                    foreach (Vector2 tile in Utility.getAdjacentTileLocationsArray(origin))
                        yield return tile;
                    break;

                // quality sprinkler
                case 621:
                    yield return origin;
                    foreach (Vector2 tile in Utility.getSurroundingTileLocationsArray(origin))
                        yield return tile;
                    break;

                // iridium sprinkler
                case 645:
                    for (int x = -this.MaxRadius; x <= this.MaxRadius; x++)
                    {
                        for (int y = -this.MaxRadius; y <= this.MaxRadius; y++)
                            yield return new Vector2(origin.X + x, origin.Y + y);
                    }
                    break;

                default:
                    throw new NotSupportedException($"Unknown sprinkler ID {sprinkler.parentSheetIndex}");
            }
        }

        /// <summary>Get tiles containing crops not covered by a sprinkler.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="coveredTiles">The tiles covered by a sprinkler.</param>
        private IEnumerable<Vector2> GetDryCrops(GameLocation location, Vector2[] visibleTiles, HashSet<Vector2> coveredTiles)
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
