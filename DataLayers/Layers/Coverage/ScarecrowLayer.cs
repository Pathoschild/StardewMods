using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage;

/// <summary>A data layer which shows scarecrow coverage.</summary>
internal class ScarecrowLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for tiles protected by a scarecrow.</summary>
    private readonly LegendEntry Covered;

    /// <summary>The legend entry for tiles not protected by a scarecrow.</summary>
    private readonly LegendEntry Exposed;

    /// <summary>The border color for the scarecrow under the cursor.</summary>
    private readonly Color SelectedColor;

    /// <summary>The maximum number of tiles from the center to search for scarecrows.</summary>
    private readonly int MaxSearchRadius = 20;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public ScarecrowLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.Scarecrows_Name(), config)
    {
        const string layerId = "ScarecrowCoverage";

        this.SelectedColor = colors.Get(layerId, "Selected", Color.Blue);
        this.Legend = [
            this.Covered = new LegendEntry(I18n.Keys.Scarecrows_Protected, colors.Get(layerId, "Covered", Color.Green)),
            this.Exposed = new LegendEntry(I18n.Keys.Scarecrows_Exposed, colors.Get(layerId, "NotCovered", Color.Red))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        // yield scarecrow coverage
        var covered = new HashSet<Vector2>();
        var groups = new List<TileGroup>();
        foreach (Vector2 origin in visibleArea.Expand(this.MaxSearchRadius).GetTiles())
        {
            if (!location.objects.TryGetValue(origin, out SObject scarecrow) || !this.AppliesTo(scarecrow))
                continue;

            TileData[] tiles = this
                .GetCoverage(scarecrow, visibleArea)
                .Select(pos => new TileData(pos, this.Covered))
                .ToArray();

            foreach (TileData tile in tiles)
                covered.Add(tile.TilePosition);

            groups.Add(new TileGroup(tiles, outerBorderColor: scarecrow.TileLocation == cursorTile ? this.SelectedColor : this.Covered.Color));
        }

        // yield exposed crops
        var exposedCrops = this
            .GetExposedCrops(location, visibleTiles, covered)
            .Select(pos => new TileData(pos, this.Exposed));
        groups.Add(new TileGroup(exposedCrops, outerBorderColor: this.Exposed.Color));

        // yield scarecrow being placed
        SObject heldObj = Game1.player.ActiveObject;
        if (this.AppliesTo(heldObj))
        {
            var tiles = this
                .GetCoverage(heldObj, visibleArea, cursorTile)
                .Select(pos => new TileData(pos, this.Covered, this.Covered.Color * 0.75f));
            groups.Add(new TileGroup(tiles, outerBorderColor: this.SelectedColor, shouldExport: false));
        }

        return groups.ToArray();
    }

    /// <inheritdoc />
    public bool AppliesTo(Item? item)
    {
        return
            item is SObject obj
            && obj.IsScarecrow();
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

    /// <summary>Get a scarecrow tile radius.</summary>
    /// <param name="scarecrow">The scarecrow to check.</param>
    /// <param name="visibleArea">The tile area currently visible on the screen.</param>
    /// <param name="overrideOrigin">The tile position to check from, if different from <see cref="SObject.TileLocation"/>.</param>
    /// <remarks>Derived from <see cref="Farm.addCrows"/>.</remarks>
    private IEnumerable<Vector2> GetCoverage(SObject scarecrow, Rectangle visibleArea, Vector2? overrideOrigin = null)
    {
        Vector2 origin = overrideOrigin ?? scarecrow.TileLocation;
        int radius = scarecrow.GetRadiusForScarecrow();

        foreach (Vector2 tile in this.GetVisibleRadiusArea(radius, origin, visibleArea).GetTiles())
        {
            if (Vector2.Distance(tile, origin) < radius)
                yield return tile;
        }
    }

    /// <summary>Get tiles containing crops not protected by a scarecrow.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    /// <param name="coveredTiles">The tiles protected by a scarecrow.</param>
    private IEnumerable<Vector2> GetExposedCrops(GameLocation location, IReadOnlySet<Vector2> visibleTiles, HashSet<Vector2> coveredTiles)
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
