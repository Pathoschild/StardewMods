using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops;

/// <summary>A data layer which shows the water range for paddy crops.</summary>
internal class CropPaddyWaterLayer : BaseLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for tiles within range of water for paddy crops.</summary>
    private readonly LegendEntry InRange;

    /// <summary>The legend entry for tiles not within range of water for paddy crops.</summary>
    private readonly LegendEntry NotInRange;

    /// <summary>The previous location for which <see cref="TilesInRange"/> was cached.</summary>
    private GameLocation? LastLocation;

    /// <summary>The cached tiles in range of open water for the current location.</summary>
    private readonly Dictionary<Vector2, bool> TilesInRange = [];

    /// <summary>A sample paddy crop.</summary>
    private readonly Lazy<Crop> PaddyCrop = new(() => new SamplePaddyCrop());


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public CropPaddyWaterLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.CropPaddyWater_Name(), config)
    {
        const string layerId = "WaterForPaddyCrops";

        this.Legend = [
            this.InRange = new LegendEntry(I18n.Keys.CropPaddyWater_InRange, colors.Get(layerId, "InRange", Color.Green)),
            this.NotInRange = new LegendEntry(I18n.Keys.CropPaddyWater_NotInRange, colors.Get(layerId, "NotInRange", Color.Red))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        // update cache on location change
        if (this.LastLocation == null || !object.ReferenceEquals(location, this.LastLocation))
        {
            this.LastLocation = location;
            this.TilesInRange.Clear();
        }

        // get paddy tiles
        var tilesInRange = visibleTiles.ToLookup(this.GetTilesInRange(location, visibleTiles).Contains);
        return [
            new TileGroup(tilesInRange[true].Select(pos => new TileData(pos, this.InRange)), outerBorderColor: this.InRange.Color),
            new TileGroup(tilesInRange[false].Select(pos => new TileData(pos, this.NotInRange)))
        ];
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get tiles within range of open water for paddy crops.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    /// <remarks>Derived from <see cref="HoeDirt.paddyWaterCheck"/>.</remarks>
    private HashSet<Vector2> GetTilesInRange(GameLocation location, IReadOnlySet<Vector2> visibleTiles)
    {
        HashSet<Vector2> tiles = [];

        foreach (Vector2 tile in visibleTiles)
        {
            if (!this.TilesInRange.TryGetValue(tile, out bool inRange))
                this.TilesInRange[tile] = inRange = this.RecalculateTileInRange(location, tile, this.PaddyCrop);

            if (inRange)
                tiles.Add(tile);
        }

        return tiles;
    }

    /// <summary>Get whether the tile is in range, without caching.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile to check.</param>
    /// <param name="samplePaddyCrop">A sample paddy crop.</param>
    private bool RecalculateTileInRange(GameLocation location, Vector2 tile, Lazy<Crop> samplePaddyCrop)
    {
        // water tile
        if (location.isWaterTile((int)tile.X, (int)tile.Y))
            return false;

        // dirt tile
        // note: paddyWaterCheck() only works if the dirt contains a paddy crop
        HoeDirt? dirt = this.GetDirt(location, tile, ignorePot: true);
        if (dirt?.hasPaddyCrop() != true && this.IsTillable(location, tile) && location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
        {
            dirt = new HoeDirt(HoeDirt.watered, samplePaddyCrop.Value)
            {
                Location = location,
                Tile = tile
            };
        }
        return dirt?.paddyWaterCheck() ?? false;
    }

    /// <summary>A sample paddy crop.</summary>
    private class SamplePaddyCrop : Crop
    {
        /// <inheritdoc />
        public override bool isPaddyCrop()
        {
            return true;
        }
    }
}
