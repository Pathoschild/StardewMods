using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
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
        private readonly Color SelectedColor = Color.Blue;

        /// <summary>The maximum number of tiles from the center a scarecrow can protect.</summary>
        private readonly int MaxRadius = 8;

        /// <summary>Object IDs for custom mod scarecrows not covered by the default logic.</summary>
        private readonly int[] ModObjectIds;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        public ScarecrowLayer(ITranslationHelper translations, LayerConfig config, ModIntegrations mods)
            : base(translations.Get("scarecrows.name"), config)
        {
            this.Legend = new[]
            {
                this.Covered = new LegendEntry(translations, "scarecrows.protected", Color.Green),
                this.Exposed = new LegendEntry(translations, "scarecrows.exposed", Color.Red)
            };
            this.ModObjectIds = this.GetModScarecrowIDs(mods).ToArray();
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            // get scarecrows
            Vector2[] searchTiles = visibleArea.Expand(this.MaxRadius).GetTiles().ToArray();
            SObject[] scarecrows =
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
            foreach (SObject scarecrow in scarecrows)
            {
                TileData[] tiles = this.GetCoverage(scarecrow.TileLocation).Select(pos => new TileData(pos, this.Covered)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorderColor: scarecrow.TileLocation == cursorTile ? this.SelectedColor : this.Covered.Color);
            }

            // yield exposed crops
            TileData[] exposedCrops = this.GetExposedCrops(location, visibleTiles, covered).Select(pos => new TileData(pos, this.Exposed)).ToArray();
            yield return new TileGroup(exposedCrops, outerBorderColor: this.Exposed.Color);

            // yield scarecrow being placed
            SObject heldObj = Game1.player.ActiveObject;
            if (this.IsScarecrow(heldObj))
            {
                TileData[] tiles = this.GetCoverage(cursorTile).Select(pos => new TileData(pos, this.Covered, this.Covered.Color * 0.75f)).ToArray();
                yield return new TileGroup(tiles, outerBorderColor: this.SelectedColor, shouldExport: false);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the object IDs for known mod scarecrows.</summary>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        private IEnumerable<int> GetModScarecrowIDs(ModIntegrations mods)
        {
            if (mods.PrismaticTools.IsLoaded && mods.PrismaticTools.ArePrismaticSprinklersScarecrows())
                yield return mods.PrismaticTools.GetSprinklerID();
        }

        /// <summary>Get whether a map object is a scarecrow.</summary>
        /// <param name="obj">The map object.</param>
        /// <remarks>Derived from <see cref="Farm.addCrows"/>.</remarks>
        private bool IsScarecrow(SObject obj)
        {
            if (obj == null)
                return false;

            // vanilla sprinkler
            if (obj.bigCraftable.Value && obj.Name.Contains("arecrow"))
                return true;

            // mod sprinkler
            if (!obj.bigCraftable.Value && this.ModObjectIds.Contains(obj.ParentSheetIndex))
                return true;

            return false;
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
