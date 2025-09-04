using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>The base implementation for a data layer.</summary>
internal abstract class BaseLayer : ILayer
{
    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public int UpdateTickRate { get; }

    /// <inheritdoc />
    public bool UpdateWhenVisibleTilesChange { get; }

    /// <inheritdoc />
    public KeybindList ShortcutKey { get; }

    /// <inheritdoc />
    public LegendEntry[] Legend { get; protected set; } = [];

    /// <inheritdoc />
    public bool AlwaysShowGrid { get; protected set; }


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public bool UpdateMetadata()
    {
        return false;
    }

    /// <inheritdoc />
    public abstract IReadOnlyCollection<TileGroup> Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile);


    /*********
    ** Protected methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="name">The data layer name.</param>
    /// <param name="config">The data layers settings.</param>
    protected BaseLayer(string name, LayerConfig config)
    {
        this.Id = this.GetType().FullName!;
        this.Name = name;
        this.UpdateTickRate = (int)(60 / config.UpdatesPerSecond);
        this.UpdateWhenVisibleTilesChange = config.UpdateWhenViewChange;
        this.ShortcutKey = config.ShortcutKey;
    }

    /// <summary>Get the dirt instance for a tile, if any.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile to check.</param>
    /// <param name="ignorePot">Whether to ignore dirt in indoor pots.</param>
    protected HoeDirt? GetDirt(GameLocation location, Vector2 tile, bool ignorePot = false)
    {
        if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && terrain is HoeDirt dirt)
            return dirt;
        if (!ignorePot && location.objects.TryGetValue(tile, out SObject obj) && obj is IndoorPot pot)
            return pot.hoeDirt.Value;

        return null;
    }

    /// <summary>Get whether a tile is tillable.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile to check.</param>
    /// <remarks>Derived from <see cref="StardewValley.Tools.Hoe.DoFunction"/>.</remarks>
    protected bool IsTillable(GameLocation location, Vector2 tile)
    {
        return location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
    }

    /// <summary>Get whether dirt contains a dead crop.</summary>
    /// <param name="dirt">The dirt to check.</param>
    protected bool IsDeadCrop(HoeDirt? dirt)
    {
        return dirt?.crop?.dead.Value == true;
    }

    /// <summary>Get the tile area around an origin that's both within a square radius and within the visible tile area.</summary>
    /// <param name="radius">The number of tiles around the origin tile to include.</param>
    /// <param name="origin">The tile at the center of the radius.</param>
    /// <param name="visibleArea">The tile area currently visible on the screen.</param>
    protected Rectangle GetVisibleRadiusArea(int radius, Vector2 origin, Rectangle visibleArea)
    {
        int x = (int)origin.X - radius;
        int y = (int)origin.Y - radius;
        int width = radius + 1 + radius;
        int height = radius + 1 + radius;

        // not visible
        if (x > visibleArea.Right || y > visibleArea.Bottom || x + width < visibleArea.X || y + height < visibleArea.Y)
            return Rectangle.Empty;

        // exclude areas outside visible area
        if (x < visibleArea.X)
        {
            width -= visibleArea.X - x;
            x = visibleArea.X;
        }
        if (y < visibleArea.Y)
        {
            height -= visibleArea.Y - y;
            y = visibleArea.Y;
        }
        if (x + width > visibleArea.Right)
            width = visibleArea.Right - x;
        if (y + height > visibleArea.Bottom)
            height = visibleArea.Bottom - y;

        return new Rectangle(x, y, width, height);
    }
}
