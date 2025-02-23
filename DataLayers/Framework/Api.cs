using System;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <inheritdoc cref="IDataLayersApi" />
public class Api : IDataLayersApi
{
    /*********
    ** Fields
    *********/
    /// <summary>The unique ID for the mod which requested this API.</summary>
    private readonly string ModId;

    /// <summary>Manages the data layers that should be available in-game.</summary>
    private readonly LayerRegistry LayerRegistry;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modId">The unique ID for the mod which requested this API.</param>
    /// <param name="layerRegistry">Manages the data layers that should be available in-game.</param>
    internal Api(string modId, LayerRegistry layerRegistry)
    {
        this.ModId = modId;
        this.LayerRegistry = layerRegistry;
    }

    /// <inheritdoc />
    public void RegisterLayer(string id, Func<string> name, GetTileGroupsDelegate getTileGroups, UpdateTilesDelegate updateTiles, decimal? updatesPerSecond = null, bool updateWhenViewChanges = true)
    {
        // validate
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException($"The '{nameof(id)}' argument must be specified.", nameof(id));
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(getTileGroups);
        ArgumentNullException.ThrowIfNull(updateTiles);

        // register layer
        string globalId = $"{this.ModId}_{id}";
        var layerData = new ApiDataLayer(globalId, id, name, getTileGroups, updateTiles, updatesPerSecond, updateWhenViewChanges);
        this.LayerRegistry.RegisterCustomLayer(layerData);
    }
}
