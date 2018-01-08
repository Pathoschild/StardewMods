using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using Pathoschild.Stardew.DataMaps.Framework.Integrations;
using Pathoschild.Stardew.DataMaps.Framework.Integrations.BetterSprinklers;
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

        /// <summary>The default sprinkler tiles centered on (0, 0), indexed by sprinkler ID.</summary>
        private IDictionary<int, Vector2[]> DefaultSprinklerTiles;

        /// <summary>Handles the logic for integrating with the Better Sprinklers mod.</summary>
        private readonly BetterSprinklersIntegration BetterSprinklers;


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
        /// <param name="betterSprinklers">Handles the logic for integrating with the Better Sprinklers mod.</param>
        public SprinklerMap(ITranslationHelper translations, BetterSprinklersIntegration betterSprinklers)
        {
            this.BetterSprinklers = betterSprinklers;
            this.DefaultSprinklerTiles = this.GetDefaultSprinklerTiles();
            if (betterSprinklers.IsLoaded)
                this.MaxRadius = Math.Max(this.MaxRadius, betterSprinklers.MaxRadius);

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

            // get radius
            var radii = this.BetterSprinklers.IsLoaded
                ? this.BetterSprinklers.GetSprinklerTiles()
                : this.DefaultSprinklerTiles;

            // yield sprinkler coverage
            HashSet<Vector2> covered = new HashSet<Vector2>();
            foreach (Object sprinkler in sprinklers)
            {
                TileData[] tiles = this.GetCoverage(sprinkler, sprinkler.TileLocation, radii).Select(pos => new TileData(pos, this.WetColor)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorderColor: this.WetColor);
            }

            // yield dry crops
            TileData[] dryCrops = this.GetDryCrops(location, visibleTiles, covered).Select(pos => new TileData(pos, this.DryColor)).ToArray();
            yield return new TileGroup(dryCrops, outerBorderColor: this.DryColor);

            // yield sprinkler being placed
            Object heldObj = Game1.player.ActiveObject;
            if (this.IsSprinkler(heldObj))
            {
                Vector2 cursorTile = TileHelper.GetTileFromCursor();
                TileData[] tiles = this.GetCoverage(heldObj, cursorTile, radii).Select(pos => new TileData(pos, this.WetColor * 0.75f)).ToArray();
                yield return new TileGroup(tiles, outerBorderColor: this.WetColor);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a map object is a sprinkler.</summary>
        /// <param name="obj">The map object.</param>
        private bool IsSprinkler(Object obj)
        {
            if (obj == null)
                return false;

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

        /// <summary>Get the default sprinkler tiles centered on (0, 0).</summary>
        private IDictionary<int, Vector2[]> GetDefaultSprinklerTiles()
        {
            Vector2 center = Vector2.Zero;
            IDictionary<int, Vector2[]> tiles = new Dictionary<int, Vector2[]>();

            // basic sprinkler
            tiles[599] = Utility.getAdjacentTileLocationsArray(center).Concat(new[] { center }).ToArray();

            // quality sprinkler
            tiles[621] = Utility.getSurroundingTileLocationsArray(center).Concat(new[] { center }).ToArray();

            // iridium sprinkler
            List<Vector2> iridiumTiles = new List<Vector2>(4 * 4);
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                    iridiumTiles.Add(new Vector2(x, y));
            }
            tiles[645] = iridiumTiles.ToArray();

            return tiles;
        }

        /// <summary>Get a sprinkler tile radius.</summary>
        /// <param name="sprinkler">The sprinkler whose radius to get.</param>
        /// <param name="origin">The sprinkler's tile.</param>
        /// <param name="radii">The sprinkler radii centered on (0, 0) indexed by sprinkler ID.</param>
        /// <remarks>Derived from <see cref="Object.DayUpdate"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(Object sprinkler, Vector2 origin, IDictionary<int, Vector2[]> radii)
        {
            // get relative tiles
            if (!radii.TryGetValue(sprinkler.parentSheetIndex, out Vector2[] tiles))
                throw new NotSupportedException($"Unknown sprinkler ID {sprinkler.parentSheetIndex}.");

            // get tiles
            foreach (Vector2 tile in tiles)
                yield return origin + tile;
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
