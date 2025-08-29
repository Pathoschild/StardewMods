using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.DataLayers.Layers;

/// <summary>A data layer which chooses an overlay automatically based on context like the held item.</summary>
internal class AutoLayer : ILayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend to show when no sub-layer is active.</summary>
    private readonly LegendEntry[] EmptyLegend = [];

    /// <summary>The number of ticks between each sub-layer selection check.</summary>
    private readonly int ReselectLayerTickRate;

    /// <summary>Handles access to the supported mod integrations.</summary>
    private readonly ModIntegrations Mods;

    /// <summary>The sub-layers which show coverage for an active building.</summary>
    private readonly IAutoBuildingLayer[] BuildingLayers;

    /// <summary>The sub-layers which show coverage for an active item.</summary>
    private readonly IAutoItemLayer[] ItemLayers;

    /// <summary>The number of ticks until we recheck which sub-layer should be shown.</summary>
    private int ReselectLayerCountdown;

    /// <summary>The current sub-layer being shown, if any.</summary>
    private ILayer? CurrentLayer;


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string Name => this.CurrentLayer != null
        ? I18n.Auto_NameActive(layerName: this.CurrentLayer.Name)
        : I18n.Auto_NameInactive();

    /// <inheritdoc />
    public int UpdateTickRate => this.CurrentLayer?.UpdateTickRate ?? 60;

    /// <inheritdoc />
    public bool UpdateWhenVisibleTilesChange => this.CurrentLayer?.UpdateWhenVisibleTilesChange ?? false;

    /// <inheritdoc />
    public LegendEntry[] Legend => this.CurrentLayer?.Legend ?? this.EmptyLegend;

    /// <inheritdoc />
    public KeybindList ShortcutKey { get; }

    /// <inheritdoc />
    public bool AlwaysShowGrid => this.CurrentLayer?.AlwaysShowGrid ?? false;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The mod config.</param>
    /// <param name="mods">The loaded mod integrations.</param>
    /// <param name="buildingLayers">The layers which show coverage for an active building.</param>
    /// <param name="itemLayers">The layers which show coverage for an active item.</param>
    public AutoLayer(ModConfig config, ModIntegrations mods, IAutoBuildingLayer[] buildingLayers, IAutoItemLayer[] itemLayers)
    {
        LayerConfig layerConfig = config.Layers.AutoLayer;

        this.Id = this.GetType().FullName!;
        this.ReselectLayerTickRate = (int)(60 / layerConfig.UpdatesPerSecond);
        this.ShortcutKey = layerConfig.ShortcutKey;
        this.Mods = mods;
        this.BuildingLayers = buildingLayers;
        this.ItemLayers = itemLayers;
    }

    /// <inheritdoc />
    public bool UpdateMetadata()
    {
        // debounce updates
        this.ReselectLayerCountdown--;
        if (this.ReselectLayerCountdown > 0)
            return false;
        this.ReselectLayerCountdown = this.ReselectLayerTickRate;

        // recheck active layer
        ILayer? layer = this.GetRelevantLayer();
        if (!object.ReferenceEquals(layer, this.CurrentLayer))
        {
            this.CurrentLayer = layer;
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<TileGroup> Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        // get active layer
        ILayer? layer = this.CurrentLayer;
        if (layer is null)
            return [];

        // update layer
        GameLocation localLocation = location;
        Rectangle localVisibleArea = visibleArea;
        IReadOnlySet<Vector2> localVisibleTiles = visibleTiles;
        Vector2 localCursorTile = cursorTile;
        return layer.Update(ref localLocation, ref localVisibleArea, ref localVisibleTiles, ref localCursorTile);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the layer which should be shown based on the current context, if any.</summary>
    private ILayer? GetRelevantLayer()
    {
        // build menu
        string? buildingType = this.GetActiveBuildingId();
        if (buildingType != null)
        {
            foreach (IAutoBuildingLayer layer in this.BuildingLayers)
            {
                if (layer.AppliesTo(buildingType))
                    return layer;
            }

            return null;
        }

        // held item
        if (Context.IsPlayerFree && Game1.player.ActiveItem is { } heldObj)
        {
            foreach (IAutoItemLayer layer in this.ItemLayers)
            {
                if (layer.AppliesTo(heldObj))
                    return layer;
            }
        }

        return null;
    }

    /// <summary>Get the building type currently selected by the player, if any.</summary>
    private string? GetActiveBuildingId()
    {
        if (Context.IsWorldReady && Game1.activeClickableMenu is { } menu)
        {
            // vanilla menu
            if (menu is CarpenterMenu carpenterMenu)
                return carpenterMenu.Blueprint.Id;

            // Pelican Fiber menu
            if (this.Mods.PelicanFiber.IsLoaded)
                return this.Mods.PelicanFiber.GetBuildMenuBlueprint()?.Id;
        }

        return null;
    }
}
