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
        public IEnumerable<TileGroup> Update(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            visibleTiles = visibleTiles.ToArray();

            // get sprinklers
            Object[] sprinklers =
                (
                    from Vector2 tile in visibleTiles
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
                TileData[] tiles = this.GetCoverage(sprinkler).Select(pos => new TileData(pos, Color.Green)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorders: true);
            }

            // yield dry crops
            TileData[] dryCrops = this.GetDryCrops(location, visibleTiles.ToArray(), covered).Select(pos => new TileData(pos, Color.Red)).ToArray();
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
                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
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

                if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && terrain is HoeDirt)
                    yield return tile;
            }
        }
    }
}
