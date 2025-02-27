using System.Collections.Generic;

namespace Pathoschild.Stardew.CentralStation.Framework.ContentModels;

/// <summary>The data for a random tourist that can be spawned in the Central Station map.</summary>
internal class TouristModel
{
    /// <summary>The tourist's position within the map file, counting left-to-right from zero.</summary>
    public int Index { get; set; }

    /// <summary>The dialogues spoken by the tourist, if any.</summary>
    public List<string?>? Dialogue { get; set; }

    /// <summary>Once the player has seen all the dialogue lines in <see cref="Dialogue"/>, whether clicking the tourist again will restart from the first dialogue (<c>true</c>) or do nothing (<c>false</c>).</summary>
    public bool DialogueRepeats { get; set; }

    /// <summary>If set, a game state query which indicates whether this tourist can appear in the Central Station today.</summary>
    public string? Condition { get; set; }

    /// <summary>The areas within the central station where the tourist may appear.</summary>
    public HashSet<string?>? OnlyInAreas { get; set; }
}
