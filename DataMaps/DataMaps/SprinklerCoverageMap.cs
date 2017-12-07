using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows sprinkler coverage.</summary>
    internal class SprinklerCoverageMap : IDataMap
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public SprinklerCoverageMap()
        {
            this.Name = "Sprinkler Coverage";
        }

        /// <summary>Get the legend entries to display.</summary>
        public IEnumerable<LegendEntry> GetLegendEntries()
        {
            return new[]
            {
                new LegendEntry("Coverage", Color.Green),
                new LegendEntry("Dry Crop", Color.Red)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        public IEnumerable<TileData> Update(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            visibleTiles = visibleTiles.ToArray();

            // get covered tiles
            Vector2[] covered =
                (
                    from Vector2 tile in visibleTiles

                        // get sprinkler
                    where location.objects.ContainsKey(tile)
                    let sprinkler = location.objects[tile]
                    where this.IsSprinkler(sprinkler)

                    // get covered tiles
                    from Vector2 coveredTile in this.GetCoverage(sprinkler, tile)
                    select coveredTile
                )
                .Distinct()
                .Intersect(visibleTiles)
                .ToArray();

            // get dry crops
            Vector2[] dryCrops =
                (
                    from Vector2 tile in visibleTiles.Except(covered)

                        // get crop
                    where location.terrainFeatures.ContainsKey(tile)
                    let crop = location.terrainFeatures[tile]
                    where this.IsCrop(crop)

                    select tile
                )
                .ToArray();

            // yield
            foreach (Vector2 tile in covered)
                yield return new TileData(tile, Color.Green);
            foreach (Vector2 tile in dryCrops)
                yield return new TileData(tile, Color.Red);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a terrain feature is a crop.</summary>
        /// <param name="obj">The terrain feature.</param>
        private bool IsCrop(TerrainFeature obj)
        {
            return obj is HoeDirt;
        }

        /// <summary>Get whether a map object is a sprinkler.</summary>
        /// <param name="obj">The map object.</param>
        private bool IsSprinkler(Object obj)
        {
            return
                obj.parentSheetIndex == 599 // basic sprinkler
                || obj.parentSheetIndex == 621 // quality
                || obj.parentSheetIndex == 645; // iridium
        }

        /// <summary>Get a sprinkler tile radius.</summary>
        /// <param name="sprinkler">The sprinkler whose radius to get.</param>
        /// <param name="origin">The sprinkler's tile position.</param>
        /// <remarks>Derived from <see cref="Object.DayUpdate"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(Object sprinkler, Vector2 origin)
        {
            switch (sprinkler.parentSheetIndex)
            {
                // basic sprinkler
                case 599:
                    foreach (Vector2 tile in Utility.getAdjacentTileLocationsArray(origin))
                        yield return tile;
                    break;

                // quality sprinkler
                case 621:
                    foreach (Vector2 tile in Utility.getSurroundingTileLocationsArray(origin))
                        yield return tile;
                    break;

                // iridium sprinkler
                case 645:
                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            if (x != 0 || y != 0)
                                yield return new Vector2(origin.X + x, origin.Y + y);
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException($"Unknown sprinkler ID {sprinkler.parentSheetIndex}");
            }
        }
    }
}
