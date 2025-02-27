using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Pathoschild.Stardew.Common;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>Configures the settings for a data layer.</summary>
internal class LayerConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>The default value for <see cref="UpdatesPerSecond"/>.</summary>
    public const decimal DefaultUpdatesPerSecond = 60;

    /// <summary>Whether to enable this data layer.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The default number of updates needed per second.</summary>
    public decimal UpdatesPerSecond { get; set; } = LayerConfig.DefaultUpdatesPerSecond;

    /// <summary>Whether to update the layer when the player's tile view changes.</summary>
    public bool UpdateWhenViewChange { get; set; } = true;

    /// <summary>The key binding which switches to this layer when the overlay is open.</summary>
    public KeybindList ShortcutKey { get; set; } = new();


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
        this.ShortcutKey ??= new();
    }

    /// <summary>Whether the data layer should be enabled.</summary>
    public bool IsEnabled()
    {
        return this.Enabled && this.UpdatesPerSecond > 0;
    }
}
