using System;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Models;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewValley;

namespace Pathoschild.LookupAnything
{
    internal class SearchResult
    {
        public SearchResult(NPC npc, Metadata metadata)
        {
            this.DisplayName = npc.getName();
            this.Name = this.DisplayName.ToLowerInvariant();
            if (npc.GetType().Namespace.Contains("Monster"))
            {
                this.TargetType = TargetType.Monster;
            }
            else
            {
                this.TargetType = TargetType.Villager;
            }
            this.Subject = new Lazy<ISubject>(() => new CharacterSubject(npc, this.TargetType, metadata));
        }

        public SearchResult(ObjectModel objectModel)
        {
            this.DisplayName = objectModel.Name;
            this.Name = this.DisplayName.ToLowerInvariant();

            this.TargetType = TargetType.Object;
            var lazyItem = new Lazy<Item>((() => GameHelper.GetObjectBySpriteIndex(objectModel.ParentSpriteIndex)));
            this.Subject = new Lazy<ISubject>(() => new ItemSubject(lazyItem.Value, ObjectContext.World, knownQuality: false));
        }

        public SearchResult(FarmAnimal farmAnimal)
        {
            this.DisplayName = farmAnimal.name;
            this.Name = this.DisplayName.ToLowerInvariant();
            this.TargetType = TargetType.FarmAnimal;
            this.Subject = new Lazy<ISubject>(() => new FarmAnimalSubject(farmAnimal));
        }

        public SearchResult(RecipeModel results)
        {
            this.DisplayName = results.Name;
            this.Name = this.DisplayName.ToLowerInvariant();
            this.TargetType = TargetType.InventoryItem;
            this.Subject = new Lazy<ISubject>(() => new ItemSubject(results.CreateItem(), ObjectContext.Inventory, false));
        }

        public string DisplayName { get; }

        public string Name { get; }

        public TargetType TargetType { get; }

        public Lazy<ISubject> Subject { get; }
    }
}