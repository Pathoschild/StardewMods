using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using Pathoschild.Stardew.DataLayers.Framework.ConfigModels;
using StardewValley;
using StardewValley.Tools;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>A data layer which shows the 'distance from shore' or 'water depth' for the purposes of fishing.</summary>
internal class FishingDepthLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The previous location for which <see cref="WaterDepths"/> was cached.</summary>
    private GameLocation? LastLocation;

    /// <summary>The maximum water depth.</summary>
    private const int MaxDepth = 5;

    /// <summary>The cached tiles depths.</summary>
    private readonly Dictionary<Vector2, int> WaterDepths = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public FishingDepthLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.FishingDepth_Name(), config)
    {
        const string layerId = "FishingDepth";

        this.Legend = [
            new LegendEntry(I18n.Keys.FishingDepth_One, colors.Get(layerId, "One", Color.Red)),
            new LegendEntry(I18n.Keys.FishingDepth_Two, colors.Get(layerId, "Two", Color.Coral)),
            new LegendEntry(I18n.Keys.FishingDepth_Three, colors.Get(layerId, "Three", Color.Wheat)),
            new LegendEntry(I18n.Keys.FishingDepth_Four, colors.Get(layerId, "Four", Color.Yellow)),
            new LegendEntry(I18n.Keys.FishingDepth_Max, colors.Get(layerId, "Max", Color.Green))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        // update cache on location change
        if (this.LastLocation == null || !object.ReferenceEquals(location, this.LastLocation))
        {
            this.LastLocation = location;
            this.WaterDepths.Clear();
        }

        // get paddy tiles
        GameLocation loc = location;
        var tilesInRange = visibleTiles.ToLookup(tile => this.GetWaterDepth(ref loc, tile));

        TileGroup[] groups = new TileGroup[MaxDepth];
        for (int i = 0; i < groups.Length; i++)
        {
            LegendEntry legendEntry = i < this.Legend.Length
                ? this.Legend[i]
                : this.Legend[MaxDepth];
            groups[i] = new TileGroup(tilesInRange[i + 1].Select(pos => new TileData(pos, legendEntry)), outerBorderColor: i == MaxDepth - 1 ? legendEntry.Color : null);
        }

        return groups;
    }

    /// <inheritdoc />
    public bool AppliesTo(Item item)
    {
        return item is FishingRod;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the water depth at the given tile.</summary>
    /// <param name="location">The location to check.</param>
    /// <param name="tile">the tile position to check.</param>
    private int GetWaterDepth(ref readonly GameLocation location, Vector2 tile)
    {
        if (!this.WaterDepths.TryGetValue(tile, out int depth))
        {
            depth = location.isTileFishable((int)tile.X, (int)tile.Y)
                ? FishingRod.distanceToLand((int)tile.X, (int)tile.Y, location)
                : 0;

            if (depth > MaxDepth)
                depth = MaxDepth;

            this.WaterDepths[tile] = depth;
        }

        return depth;
    }
}
