using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.DataMaps.DataMaps.Coverage
{
    /// <summary>A data map which shows bee house coverage.</summary>
    internal class BeeHouseMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for tiles covered by a bee house.</summary>
        private readonly Color CoveredColor = Color.Green;

        /// <summary>The border color for the bee house under the cursor.</summary>
        private readonly Color SelectedColor = Color.Blue;

        /// <summary>The maximum number of tiles from the center a bee house can cover.</summary>
        private readonly int MaxRadius = 5;

        /// <summary>The relative tile coordinates covered by a bee house.</summary>
        private readonly Vector2[] RelativeRange = BeeHouseMap.GetRelativeCoverage().ToArray();


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
        public BeeHouseMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.bee-houses.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.bee-houses.range"), this.CoveredColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            // get bee houses
            Vector2[] searchTiles = visibleArea.Expand(this.MaxRadius).GetTiles().ToArray();
            Object[] beeHouses =
                (
                    from Vector2 tile in searchTiles
                    where location.objects.ContainsKey(tile)
                    let beeHouse = location.objects[tile]
                    where this.IsBeeHouse(beeHouse)
                    select beeHouse
                )
                .ToArray();

            // yield coverage
            HashSet<Vector2> covered = new HashSet<Vector2>();
            foreach (Object beeHouse in beeHouses)
            {
                TileData[] tiles = this.GetCoverage(location, beeHouse.TileLocation).Select(pos => new TileData(pos, this.CoveredColor)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorderColor: beeHouse.TileLocation == cursorTile ? this.SelectedColor : this.CoveredColor);
            }

            // yield bee house being placed
            Object heldObj = Game1.player.ActiveObject;
            if (this.IsBeeHouse(heldObj))
            {
                TileData[] tiles = this.GetCoverage(location, cursorTile).Select(pos => new TileData(pos, this.CoveredColor * 0.75f)).ToArray();
                yield return new TileGroup(tiles, outerBorderColor: this.SelectedColor);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a map object is a bee house.</summary>
        /// <param name="obj">The map object.</param>
        private bool IsBeeHouse(Object obj)
        {
            return obj != null && obj.bigCraftable && obj.Name == "Bee House";
        }

        /// <summary>Get a bee house tile radius.</summary>
        /// <param name="location">The bee house's location.</param>
        /// <param name="origin">The bee house's tile.</param>
        /// <remarks>Derived from <see cref="Object.checkForAction"/> and <see cref="Utility.findCloseFlower"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(GameLocation location, Vector2 origin)
        {
            if (!(location is Farm))
                yield break; // bee houses are hardcoded to only work on the farm

            foreach (Vector2 relativeTile in this.RelativeRange)
                yield return origin + relativeTile;
        }

        /// <summary>Get the relative tiles covered by a bee house.</summary>
        /// <remarks>Derived from <see cref="Utility.findCloseFlower"/>.</remarks>
        private static IEnumerable<Vector2> GetRelativeCoverage()
        {
            Queue<Vector2> queue = new Queue<Vector2>();
            HashSet<Vector2> visited = new HashSet<Vector2>();
            queue.Enqueue(Vector2.Zero);
            for (int i = 0; i <= 150 && queue.Count > 0; ++i)
            {
                Vector2 tile = queue.Dequeue();
                yield return tile;
                foreach (Vector2 adjacentTile in Utility.getAdjacentTileLocations(tile))
                {
                    if (!visited.Contains(adjacentTile))
                        queue.Enqueue(adjacentTile);
                }
                visited.Add(tile);
            }
        }
    }
}
