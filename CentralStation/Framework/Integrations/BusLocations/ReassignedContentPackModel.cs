namespace Pathoschild.Stardew.CentralStation.Framework.Integrations.BusLocations;

/// <summary>The data model for a Bus Locations content pack reassigned to Central Station.</summary>
internal class ReassignedContentPackModel
{
    /*********
    ** Accessors
    *********/
    /// <inheritdoc cref="Stop.ToLocation" />
    public string? MapName { get; set; }

    /// <inheritdoc cref="Stop.DisplayName" />
    public string? DisplayName { get; set; }

    /// <summary>The X position of the <see cref="Stop.ToTile" />.</summary>
    public int DestinationX { get; set; }

    /// <summary>The Y position of the <see cref="Stop.ToTile" />.</summary>
    public int DestinationY { get; set; }

    /// <inheritdoc cref="Stop.ToFacingDirection" />
    public int ArrivalFacing { get; set; }

    /// <inheritdoc cref="Stop.Cost" />
    public int TicketPrice { get; set; }
}
