using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage
{
    /// <summary>A data layer which shows Junimo hut coverage.</summary>
    internal class JunimoHutLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for tiles harvested by a Junimo hut.</summary>
        private readonly LegendEntry Covered;

        /// <summary>The legend entry for tiles not harvested by a Junimo hut.</summary>
        private readonly LegendEntry NotCovered;

        /// <summary>The border color for the Junimo hut under the cursor.</summary>
        private readonly Color SelectedColor;

        /// <summary>Handles access to the supported mod integrations.</summary>
        private readonly ModIntegrations Mods;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The data layer settings.</param>
        /// <param name="colors">The colors to render.</param>
        /// <param name="mods">Handles access to the supported mod integrations.</param>
        public JunimoHutLayer(LayerConfig config, ColorScheme colors, ModIntegrations mods)
            : base(I18n.JunimoHuts_Name(), config)
        {
            const string layerId = "JunimoHutCoverage";

            this.Mods = mods;
            this.SelectedColor = colors.Get(layerId, "Selected", Color.Blue);
            this.Legend = new[]
            {
                this.Covered = new LegendEntry(I18n.Keys.JunimoHuts_CanHarvest, colors.Get(layerId, "Covered", Color.Green)),
                this.NotCovered = new LegendEntry(I18n.Keys.JunimoHuts_CannotHarvest, colors.Get(layerId, "NotCovered", Color.Red))
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            if (!location.IsBuildableLocation())
                return Array.Empty<TileGroup>();

            // get Junimo huts
            Rectangle searchArea = visibleArea;
            JunimoHut[] huts =
                (
                    from JunimoHut hut in location.buildings.OfType<JunimoHut>()
                    where new Rectangle(hut.tileX + 1, hut.tileY + 1, hut.cropHarvestRadius, hut.cropHarvestRadius).Intersects(searchArea) // range centered on hut door
                    select hut
                )
                .ToArray();

            // yield Junimo hut coverage
            var groups = new List<TileGroup>();
            var covered = new HashSet<Vector2>();
            foreach (JunimoHut hut in huts)
            {
                TileData[] tiles = this
                    .GetCoverage(hut, hut.tileX.Value, hut.tileY.Value)
                    .Select(pos => new TileData(pos, this.Covered))
                    .ToArray();

                foreach (TileData tile in tiles)
                    covered.Add(tile.TilePosition);

                groups.Add(new TileGroup(tiles, outerBorderColor: this.IntersectsTile(hut, cursorTile) ? this.SelectedColor : this.Covered.Color));
            }

            // yield unharvested crops
            IEnumerable<TileData> unharvested = this
                .GetUnharvestedCrops(location, visibleTiles, covered)
                .Select(pos => new TileData(pos, this.NotCovered));
            groups.Add(new TileGroup(unharvested, outerBorderColor: this.NotCovered.Color));

            // yield hut being placed in build menu
            if (this.IsBuildingHut())
            {
                Vector2 tile = TileHelper.GetTileFromCursor();
                var tiles = this
                    .GetCoverage(new JunimoHut(), (int)tile.X, (int)tile.Y) // TODO: get hut from carpenter menu
                    .Select(pos => new TileData(pos, this.Covered, this.Covered.Color * 0.75f));

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
            return terrain is HoeDirt { crop: not null };
        }

        /// <summary>Get whether the build menu is open with a Junimo hut selected.</summary>
        private bool IsBuildingHut()
        {
            // vanilla menu
            if (Game1.activeClickableMenu is CarpenterMenu carpenterMenu && carpenterMenu.Blueprint.Id == "Junimo Hut")
                return true;

            // Pelican Fiber menu
            if (this.Mods.PelicanFiber.IsLoaded && this.Mods.PelicanFiber.GetBuildMenuBlueprint()?.Id == "Junimo Hut")
                return true;

            return false;
        }

        /// <summary>Get whether a hut's building covers the given tile.</summary>
        /// <param name="hut">The Junimo hut.</param>
        /// <param name="tile">The tile position.</param>
        private bool IntersectsTile(JunimoHut hut, Vector2 tile)
        {
            return new Rectangle(hut.tileX.Value, hut.tileY.Value, hut.tilesWide.Value, hut.tilesHigh.Value).Contains((int)tile.X, (int)tile.Y);
        }

        /// <summary>Get a Junimo hut tile radius.</summary>
        /// <param name="hut">The Junimo hut.</param>
        /// <summary>Get a Junimo hut tile radius.</summary>
        /// <param name="tileX">The hut's tile X position.</param>
        /// <param name="tileY">The hut's tile Y position.</param>
        /// <remarks>Derived from <see cref="StardewValley.Characters.JunimoHarvester.pathfindToNewCrop"/>.</remarks>
        private IEnumerable<Vector2> GetCoverage(JunimoHut hut, int tileX, int tileY)
        {
            // center radius on door
            tileX++;
            tileY++;
            int radius = hut.cropHarvestRadius;

            // get tiles
            for (int x = tileX - radius; x <= tileX + radius; x++)
            {
                for (int y = tileY - radius; y <= tileY + radius; y++)
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
