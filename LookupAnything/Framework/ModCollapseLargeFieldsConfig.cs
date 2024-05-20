namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>As part of <see cref="ModConfig"/>, the minimum field values needed before they're auto-collapsed.</summary>
    public class ModCollapseLargeFieldsConfig
    {
        /// <summary>Whether to collapse large fields.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>In a building lookup, the minimum recipes needed before the field is collapsed by default.</summary>
        public int BuildingRecipes { get; set; } = 25;

        /// <summary>In an item lookup, the minimum recipes needed before the field is collapsed by default.</summary>
        public int ItemRecipes { get; set; } = 25;

        /// <summary>In a character lookup, the minimum gift tastes needed before the field is collapsed by default.</summary>
        public int NpcGiftTastes { get; set; } = 75;
    }
}
