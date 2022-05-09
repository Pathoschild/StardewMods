namespace Pathoschild.Stardew.LookupAnything.Framework.Data
{
    /// <summary>Provides override metadata about a game NPC.</summary>
    /// <param name="ID">The NPC identifier, like "Horse" (any NPCs of type Horse) or "Villager::Gunther" (any NPCs of type Villager with the name "Gunther").</param>
    /// <param name="DescriptionKey">The translation key which should override the NPC description (if any).</param>
    internal record CharacterData(string ID, string? DescriptionKey);
}
