using StardewValley;

namespace Pathoschild.Stardew.CentralStation.Framework.Constants;

/// <summary>The general constants defined for Central Station.</summary>
internal class Constant
{
    /****
    ** Main values
    ****/
    /// <summary>The unique mod ID for Central Station.</summary>
    public const string ModId = "Pathoschild.CentralStation";

    /// <summary>The unique ID for the Central Station location.</summary>
    public const string CentralStationLocationId = $"{Constant.ModId}_CentralStation";

    /// <summary>The map property name which adds a ticket machine automatically to a map.</summary>
    public const string TicketMachineMapProperty = $"{Constant.ModId}_TicketMachine";

    /// <summary>The map property name which defines the tourist areas in Central Station.</summary>
    public const string TouristAreasMapProperty = $"{Constant.ModId}_TouristAreas";

    /// <summary>The map property which performs an internal sub-action identified by a <see cref="MapSubActions"/> value.</summary>
    public const string InternalAction = Constant.ModId;

    /// <summary>The map property which opens a destination menu.</summary>
    public const string TicketsAction = "CentralStation";

    /// <summary>The key in <see cref="Game1.stats"/> for the number of times a player has visited the Central Station.</summary>
    public const string TimesVisitedStatKey = $"{Constant.ModId}_TimesVisited";

    /// <summary>The probability that a tourist will spawn on a given spawn tile, as a value between 0 (never) and 1 (always).</summary>
    public const float TouristSpawnChance = 0.35f;


    /****
    ** Strange occurrences
    ****/
    /// <summary>The minimum Central Station visits for the player to get a free item and strange message from the cola machine.</summary>
    public const int StrangeColaMachineMinVisits = 1;

    /// <summary>The chance of getting a free item and strange message from the cola machine, as a value between 0 (never) and 1 (always).</summary>
    public const float StrangeColaMachineChance = 0.05f;

    /// <summary>The minimum Central Station visits for the player to hear strange sounds at the exit door.</summary>
    public const int StrangeSoundsMinVisits = 1;

    /// <summary>The chance of hearing strange sounds at the exit door, as a value between 0 (never) and 1 (always).</summary>
    public const float StrangeSoundsChance = 0.05f;

    /// <summary>The minimum Central Station visits for the player to see a strange interaction message.</summary>
    public const int StrangeMessageMinVisits = 5;

    /// <summary>The chance of the player seeing a strange interaction message, as a value between 0 (never) and 1 (always).</summary>
    public const float StrangeMessageChance = 0.05f;

    /// <summary>The minimum Central Station visits for the player to see the station being dark and closed.</summary>
    public const int DarkStationMinVisits = 15;

    /// <summary>The minimum time of day for the player to see the station being dark and closed.</summary>
    public const int DarkStationMinTime = 2400;

    /// <summary>The chance of the central station being dark and closed, as a value between 0 (never) and 1 (always).</summary>
    public const float DarkStationChance = 0.005f;
}
