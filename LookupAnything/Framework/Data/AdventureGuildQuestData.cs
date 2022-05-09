namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Information about an Adventure Guild monster-slaying quest.</summary>
    /// <param name="KillListKey">The suffix for this monster in the <c>Strings\Locations:AdventureGuild_KillList_</c> translations.</param>
    /// <param name="Targets">The names of the monsters in this category.</param>
    /// <param name="RequiredKills">The number of kills required for the reward.</param>
    internal record AdventureGuildQuestData(string KillListKey, string[] Targets, int RequiredKills);
}
