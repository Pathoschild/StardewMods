using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>A data layer which shows whether tiles are tillable.</summary>
internal class TillableLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for tiles already tilled but not planted.</summary>
    private readonly LegendEntry Tilled;

    /// <summary>The legend entry for tillable tiles.</summary>
    private readonly LegendEntry Tillable;

    /// <summary>The legend entry for tillable but occupied tiles.</summary>
    private readonly LegendEntry Occupied;

    /// <summary>The legend entry for non-tillable tiles.</summary>
    private readonly LegendEntry NonTillable;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public TillableLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.Tillable_Name(), config)
    {
        const string layerId = "Tillable";

        this.Legend = [
            this.Tilled = new LegendEntry(I18n.Keys.Tillable_Tilled, colors.Get(layerId, "Tilled", Color.DarkMagenta)),
            this.Tillable = new LegendEntry(I18n.Keys.Tillable_Tillable, colors.Get(layerId, "Tillable", Color.Green)),
            this.Occupied = new LegendEntry(I18n.Keys.Tillable_Occupied, colors.Get(layerId, "Occupied", Color.Orange)),
            this.NonTillable = new LegendEntry(I18n.Keys.Tillable_NotTillable, colors.Get(layerId, "NotTillable", Color.Red))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        var tiles = this.GetTiles(location, visibleTiles).ToLookup(p => p.Type.Id);
        return [
            new TileGroup(tiles[this.Tilled.Id]),
            new TileGroup(tiles[this.Tillable.Id], outerBorderColor: this.Tillable.Color),
            new TileGroup(tiles[this.Occupied.Id]),
            new TileGroup(tiles[this.NonTillable.Id])
        ];
    }

    /// <inheritdoc />
    public bool AppliesTo(Item item)
    {
        return item is Hoe;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the updated data layer tiles.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    private IEnumerable<TileData> GetTiles(GameLocation location, IReadOnlySet<Vector2> visibleTiles)
    {
        foreach (Vector2 tile in visibleTiles)
        {
            LegendEntry type;
            if (!this.IsTillable(location, tile))
                type = this.NonTillable;
            else if (this.IsOccupied(location, tile))
                type = this.Occupied;
            else if (this.GetDirt(location, tile) != null && !location.isCropAtTile((int)tile.X, (int)tile.Y))
                type = this.Tilled;
            else
                type = this.Tillable;

            yield return new TileData(tile, type);
        }
    }

    /// <summary>Get whether a tile is blocked due to something it contains.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile to check.</param>
    /// <remarks>Derived from <see cref="StardewValley.Tools.Hoe.DoFunction"/>.</remarks>
    private bool IsOccupied(GameLocation location, Vector2 tile)
    {
        // impassable tiles (e.g. water)
        if (!location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
            return true;

        // objects & large terrain features
        if (location.objects.ContainsKey(tile) || location.largeTerrainFeatures.Any(p => p.Tile == tile))
            return true;

        // non-dirt terrain features
        if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
        {
            HoeDirt? dirt = feature as HoeDirt;
            if (dirt == null || dirt.crop != null)
                return true;
        }

        // buildings
        if (location.buildings.Any(building => building.occupiesTile(tile)))
            return true;

        return false;
    }
}
