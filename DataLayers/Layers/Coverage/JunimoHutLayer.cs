using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage;

/// <summary>A data layer which shows Junimo hut coverage.</summary>
internal class JunimoHutLayer : BaseLayer, IAutoBuildingLayer
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
        this.Legend = [
            this.Covered = new LegendEntry(I18n.Keys.JunimoHuts_CanHarvest, colors.Get(layerId, "Covered", Color.Green)),
            this.NotCovered = new LegendEntry(I18n.Keys.JunimoHuts_CannotHarvest, colors.Get(layerId, "NotCovered", Color.Red))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        if (!location.IsBuildableLocation())
            return [];

        // yield Junimo hut coverage
        var groups = new List<TileGroup>();
        var covered = new HashSet<Vector2>();
        foreach (Building building in location.buildings)
        {
            if (building is not JunimoHut hut)
                continue;

            Rectangle range = this.GetCoverageArea(hut);
            if (!range.Intersects(visibleArea))
                continue;

            TileData[] tiles = this
                .GetCoverage(hut, hut.tileX.Value, hut.tileY.Value, visibleArea)
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
                .GetCoverage(new JunimoHut(), (int)tile.X, (int)tile.Y, visibleArea) // TODO: get hut from carpenter menu
                .Select(pos => new TileData(pos, this.Covered, this.Covered.Color * 0.75f));

            groups.Add(new TileGroup(tiles, outerBorderColor: this.SelectedColor, shouldExport: false));
        }

        return groups.ToArray();
    }

    /// <inheritdoc />
    public bool AppliesTo(string? buildingType)
    {
        return buildingType == "Junimo Hut";
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
        if (Game1.activeClickableMenu is CarpenterMenu carpenterMenu && this.AppliesTo(carpenterMenu.Blueprint.Id))
            return true;

        // Pelican Fiber menu
        if (this.Mods.PelicanFiber.IsLoaded && this.AppliesTo(this.Mods.PelicanFiber.GetBuildMenuBlueprint()?.Id))
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

    /// <summary>Get a Junimo hut's coverage tile area.</summary>
    /// <param name="hut">The Junimo hut.</param>
    /// <summary>Get a Junimo hut tile radius.</summary>
    /// <remarks>Derived from <see cref="StardewValley.Characters.JunimoHarvester.pathfindToNewCrop"/>.</remarks>
    private Rectangle GetCoverageArea(JunimoHut hut)
    {
        // note: offset x/y by one so range to center on door
        return new Rectangle(
            hut.tileX.Value + 1 - hut.cropHarvestRadius,
            hut.tileY.Value + 1 - hut.cropHarvestRadius,
            hut.cropHarvestRadius + 1 + hut.cropHarvestRadius,
            hut.cropHarvestRadius + 1 + hut.cropHarvestRadius
        );
    }

    /// <summary>Get a Junimo hut's coverage tiles.</summary>
    /// <param name="hut">The Junimo hut.</param>
    /// <summary>Get a Junimo hut tile radius.</summary>
    /// <param name="tileX">The hut's tile X position.</param>
    /// <param name="tileY">The hut's tile Y position.</param>
    /// <param name="visibleArea">The tile area currently visible on the screen.</param>
    /// <remarks>Derived from <see cref="StardewValley.Characters.JunimoHarvester.pathfindToNewCrop"/>.</remarks>
    private IEnumerable<Vector2> GetCoverage(JunimoHut hut, int tileX, int tileY, Rectangle visibleArea)
    {
        Vector2 door = new Vector2(tileX + 1, tileY + 1);
        int radius = hut.cropHarvestRadius;

        foreach (Vector2 tile in this.GetVisibleRadiusArea(radius, door, visibleArea).GetTiles())
        {
            yield return tile;
        }
    }

    /// <summary>Get tiles containing crops not harvested by a Junimo hut.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    /// <param name="coveredTiles">The tiles harvested by Junimo huts.</param>
    private IEnumerable<Vector2> GetUnharvestedCrops(GameLocation location, IReadOnlySet<Vector2> visibleTiles, HashSet<Vector2> coveredTiles)
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
