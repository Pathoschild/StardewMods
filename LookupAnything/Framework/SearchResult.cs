using System;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Models;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.Monsters;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>Data about a subject which can be searched dynamically.</summary>
    internal class SearchResult
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The subject's display name.</summary>
        public string DisplayName { get; }

        /// <summary>The internal search name.</summary>
        public string Name { get; }

        /// <summary>The subject type.</summary>
        public TargetType TargetType { get; }

        /// <summary>The subject data.</summary>
        public Lazy<ISubject> Subject { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="npc">The subject.</param>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public SearchResult(NPC npc, Metadata metadata)
        {
            this.DisplayName = npc.getName();
            this.Name = this.DisplayName.ToLowerInvariant();
            this.TargetType = npc is Monster
                ? TargetType.Monster
                : TargetType.Villager;
            this.Subject = new Lazy<ISubject>(() => new CharacterSubject(npc, this.TargetType, metadata));
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="objectModel">The subject.</param>
        public SearchResult(ObjectModel objectModel)
        {
            this.DisplayName = objectModel.Name;
            this.Name = this.DisplayName.ToLowerInvariant();
            this.TargetType = TargetType.Object;
            this.Subject = new Lazy<ISubject>(() => new ItemSubject(GameHelper.GetObjectBySpriteIndex(objectModel.ParentSpriteIndex), ObjectContext.World, knownQuality: false));
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="animal">The subject.</param>
        public SearchResult(FarmAnimal animal)
        {
            this.DisplayName = animal.name;
            this.Name = this.DisplayName.ToLowerInvariant();
            this.TargetType = TargetType.FarmAnimal;
            this.Subject = new Lazy<ISubject>(() => new FarmAnimalSubject(animal));
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="recipe">The subject.</param>
        public SearchResult(RecipeModel recipe)
        {
            this.DisplayName = recipe.Name;
            this.Name = this.DisplayName.ToLowerInvariant();
            this.TargetType = TargetType.InventoryItem;
            this.Subject = new Lazy<ISubject>(() => new ItemSubject(recipe.CreateItem(), ObjectContext.Inventory, false));
        }
    }
}