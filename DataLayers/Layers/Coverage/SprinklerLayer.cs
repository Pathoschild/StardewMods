using System;
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
    /// <summary>A data layer which shows sprinkler coverage.</summary>
    internal class SprinklerLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for sprinkled tiles.</summary>
        private readonly LegendEntry Wet;

        /// <summary>The legend entry for unsprinkled tiles.</summary>
        private readonly LegendEntry Dry;

        /// <summary>The border color for the sprinkler under the cursor.</summary>
        private readonly Color SelectedColor = Color.Blue;

        /// <summary>The maximum number of tiles outside the visible screen area to search for sprinklers.</summary>
        private readonly int SearchRadius;

        /// <summary>Handles access to the supported mod integrations.</summary>
        private readonly ModIntegrations Mods;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        public SprinklerLayer(LayerConfig config, ModIntegrations mods)
            : base(I18n.Sprinklers_Name(), config)
        {
            // init
            this.Mods = mods;
            this.Legend = new[]
            {
                this.Wet = new LegendEntry(I18n.Keys.Sprinklers_Covered, Color.Green),
                this.Dry = new LegendEntry(I18n.Keys.Sprinklers_DryCrops, Color.Red)
            };

            // get search radius
            this.SearchRadius = 10;
            if (mods.BetterSprinklers.IsLoaded)
                this.SearchRadius = Math.Max(this.SearchRadius, mods.BetterSprinklers.MaxRadius);
            if (mods.LineSprinklers.IsLoaded)
                this.SearchRadius = Math.Max(this.SearchRadius, mods.LineSprinklers.MaxRadius);
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            // get coverage
            IDictionary<int, Vector2[]> coverageBySprinklerID = this.GetCustomSprinklerTiles();

            // get sprinklers
            Vector2[] searchTiles = visibleArea.Expand(this.SearchRadius).GetTiles().ToArray();
            SObject[] sprinklers =
                (
                    from Vector2 tile in searchTiles
                    where location.objects.ContainsKey(tile)
                    let sprinkler = location.objects[tile]
                    where
                        sprinkler.IsSprinkler()
                        || (sprinkler.bigCraftable.Value && coverageBySprinklerID.ContainsKey(sprinkler.ParentSheetIndex)) // older custom sprinklers
                    select sprinkler
                )
                .ToArray();

            // yield sprinkler coverage
            var covered = new HashSet<Vector2>();
            var groups = new List<TileGroup>();
            foreach (SObject sprinkler in sprinklers)
            {
                TileData[] tiles = this
                    .GetCoverage(sprinkler, sprinkler.TileLocation, coverageBySprinklerID)
                    .Select(pos => new TileData(pos, this.Wet))
                    .ToArray();

                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);

                groups.Add(new TileGroup(tiles, outerBorderColor: sprinkler.TileLocation == cursorTile ? this.SelectedColor : this.Wet.Color));
            }

            // yield dry crops
            var dryCrops = this
                .GetDryCrops(location, visibleTiles, covered)
                .Select(pos => new TileData(pos, this.Dry));
            groups.Add(new TileGroup(dryCrops, outerBorderColor: this.Dry.Color));

            // yield sprinkler being placed
            SObject heldObj = Game1.player.ActiveObject;
            if (heldObj != null && coverageBySprinklerID.ContainsKey(heldObj.ParentSheetIndex))
            {
                var tiles = this
                    .GetCoverage(heldObj, cursorTile, coverageBySprinklerID)
                    .Select(pos => new TileData(pos, this.Wet, this.Wet.Color * 0.75f));
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
            return terrain is HoeDirt dirt && dirt.crop != null;
        }

        /// <summary>Get the current relative sprinkler coverage, including any dynamic mod changes.</summary>
        private IDictionary<int, Vector2[]> GetCustomSprinklerTiles()
        {
            var tilesBySprinklerID = new Dictionary<int, Vector2[]>();

            // Better Sprinklers
            if (this.Mods.BetterSprinklers.IsLoaded)
            {
                foreach (var pair in this.Mods.BetterSprinklers.GetSprinklerTiles())
                    tilesBySprinklerID[pair.Key] = pair.Value;
            }

            // Line Sprinklers
            if (this.Mods.LineSprinklers.IsLoaded)
            {
                foreach (var pair in this.Mods.LineSprinklers.GetSprinklerTiles())
                    tilesBySprinklerID[pair.Key] = pair.Value;
            }

            // Simple Sprinkler
            if (this.Mods.SimpleSprinkler.IsLoaded)
            {
                foreach (var pair in this.Mods.SimpleSprinkler.GetNewSprinklerTiles())
                    tilesBySprinklerID[pair.Key] = pair.Value;
            }

            return tilesBySprinklerID;
        }

        /// <summary>Get a sprinkler tile radius.</summary>
        /// <param name="sprinkler">The sprinkler whose radius to get.</param>
        /// <param name="origin">The sprinkler's tile.</param>
        /// <param name="customSprinklerRanges">The custom sprinkler ranges centered on (0, 0) indexed by sprinkler ID.</param>
        /// <remarks>Derived from <see cref="SObject.DayUpdate"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(SObject sprinkler, Vector2 origin, IDictionary<int, Vector2[]> customSprinklerRanges)
        {
            IEnumerable<Vector2> tiles = sprinkler.GetSprinklerTiles();

            if (customSprinklerRanges.TryGetValue(sprinkler.ParentSheetIndex, out Vector2[] customTiles))
                tiles = new HashSet<Vector2>(tiles.Concat(customTiles.Select(tile => tile + origin)));

            return tiles;
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
