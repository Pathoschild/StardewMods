using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Storage;
using Pathoschild.Stardew.Common;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Manages access to items in the underlying containers.</summary>
    internal class StorageManager : IStorage
    {
        /*********
        ** Fields
        *********/
        /// <summary>The storage containers that accept input, in priority order.</summary>
        private IContainer[] InputContainers;

        /// <summary>The storage containers that provide items, in priority order.</summary>
        private IContainer[] OutputContainers;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="containers">The storage containers.</param>
        public StorageManager(IEnumerable<IContainer> containers)
        {
            this.SetContainers(containers);
        }

        /// <summary>Set the containers to use.</summary>
        /// <param name="containers">The storage containers.</param>
        public void SetContainers(IEnumerable<IContainer> containers)
        {
            containers = containers.ToArray();

            this.InputContainers = containers
                .Where(p => p.StorageAllowed())
                .OrderBy(p => p.IsJunimoChest) // push items into Junimo chests last
                .ThenByDescending(p => p.StoragePreferred())
                .ToArray();

            this.OutputContainers = containers
                .Where(p => p.TakingItemsAllowed())
                .OrderByDescending(p => p.IsJunimoChest) // take items from Junimo chests first
                .ThenByDescending(p => p.TakingItemsPreferred())
                .ToArray();
        }


        /****
        ** GetItems
        ****/
        /// <inheritdoc />
        public IEnumerable<ITrackedStack> GetItems()
        {
            foreach (IContainer container in this.OutputContainers)
            {
                bool preventRemovingStacks = container.ModData.ReadField(AutomateContainerHelper.PreventRemovingStacksKey, bool.Parse);

                foreach (ITrackedStack stack in container)
                {
                    if (preventRemovingStacks)
                        stack.PreventEmptyStacks();

                    if (stack.Count > 0)
                        yield return stack;
                }
            }
        }

        /****
        ** TryGetIngredient
        ****/
        /// <inheritdoc />
        public bool TryGetIngredient(Func<ITrackedStack, bool> predicate, int count, out IConsumable consumable)
        {
            StackAccumulator stacks = new StackAccumulator();
            foreach (ITrackedStack input in this.GetItems().Where(predicate))
            {
                TrackedItemCollection stack = stacks.Add(input);
                if (stack.Count >= count)
                {
                    consumable = new Consumable(stack, count);
                    return consumable.IsMet;
                }
            }

            consumable = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetIngredient(int id, int count, out IConsumable consumable, ItemType? type = ItemType.Object)
        {
            return this.TryGetIngredient(item => (type == null || item.Type == type) && (item.Sample.ParentSheetIndex == id || item.Sample.Category == id), count, out consumable);
        }

        /// <inheritdoc />
        public bool TryGetIngredient(IRecipe[] recipes, out IConsumable consumable, out IRecipe recipe)
        {
            IDictionary<IRecipe, StackAccumulator> accumulator = recipes.ToDictionary(req => req, req => new StackAccumulator());

            foreach (ITrackedStack input in this.GetItems())
            {
                foreach (var entry in accumulator)
                {
                    recipe = entry.Key;
                    StackAccumulator stacks = entry.Value;

                    if (recipe.AcceptsInput(input))
                    {
                        ITrackedStack stack = stacks.Add(input);
                        if (stack.Count >= recipe.InputCount)
                        {
                            consumable = new Consumable(stack, entry.Key.InputCount);
                            return true;
                        }
                    }
                }
            }

            consumable = null;
            recipe = null;
            return false;
        }

        /****
        ** TryConsume
        ****/
        /// <inheritdoc />
        public bool TryConsume(Func<ITrackedStack, bool> predicate, int count)
        {
            if (this.TryGetIngredient(predicate, count, out IConsumable requirement))
            {
                requirement.Reduce();
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public bool TryConsume(int itemID, int count, ItemType? type = ItemType.Object)
        {
            return this.TryConsume(item => (type == null || item.Type == type) && item.Sample.ParentSheetIndex == itemID, count);
        }

        /****
        ** TryPush
        ****/
        /// <inheritdoc />
        public bool TryPush(ITrackedStack item)
        {
            if (item == null || item.Count <= 0)
                return false;

            int originalCount = item.Count;

            IContainer[] preferredContainers = this.InputContainers.TakeWhile(p => p.StoragePreferred()).ToArray();
            IContainer[] otherContainers = this.InputContainers.Skip(preferredContainers.Length).ToArray();

            // push into 'output' chests
            foreach (IContainer container in preferredContainers)
            {
                container.Store(item);
                if (item.Count <= 0)
                    return true;
            }

            // push into chests that already have this item
            string itemKey = this.GetItemKey(item.Sample);
            foreach (IContainer container in otherContainers)
            {
                if (container.All(p => this.GetItemKey(p.Sample) != itemKey))
                    continue;

                container.Store(item);
                if (item.Count <= 0)
                    return true;
            }

            // push into first available chest
            if (item.Count >= 0)
            {
                foreach (IContainer container in otherContainers)
                {
                    container.Store(item);
                    if (item.Count <= 0)
                        return true;
                }
            }

            return item.Count < originalCount;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a key which uniquely identifies an item type.</summary>
        /// <param name="item">The item to identify.</param>
        private string GetItemKey(Item item)
        {
            string key = item.GetType().FullName;
            if (item is SObject obj)
                key += "_craftable:" + obj.bigCraftable.Value;
            key += "_id:" + item.ParentSheetIndex;

            return key;
        }
    }
}
