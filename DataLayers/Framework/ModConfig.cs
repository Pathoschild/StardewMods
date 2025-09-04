using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>The parsed mod configuration.</summary>
internal class ModConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>When two groups of the same color overlap, whether to draw one border around their combined edge instead of their individual borders.</summary>
    public bool CombineOverlappingBorders { get; set; } = true;

    /// <summary>Whether to show a tile grid when a layer is open.</summary>
    public bool ShowGrid { get; set; }

    /// <summary>The color scheme in <see cref="ColorScheme.AssetName"/> to use.</summary>
    public string ColorScheme { get; set; } = "Default";

    /// <summary>The legend opacity to set when the cursor overlaps it, as a value between 0 (transparent) and 1 (opaque).</summary>
    public float LegendAlphaOnHover { get; set; } = 0.2f;

    /// <summary>The key bindings.</summary>
    public ModConfigKeys Controls { get; set; } = new();

    /// <summary>The generic settings for each layer.</summary>
    public ModConfigLayers Layers { get; set; } = new();

    /// <summary>The generic settings for data layers registered through the API, indexed by mod ID and layer name.</summary>
    public Dictionary<string, LayerConfig> ModLayers { get; set; } = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Normalize the model after it's deserialized.</summary>
    /// <param name="context">The deserialization context.</param>
    [OnDeserialized]
    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract", Justification = SuppressReasons.MethodValidatesNullability)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaOnDeserialized)]
    public void OnDeserialized(StreamingContext context)
    {
        this.Controls ??= new ModConfigKeys();
        this.Layers ??= new ModConfigLayers();
    }

    /// <summary>Get the configuration entry for a layer registered through the API, creating one if it doesn't already exist.</summary>
    /// <param name="layer">The layer registered through the API.</param>
    /// <returns>The configuration to use for the layer.</returns>
    public LayerConfig GetModLayerConfig(ApiDataLayer layer)
    {
        string id = layer.UniqueId;
        if (!this.ModLayers.TryGetValue(id, out LayerConfig? config))
        {
            this.ModLayers[id] = config = new()
            {
                UpdatesPerSecond = layer.DefaultUpdatesPerSecond ?? LayerConfig.DefaultUpdatesPerSecond,
                UpdateWhenViewChange = layer.DefaultUpdateWhenViewChanges
            };
        }

        return config;
    }
}
