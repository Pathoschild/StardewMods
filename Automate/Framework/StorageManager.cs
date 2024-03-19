using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Storage;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Manages access to items in the underlying containers.</summary>
    internal class StorageManager : IStorage
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public IContainer[] InputContainers { get; private set; }

        /// <inheritdoc />
        public IContainer[] OutputContainers { get; private set; }


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
        [MemberNotNull(nameof(StorageManager.InputContainers), nameof(StorageManager.OutputContainers))]
        public void SetContainers(IEnumerable<IContainer> containers)
        {
            ICollection<IContainer> containerCollection = containers as ICollection<IContainer> ?? containers.ToArray();

            this.InputContainers = containerCollection
                .Where(p => p.StorageAllowed())
                .OrderBy(p => p.IsJunimoChest) // push items into Junimo chests last
                .ThenByDescending(p => p.StoragePreferred())
                .ToArray();

            this.OutputContainers = containerCollection
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
                foreach (ITrackedStack stack in container)
                {
                    if (stack.Count > 0)
                        yield return stack;
                }
            }
        }

        /****
        ** TryGetIngredient
        ****/
        /// <inheritdoc />
        public bool TryGetIngredient(Func<ITrackedStack, bool> predicate, int count, [NotNullWhen(true)] out IConsumable? consumable)
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
        public bool TryGetIngredient(IRecipe[] recipes, [NotNullWhen(true)] out IConsumable? consumable, [NotNullWhen(true)] out IRecipe? recipe)
        {
            IDictionary<IRecipe, StackAccumulator> accumulator = recipes.ToDictionary(req => req, _ => new StackAccumulator());

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
            if (this.TryGetIngredient(predicate, count, out IConsumable? requirement))
            {
                requirement.Reduce();
                return true;
            }
            return false;
        }

        /****
        ** TryPush
        ****/
        /// <inheritdoc />
        public bool TryPush(ITrackedStack? item)
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
            string itemKey = item.Sample.QualifiedItemId;
            foreach (IContainer container in otherContainers)
            {
                if (container.All(p => p.Sample.QualifiedItemId != itemKey))
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
    }
}
