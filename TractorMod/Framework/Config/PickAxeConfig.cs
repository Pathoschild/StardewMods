using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.TractorMod.Framework.Config;

/// <summary>Configuration for the pickaxe attachment.</summary>
internal class PickAxeConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether to clear dead crops.</summary>
    public bool ClearDeadCrops { get; set; } = true;

    /// <summary>Whether to clear tilled dirt which doesn't have any fertilizer.</summary>
    public bool ClearDirt { get; set; } = true;

    /// <summary>Whether to clear tilled dirt which has fertilizer.</summary>
    public bool ClearDirtWithFertilizer { get; set; } = false;

    /// <summary>Whether to clear placed flooring.</summary>
    public bool ClearFlooring { get; set; } = false;

    /// <summary>Whether to clear boulders and meteorites.</summary>
    public bool ClearBouldersAndMeteorites { get; set; } = true;

    /// <summary>Whether to break mine stones.</summary>
    public bool BreakMineStones { get; set; } = true;

    /// <summary>Whether to break containers in the mine.</summary>
    public bool BreakMineContainers { get; set; } = true;

    /// <summary>Whether to clear placed objects.</summary>
    public bool ClearObjects { get; set; } = false;

    /// <summary>Whether to clear weeds.</summary>
    public bool ClearWeeds { get; set; } = true;

    /// <summary>Whether to harvest spawned items in the mines.</summary>
    public bool HarvestMineSpawns { get; set; } = true;

    /// <summary>The extra fields which don't match one of the other fields.</summary>
#pragma warning disable CS0649 // populated by Json.NET when there are extra fields
    [JsonExtensionData]
    private IDictionary<string, JToken>? AdditionalFields;
#pragma warning restore CS0649


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
        if (this.AdditionalFields != null)
        {
            // ClearDebris renamed to BreakMineStones in 4.24.4
            if (this.AdditionalFields.TryGetValue("ClearDebris", out JToken? rawValue))
            {
                this.BreakMineStones = rawValue.Value<bool>();
                this.AdditionalFields.Remove("ClearDebris");
            }
        }
    }
}
