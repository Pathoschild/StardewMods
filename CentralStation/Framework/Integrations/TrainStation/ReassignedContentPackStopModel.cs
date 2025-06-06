using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.CentralStation.Framework.Integrations.TrainStation;

/// <summary>A boat or train stop in a <see cref="ReassignedContentPackModel"/> data model.</summary>
internal class ReassignedContentPackStopModel
{
    /*********
    ** Accessors
    ********/
    /// <summary>The display name translations for each language.</summary>
    public Dictionary<string, string?>? LocalizedDisplayName { get; set; }

    /// <summary>The internal name of the location to which the player should warp when they select this stop.</summary>
    public string? TargetMapName { get; set; }

    /// <summary>The tile X position to which the player should warp when they select this stop.</summary>
    public int TargetX { get; set; }

    /// <summary>The tile Y position to which the player should warp when they select this stop.</summary>
    public int TargetY { get; set; }

    /// <summary>The gold price to go to that stop.</summary>
    public int Cost { get; set; } = 0;

    /// <summary>The direction the player should be facing after they warp, matching a constant like <see cref="Game1.down"/>.</summary>
    public int FacingDirectionAfterWarp { get; set; } = Game1.down;

    /// <summary>If set, the Expanded Precondition Utility conditions which indicate whether this stop should appear in the menu at a given time.</summary>
    public string?[]? Conditions { get; set; }


    /*********
    ** Public methods
    ********/
    /// <summary>Get the localized display name.</summary>
    /// <remarks>Derived from <c>StopModel.GetDisplayName()</c> in the Train Station code.</remarks>
    public string GetDisplayName()
    {
        return
            this.LocalizedDisplayName?.GetValueOrDefault(LocalizedContentManager.CurrentLanguageCode.ToString())
            ?? this.LocalizedDisplayName?.GetValueOrDefault("en")
            ?? "No translation";
    }
}
