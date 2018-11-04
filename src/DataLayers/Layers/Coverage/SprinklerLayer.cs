using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage
{
    /// <summary>A data layer which shows sprinkler coverage.</summary>
    internal class SprinklerLayer : BaseLayer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for sprinkled tiles.</summary>
        private readonly Color WetColor = Color.Green;

        /// <summary>The color for unsprinkled tiles.</summary>
        private readonly Color DryColor = Color.Red;

        /// <summary>The border color for the sprinkler under the cursor.</summary>
        private readonly Color SelectedColor = Color.Blue;

        /// <summary>The maximum number of tiles from the center a sprinkler can protect.</summary>
        private readonly int MaxRadius;

        /// <summary>The static sprinkler tiles centered on (0, 0), indexed by sprinkler ID.</summary>
        private readonly IDictionary<int, Vector2[]> StaticTilesBySprinklerID;

        /// <summary>Handles access to the supported mod integrations.</summary>
        private readonly ModIntegrations Mods;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        public SprinklerLayer(ITranslationHelper translations, LayerConfig config, ModIntegrations mods)
            : base(translations.Get("sprinklers.name"), config)
        {
            // init
            this.Mods = mods;
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("sprinklers.covered"), this.WetColor),
                new LegendEntry(translations.Get("sprinklers.dry-crops"), this.DryColor)
            };

            // get static sprinkler coverage
            this.StaticTilesBySprinklerID = this.GetStaticSprinklerTiles(mods);

            // get max sprinkler radius
            this.MaxRadius = this.StaticTilesBySprinklerID.Max(p => this.GetMaxRadius(p.Value));
            if (mods.BetterSprinklers.IsLoaded)
                this.MaxRadius = Math.Max(this.MaxRadius, mods.BetterSprinklers.MaxRadius);
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
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
            IDictionary<int, Vector2[]> coverageBySprinklerID = this.GetCurrentSprinklerTiles(this.StaticTilesBySprinklerID);

            // yield sprinkler coverage
            HashSet<Vector2> covered = new HashSet<Vector2>();
            foreach (Object sprinkler in sprinklers)
            {
                TileData[] tiles = this.GetCoverage(sprinkler, sprinkler.TileLocation, coverageBySprinklerID).Select(pos => new TileData(pos, this.WetColor)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorderColor: sprinkler.TileLocation == cursorTile ? this.SelectedColor : this.WetColor);
            }

            // yield dry crops
            TileData[] dryCrops = this.GetDryCrops(location, visibleTiles, covered).Select(pos => new TileData(pos, this.DryColor)).ToArray();
            yield return new TileGroup(dryCrops, outerBorderColor: this.DryColor);

            // yield sprinkler being placed
            Object heldObj = Game1.player.ActiveObject;
            if (this.IsSprinkler(heldObj))
            {
                TileData[] tiles = this.GetCoverage(heldObj, cursorTile, coverageBySprinklerID).Select(pos => new TileData(pos, this.WetColor * 0.75f)).ToArray();
                yield return new TileGroup(tiles, outerBorderColor: this.SelectedColor);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a map object is a sprinkler.</summary>
        /// <param name="obj">The map object.</param>
        private bool IsSprinkler(Object obj)
        {
            return obj != null && this.StaticTilesBySprinklerID.ContainsKey(obj.ParentSheetIndex);
        }

        /// <summary>Get whether a map terrain feature is a crop.</summary>
        /// <param name="terrain">The map terrain feature.</param>
        private bool IsCrop(TerrainFeature terrain)
        {
            return terrain is HoeDirt dirt && dirt.crop != null;
        }

        /// <summary>Get the relative sprinkler tile coverage, including any mod customisations which don't change after launch.</summary>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        private IDictionary<int, Vector2[]> GetStaticSprinklerTiles(ModIntegrations mods)
        {
            IDictionary<int, Vector2[]> tiles = new Dictionary<int, Vector2[]>();

            // vanilla coverage
            {
                Vector2 center = Vector2.Zero;

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
            }

            // Cobalt sprinkler
            if (mods.Cobalt.IsLoaded)
                tiles[mods.Cobalt.GetSprinklerId()] = mods.Cobalt.GetSprinklerTiles().ToArray();

            // Prismatic Sprinkler
            if (mods.PrismaticTools.IsLoaded)
                tiles[mods.PrismaticTools.GetSprinklerID()] = mods.PrismaticTools.GetSprinklerCoverage().ToArray();

            // Simple Sprinkler mod adds tiles to default coverage
            if (mods.SimpleSprinkler.IsLoaded)
            {
                foreach (var pair in mods.SimpleSprinkler.GetNewSprinklerTiles())
                {
                    int sprinklerID = pair.Key;
                    if (tiles.TryGetValue(sprinklerID, out Vector2[] currentTiles))
                        tiles[sprinklerID] = currentTiles.Union(pair.Value).ToArray();
                    else
                        tiles[sprinklerID] = pair.Value;
                }
            }

            return tiles;
        }

        /// <summary>Get the current relative sprinkler coverage, including any dynamic mod changes.</summary>
        /// <param name="staticTiles">The static sprinkler coverage.</param>
        private IDictionary<int, Vector2[]> GetCurrentSprinklerTiles(IDictionary<int, Vector2[]> staticTiles)
        {
            // get static tiles
            if (!this.Mods.BetterSprinklers.IsLoaded)
                return staticTiles;

            // merge custom tiles
            IDictionary<int, Vector2[]> tilesBySprinklerID = new Dictionary<int, Vector2[]>(staticTiles);
            if (this.Mods.BetterSprinklers.IsLoaded)
            {
                foreach (var pair in this.Mods.BetterSprinklers.GetSprinklerTiles())
                    tilesBySprinklerID[pair.Key] = pair.Value;
            }
            return tilesBySprinklerID;
        }

        /// <summary>Get the maximum distance from the center for a sprinkler coverage.</summary>
        /// <param name="tiles">The covered tiles.</param>
        public int GetMaxRadius(IEnumerable<Vector2> tiles)
        {
            int maxRadius = 0;
            foreach (Vector2 tile in tiles)
                maxRadius = (int)Math.Max(Math.Max(maxRadius, Math.Abs(tile.X)), Math.Abs(tile.Y));
            return maxRadius;
        }

        /// <summary>Get a sprinkler tile radius.</summary>
        /// <param name="sprinkler">The sprinkler whose radius to get.</param>
        /// <param name="origin">The sprinkler's tile.</param>
        /// <param name="radii">The sprinkler radii centered on (0, 0) indexed by sprinkler ID.</param>
        /// <remarks>Derived from <see cref="Object.DayUpdate"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(Object sprinkler, Vector2 origin, IDictionary<int, Vector2[]> radii)
        {
            // get relative tiles
            if (!radii.TryGetValue(sprinkler.ParentSheetIndex, out Vector2[] tiles))
                throw new NotSupportedException($"Unknown sprinkler ID {sprinkler.ParentSheetIndex}.");

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
