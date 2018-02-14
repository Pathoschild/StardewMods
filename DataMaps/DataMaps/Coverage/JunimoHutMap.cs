using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.PelicanFiber;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataMaps.DataMaps.Coverage
{
    /// <summary>A data map which shows Junimo hut coverage.</summary>
    internal class JunimoHutMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for tiles harvested by a Junimo hut.</summary>
        private readonly Color CoveredColor = Color.Green;

        /// <summary>The color for tiles not harvested by a Junimo hut.</summary>
        private readonly Color NotCoveredColor = Color.Red;

        /// <summary>The border color for the Junimo hut under the cursor.</summary>
        private readonly Color SelectedColor = Color.Blue;

        /// <summary>The maximum number of tiles from the center a Junimo hut can harvest.</summary>
        private readonly int MaxRadius = JunimoHut.cropHarvestRadius;

        /// <summary>Handles access to the Pelican Fiber mod.</summary>
        private readonly PelicanFiberIntegration PelicanFiber;


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
        /// <param name="pelicanFiber">Handles access to the Pelican Fiber mod.</param>
        public JunimoHutMap(ITranslationHelper translations, PelicanFiberIntegration pelicanFiber)
        {
            this.PelicanFiber = pelicanFiber;

            this.Name = translations.Get("maps.junimo-huts.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.junimo-huts.can-harvest"), this.CoveredColor),
                new LegendEntry(translations.Get("maps.junimo-huts.cannot-harvest"), this.NotCoveredColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            if (!(location is BuildableGameLocation buildableLocation))
                yield break;

            Vector2[] visibleTiles = visibleArea.GetTiles().ToArray();

            // get Junimo huts
            Rectangle searchArea = visibleArea.Expand(this.MaxRadius);
            JunimoHut[] huts =
                (
                    from JunimoHut hut in buildableLocation.buildings.OfType<JunimoHut>()
                    where searchArea.Contains(hut.tileX, hut.tileY)
                    select hut
                )
                .ToArray();

            // yield Junimo hut coverage
            HashSet<Vector2> covered = new HashSet<Vector2>();
            foreach (JunimoHut hut in huts)
            {
                TileData[] tiles = this.GetCoverage(hut.tileX, hut.tileY).Select(pos => new TileData(pos, this.CoveredColor)).ToArray();
                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);
                yield return new TileGroup(tiles, outerBorderColor: this.IntersectsTile(hut, cursorTile) ? this.SelectedColor : this.CoveredColor);
            }

            // yield unharvested crops
            TileData[] unharvested = this.GetUnharvestedCrops(location, visibleTiles, covered).Select(pos => new TileData(pos, this.NotCoveredColor)).ToArray();
            yield return new TileGroup(unharvested, outerBorderColor: this.NotCoveredColor);

            // yield hut being placed in build menu
            if (this.IsBuildingHut())
            {
                Vector2 tile = TileHelper.GetTileFromCursor();
                TileData[] tiles = this.GetCoverage((int)tile.X, (int)tile.Y).Select(pos => new TileData(pos, this.CoveredColor * 0.75f)).ToArray();
                yield return new TileGroup(tiles, outerBorderColor: this.SelectedColor);
            }
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

        /// <summary>Get whether the build menu is open with a Junimo hut selected.</summary>
        private bool IsBuildingHut()
        {
            // vanilla menu
            if (Game1.activeClickableMenu is CarpenterMenu carpenterMenu && carpenterMenu.CurrentBlueprint.name == "Junimo Hut")
                return true;

            // Pelican Fiber menu
            if (this.PelicanFiber.IsLoaded && this.PelicanFiber.GetBuildMenuBlueprint()?.name == "Junimo Hut")
                return true;

            return false;
        }

        /// <summary>Get whether a hut's building covers the given tile.</summary>
        /// <param name="hut">The Junimo hut.</param>
        /// <param name="tile">The tile position.</param>
        private bool IntersectsTile(JunimoHut hut, Vector2 tile)
        {
            return new Rectangle(hut.tileX, hut.tileY, hut.tilesWide, hut.tilesHigh).Contains((int)tile.X, (int)tile.Y);
        }

        /// <summary>Get a Junimo hut tile radius.</summary>
        /// <param name="tileX">The hut's tile X position.</param>
        /// <param name="tileY">The hut's tile X position.</param>
        /// <remarks>Derived from <see cref="StardewValley.Characters.JunimoHarvester.pathFindToNewCrop_doWork"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(int tileX, int tileY)
        {
            Vector2 origin = new Vector2(tileX + 1, tileY + 1); // centered on hut door
            for (int x = (int)origin.X - this.MaxRadius; x <= (int)origin.X + this.MaxRadius; x++)
            {
                for (int y = (int)origin.Y - this.MaxRadius; y <= (int)origin.Y + this.MaxRadius; y++)
                {
                    yield return new Vector2(x, y);
                }
            }
        }

        /// <summary>Get tiles containing crops not harvested by a Junimo hut.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        /// <param name="coveredTiles">The tiles harvested by Junimo huts.</param>
        private IEnumerable<Vector2> GetUnharvestedCrops(GameLocation location, Vector2[] visibleTiles, HashSet<Vector2> coveredTiles)
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
