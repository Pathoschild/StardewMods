using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.TractorMod.Framework;

/// <summary>Metadata for a stashed building.</summary>
internal class LegacySaveDataBuilding
{
    /*********
    ** Accessors
    *********/
    /// <summary>The tile location.</summary>
    public Vector2 Tile { get; }

    /// <summary>The associated tractor ID.</summary>
    public Guid TractorId { get; }

    /// <summary>The associated tractor's hat ID.</summary>
    public int? TractorHatId { get; }

    /// <summary>The building type.</summary>
    public string Type { get; }

    /// <summary>The number of days until construction ends.</summary>
    public int DaysOfConstructionLeft { get; }

    /// <summary>The name of the map containing the building.</summary>
    public string? Map { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="tile">The building type.</param>
    /// <param name="tractorId">The associated tractor ID.</param>
    /// <param name="tractorHatId">The associated tractor's hat ID.</param>
    /// <param name="type">The tile location.</param>
    /// <param name="map">The name of the map containing the building.</param>
    /// <param name="daysOfConstructionLeft">The number of days until construction ends.</param>
    public LegacySaveDataBuilding(Vector2 tile, Guid tractorId, int? tractorHatId, string type, string map, int daysOfConstructionLeft)
    {
        this.Tile = tile;
        this.TractorId = tractorId != Guid.Empty ? tractorId : Guid.NewGuid(); // assign ID for older data
        this.TractorHatId = tractorHatId;
        this.Type = type;
        this.Map = map;
        this.DaysOfConstructionLeft = daysOfConstructionLeft;
    }
}
