namespace Pathoschild.Stardew.CentralStation.Framework;

/// <summary>The mod configuration.</summary>
internal class ModConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>Whether Pam's bus must be repaired before the ticket machine is available at the vanilla bus stop.</summary>
    public bool RequirePamBus { get; set; }

    /// <summary>Whether Pam must be ready at the bus stop to take the bus from that location.</summary>
    public bool RequirePam { get; set; }
}
