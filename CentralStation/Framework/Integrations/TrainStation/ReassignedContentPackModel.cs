using System.Collections.Generic;

namespace Pathoschild.Stardew.CentralStation.Framework.Integrations.TrainStation;

/// <summary>The data model for a Train Station content pack reassigned to Central Station.</summary>
internal class ReassignedContentPackModel
{
    /*********
    ** Accessors
    *********/
    /// <summary>The train stops to register.</summary>
    public List<ReassignedContentPackStopModel?>? TrainStops { get; set; }

    /// <summary>The boat stops to register.</summary>
    public List<ReassignedContentPackStopModel?>? BoatStops { get; set; }
}
