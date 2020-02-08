using System;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.Monsters;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Data about a subject which can be searched dynamically.</summary>
    internal class SearchResult
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The subject's display name.</summary>
        public string DisplayName { get; }

        /// <summary>The subject type.</summary>
        public SubjectType Type { get; }

        /// <summary>The subject data.</summary>
        public Lazy<ISubject> Subject { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="npc">The subject.</param>
        public SearchResult(SubjectFactory codex, NPC npc)
        {
            this.DisplayName = npc.getName();
            this.Type = npc is Monster
                ? SubjectType.Monster
                : SubjectType.Villager;
            this.Subject = new Lazy<ISubject>(() => codex.GetCharacter(npc, this.Type));
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="objectModel">The subject.</param>
        public SearchResult(SubjectFactory codex, GameHelper gameHelper, ObjectModel objectModel)
        {
            this.DisplayName = objectModel.Name;
            this.Type = SubjectType.Object;
            this.Subject = new Lazy<ISubject>(() => codex.GetItem(gameHelper.GetObjectBySpriteIndex(objectModel.ParentSpriteIndex), ObjectContext.World, knownQuality: false));
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="animal">The subject.</param>
        public SearchResult(SubjectFactory codex, FarmAnimal animal)
        {
            this.DisplayName = animal.Name;
            this.Type = SubjectType.FarmAnimal;
            this.Subject = new Lazy<ISubject>(() => codex.GetFarmAnimal(animal));
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="recipe">The subject.</param>
        public SearchResult(SubjectFactory codex, GameHelper gameHelper, RecipeModel recipe)
        {
            if (recipe.Type != RecipeType.Cooking && recipe.Type != RecipeType.Crafting)
                throw new InvalidOperationException("Unsupported recipe type.");

            var item = gameHelper.GetObjectBySpriteIndex(recipe.OutputItemIndex.Value);

            this.DisplayName = item.Name;
            this.Type = SubjectType.InventoryItem;
            this.Subject = new Lazy<ISubject>(() => codex.GetItem(item, ObjectContext.Inventory, false));
        }
    }
}
