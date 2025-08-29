using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>A data layer which implements a layer registered through the mod API.</summary>
internal class ModLayer : ILayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The layer registered through the mod API.</summary>
    private readonly ApiDataLayer Layer;

    /// <summary>The configuration data for this layer.</summary>
    private readonly LayerConfig Config;

    /// <summary>The current color scheme.</summary>
    private readonly ColorScheme Colors;

    /// <summary>The legend entries by group ID.</summary>
    private LegendEntry[] LegendImpl = [];

    /// <summary>The tile groups registered through the mod API.</summary>
    private Dictionary<string, TileGroupData>? TileGroups;



    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Id => this.Layer.UniqueId;

    /// <inheritdoc />
    public string Name => this.Layer.Name();

    /// <inheritdoc />
    public int UpdateTickRate => (int)(60 / this.Config.UpdatesPerSecond);

    /// <inheritdoc />
    public bool UpdateWhenVisibleTilesChange => this.Config.UpdateWhenViewChange;

    /// <inheritdoc />
    public LegendEntry[] Legend
    {
        get
        {
            this.InitializeIfNeeded();
            return this.LegendImpl;
        }
    }

    /// <inheritdoc />
    public KeybindList ShortcutKey => this.Config.ShortcutKey;

    /// <inheritdoc />
    public bool AlwaysShowGrid => false;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="layer"><inheritdoc cref="Layer" path="/summary" /></param>
    /// <param name="config"><inheritdoc cref="Config" path="/summary" /></param>
    /// <param name="colors"><inheritdoc cref="Colors" path="/summary" /></param>
    public ModLayer(ApiDataLayer layer, LayerConfig config, ColorScheme colors)
    {
        this.Layer = layer;
        this.Config = config;
        this.Colors = colors;
    }

    /// <inheritdoc />
    public bool UpdateMetadata()
    {
        return false;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<TileGroup> Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        this.InitializeIfNeeded();

        // get tiles
        List<TileGroup> tileGroups = [];
        foreach (IGrouping<string?, Vector2> group in this.Layer.UpdateTiles(location, visibleArea, visibleTiles, cursorTile))
        {
            if (group.Key is null)
                continue;

            TileGroupData? groupData = this.TileGroups.GetValueOrDefault(group.Key);
            if (groupData is null)
                continue;

            tileGroups.Add(
                new TileGroup(
                    tiles: group.Select(groupData.GetTileData),
                    outerBorderColor: groupData.BorderColor
                )
            );
        }

        return tileGroups;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Populate the cached tile groups and legend data if needed.</summary>
    [MemberNotNull(nameof(ModLayer.TileGroups))]
    private void InitializeIfNeeded()
    {
        if (this.TileGroups is null)
        {
            var tileGroups = new Dictionary<string, TileGroupData>(StringComparer.OrdinalIgnoreCase);

            this.Layer.GetTileGroups((id, name, overlayColor, borderColor) =>
            {
                Color overlayColorParsed = this.Colors.Get(this.Id, overlayColor, null);
                Color? borderColorParsed = borderColor != null
                    ? this.Colors.Get(this.Id, borderColor, null)
                    : null;

                tileGroups[id] = new TileGroupData(borderColorParsed, new LegendEntry(id, name(), overlayColorParsed));
            });

            this.TileGroups = tileGroups;
            this.LegendImpl = tileGroups.Select(p => p.Value.LegendEntry).ToArray();
        }
    }


    /// <summary>The data for a tile group registered through the API.</summary>
    /// <param name="BorderColor">The color for the tile group's outer borders, or <c>null</c> for no border.</param>
    /// <param name="LegendEntry">The legend entry.</param>
    private record TileGroupData(Color? BorderColor, LegendEntry LegendEntry)
    {
        /// <summary>Get the tile data for a tile position in this group.</summary>
        /// <param name="tile">The tile position.</param>
        public TileData GetTileData(Vector2 tile)
        {
            return new TileData(tile, this.LegendEntry);
        }
    }
}
