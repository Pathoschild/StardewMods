using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>A data layer which shows whether tiles can be built on.</summary>
internal class BuildableLayer : BaseLayer, IAutoBuildingLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for buildable tiles.</summary>
    private readonly LegendEntry Buildable;

    /// <summary>The legend entry for buildable but occupied tiles.</summary>
    private readonly LegendEntry Occupied;

    /// <summary>The legend entry for non-buildable tiles.</summary>
    private readonly LegendEntry NonBuildable;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public BuildableLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.Buildable_Name(), config)
    {
        const string layerId = "Buildable";

        this.Legend = [
            this.Buildable = new LegendEntry(I18n.Keys.Buildable_Buildable, colors.Get(layerId, "Buildable", Color.Green)),
            this.Occupied = new LegendEntry(I18n.Keys.Buildable_Occupied, colors.Get(layerId, "Occupied", Color.Orange)),
            this.NonBuildable = new LegendEntry(I18n.Keys.Buildable_NotBuildable, colors.Get(layerId, "NotBuildable", Color.Red))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        var buildableTiles = this
            .GetTiles(location, visibleTiles)
            .ToLookup(p => p.Type.Id == this.Buildable.Id);

        return [
            new TileGroup(buildableTiles[true], outerBorderColor: this.Buildable.Color),
            new TileGroup(buildableTiles[false])
        ];
    }

    /// <inheritdoc />
    public bool AppliesTo(string buildingType)
    {
        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the updated data layer tiles.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    private IEnumerable<TileData> GetTiles(GameLocation location, IReadOnlySet<Vector2> visibleTiles)
    {
        // buildable location
        if (location.IsBuildableLocation())
        {
            foreach (Vector2 tile in visibleTiles)
            {
                // get color
                LegendEntry type;
                if (this.IsBuildable(location, tile))
                    type = this.IsOccupied(location, tile) ? this.Occupied : this.Buildable;
                else
                    type = this.NonBuildable;

                // yield
                yield return new TileData(tile, type);
            }
        }

        // non-buildable location
        else
        {
            foreach (Vector2 tile in visibleTiles)
                yield return new TileData(tile, this.NonBuildable);
        }
    }


    /// <summary>Get whether a tile is buildable.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile to check.</param>
    /// <remarks>Derived from <see cref="GameLocation.buildStructure(Building, Vector2, Farmer, bool)"/>.</remarks>
    private bool IsBuildable(GameLocation location, Vector2 tile)
    {
        return location.isBuildable(tile);
    }

    /// <summary>Get whether a tile is blocked due to something it contains.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile to check.</param>
    /// <remarks>Derived from <see cref="GameLocation.buildStructure(Building, Vector2, Farmer, bool)"/>.</remarks>
    private bool IsOccupied(GameLocation location, Vector2 tile)
    {
        // buildings
        foreach (Building building in location.buildings)
        {
            if (building.occupiesTile(tile))
                return true;
        }

        // players
        foreach (Farmer farmer in location.farmers)
        {
            if (farmer.GetBoundingBox().Intersects(new Rectangle((int)tile.X * Game1.tileSize, (int)tile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize)))
                return false;
        }

        return false;
    }
}
