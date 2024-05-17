using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Provides metadata that's not available from the game data directly (e.g. because it's buried in the logic).</summary>
    /// <param name="Constants">Constant values hardcoded by the game.</param>
    /// <param name="Items">Metadata for game objects (including inventory items, terrain features, crops, trees, and other map objects).</param>
    /// <param name="Characters">Metadata for NPCs in the game.</param>
    /// <param name="BuildingRecipes">The building recipes. Derived from <see cref="StardewValley.Buildings.Mill.dayUpdate"/>.</param>
    /// <param name="Shops">The shops that buy items from the player. Derived from <see cref="StardewValley.Menus.ShopMenu"/> constructor.</param>
    /// <param name="CustomFishSpawnRules">Added fish spawn rules.</param>
    /// <param name="IgnoreFishingLocations">The fishing location names to hide in the UI (e.g. because they're inaccessible in-game).</param>
    /// <param name="PuzzleSolutions">The solutions for hardcoded in-game puzzles.</param>
    internal record Metadata(
        ConstantData Constants,
        ItemData[] Items,
        CharacterData[] Characters,
        BuildingRecipeData[] BuildingRecipes,
        ShopData[] Shops,
        Dictionary<string, FishSpawnData> CustomFishSpawnRules,
        HashSet<string> IgnoreFishingLocations,
        PuzzleSolutionsData PuzzleSolutions
    )
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the metadata seems to be basically valid.</summary>
        public bool LooksValid()
        {
            return new object?[] { this.Constants, this.Items, this.Characters, this.BuildingRecipes, this.Shops, this.CustomFishSpawnRules, this.IgnoreFishingLocations, this.PuzzleSolutions }
                .All(p => p != null);
        }

        /// <summary>Get overrides for a game object.</summary>
        /// <param name="item">The item for which to get overrides.</param>
        /// <param name="context">The context for which to get an override.</param>
        public ItemData? GetObject(Item item, ObjectContext context)
        {
            return this.Items
                .FirstOrDefault(p => p.QualifiedId.Contains(item.QualifiedItemId) && p.Context.HasFlag(context));
        }

        /// <summary>Get overrides for a game object.</summary>
        /// <param name="character">The character for which to get overrides.</param>
        /// <param name="type">The character type.</param>
        public CharacterData? GetCharacter(NPC character, SubjectType type)
        {
            return
                this.Characters?.FirstOrDefault(p => p.ID == $"{type}::{character.Name}") // override by type + name
                ?? this.Characters?.FirstOrDefault(p => p.ID == type.ToString()); // override by type
        }
    }
}
