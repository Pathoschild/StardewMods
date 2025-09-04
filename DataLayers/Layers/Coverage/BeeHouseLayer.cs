using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage;

/// <summary>A data layer which shows bee house coverage.</summary>
internal class BeeHouseLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for tiles covered by a bee house.</summary>
    private readonly LegendEntry Covered;

    /// <summary>The border color for the bee house under the cursor.</summary>
    private readonly Color SelectedColor;

    /// <summary>The maximum number of tiles from the center a bee house can cover.</summary>
    private readonly int MaxRadius = 5;

    /// <summary>The relative tile coordinates covered by a bee house.</summary>
    private readonly Vector2[] RelativeRange;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public BeeHouseLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.BeeHouses_Name(), config)
    {
        const string layerId = "BeeHouseCoverage";

        this.SelectedColor = colors.Get(layerId, "Selected", Color.Blue);
        this.Legend = [
            this.Covered = new LegendEntry(I18n.Keys.BeeHouses_Range, colors.Get(layerId, "Covered", Color.Green))
        ];
        this.RelativeRange = BeeHouseLayer
            .GetRelativeCoverage()
            .ToArray();
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        // yield coverage
        var groups = new List<TileGroup>();
        foreach (Vector2 origin in visibleArea.Expand(this.MaxRadius).GetTiles())
        {
            if (!location.objects.TryGetValue(origin, out SObject beeHouse) || !this.AppliesTo(beeHouse))
                continue;

            TileData[] tiles = this
                .GetCoverage(location, beeHouse.TileLocation, visibleTiles)
                .Select(pos => new TileData(pos, this.Covered))
                .ToArray();

            groups.Add(new TileGroup(tiles, outerBorderColor: beeHouse.TileLocation == cursorTile ? this.SelectedColor : this.Covered.Color));
        }

        // yield bee house being placed
        SObject heldObj = Game1.player.ActiveObject;
        if (this.AppliesTo(heldObj))
        {
            var tiles = this
                .GetCoverage(location, cursorTile, visibleTiles)
                .Select(pos => new TileData(pos, this.Covered, color: this.Covered.Color * 0.75f));
            groups.Add(new TileGroup(tiles, outerBorderColor: this.SelectedColor, shouldExport: false));
        }

        return groups.ToArray();
    }

    /// <inheritdoc />
    public bool AppliesTo(Item? item)
    {
        return
            item?.Name == "Bee House"
            && item is SObject obj
            && obj.bigCraftable.Value;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a bee house tile radius.</summary>
    /// <param name="location">The bee house's location.</param>
    /// <param name="origin">The bee house's tile.</param>
    /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
    /// <remarks>Derived from <see cref="SObject.checkForAction"/> and <see cref="Utility.findCloseFlower"/>.</remarks>
    private IEnumerable<Vector2> GetCoverage(GameLocation location, Vector2 origin, IReadOnlySet<Vector2> visibleTiles)
    {
        if (!location.IsOutdoors)
            yield break; // bee houses are hardcoded to only work outdoors

        foreach (Vector2 relativeTile in this.RelativeRange)
        {
            Vector2 tile = origin + relativeTile;
            if (visibleTiles.Contains(tile))
                yield return tile;
        }
    }

    /// <summary>Get the relative tiles covered by a bee house.</summary>
    /// <remarks>Derived from <see cref="Utility.findCloseFlower"/>.</remarks>
    private static IEnumerable<Vector2> GetRelativeCoverage()
    {
        const int range = 5;

        Queue<Vector2> queue = new Queue<Vector2>();
        HashSet<Vector2> visited = [];
        queue.Enqueue(Vector2.Zero);
        while (queue.Count > 0)
        {
            Vector2 tile = queue.Dequeue();
            yield return tile;
            foreach (Vector2 adjacentTile in Utility.getAdjacentTileLocations(tile))
            {
                if (!visited.Contains(adjacentTile) && (Math.Abs(adjacentTile.X) + (double)Math.Abs(adjacentTile.Y)) <= range)
                    queue.Enqueue(adjacentTile);
            }
            visited.Add(tile);
        }
    }
}
