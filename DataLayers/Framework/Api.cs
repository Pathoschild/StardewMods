using System.Collections.Generic;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <inheritdoc cref="IDataLayersApi" />
public class Api : IDataLayersApi
{
    /*********
    ** Fields
    *********/
    /// <summary>Manages available color schemes and colors.</summary>
    private readonly ColorRegistry ColorRegistry;

    /// <summary>Manages the data layers that should be available in-game.</summary>
    private readonly LayerRegistry LayerRegistry;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="colorRegistry">Manages available color schemes and colors.</param>
    /// <param name="layerRegistry">Manages the data layers that should be available in-game.</param>
    internal Api(ColorRegistry colorRegistry, LayerRegistry layerRegistry)
    {
        this.ColorRegistry = colorRegistry;
        this.LayerRegistry = layerRegistry;
    }

    /// <inheritdoc />
    public void RegisterColorSchemes(Dictionary<string, Dictionary<string, string?>> schemeData, string assetName)
    {
        this.ColorRegistry.LoadSchemes(schemeData, assetName);
    }

    /// <inheritdoc />
    public void RegisterLayer(IManifest mod, string id, IDataLayer layer)
    {
        string globalId = $"{mod.UniqueID}_{id}";
        var layerData = new LayerRegistration(globalId, id, layer);

        this.LayerRegistry.RegisterCustomLayer(layerData);
    }
}
