using System;
using System.Collections.Generic;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <inheritdoc cref="IDataLayersApi" />
public class Api : IDataLayersApi
{
    /*********
    ** Fields
    *********/
    /// <summary>The unique ID for the mod which requested this API.</summary>
    private readonly string ModId;

    /// <summary>Manages available color schemes and colors.</summary>
    private readonly ColorRegistry ColorRegistry;

    /// <summary>Manages the data layers that should be available in-game.</summary>
    private readonly LayerRegistry LayerRegistry;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modId">The unique ID for the mod which requested this API.</param>
    /// <param name="colorRegistry">Manages available color schemes and colors.</param>
    /// <param name="layerRegistry">Manages the data layers that should be available in-game.</param>
    internal Api(string modId, ColorRegistry colorRegistry, LayerRegistry layerRegistry)
    {
        this.ModId = modId;
        this.ColorRegistry = colorRegistry;
        this.LayerRegistry = layerRegistry;
    }

    /// <inheritdoc />
    public void RegisterColorSchemes(Dictionary<string, Dictionary<string, string?>> schemeData, string assetName)
    {
        // validate
        ArgumentNullException.ThrowIfNull(schemeData);
        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentException($"The '{nameof(assetName)}' argument must be specified.", nameof(assetName));

        // register color schemes
        this.ColorRegistry.LoadSchemes(schemeData, assetName);
    }

    /// <inheritdoc />
    public void RegisterLayer(string id, Func<string> name, GetTileGroupsDelegate getTileGroups, UpdateTilesDelegate updateTiles)
    {
        // validate
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException($"The '{nameof(id)}' argument must be specified.", nameof(id));
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(getTileGroups);
        ArgumentNullException.ThrowIfNull(updateTiles);

        // register layer
        string globalId = $"{this.ModId}_{id}";
        var layerData = new ApiDataLayer(globalId, id, name, getTileGroups, updateTiles);
        this.LayerRegistry.RegisterCustomLayer(layerData);
    }
}
