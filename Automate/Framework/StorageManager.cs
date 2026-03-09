using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Storage;

namespace Pathoschild.Stardew.Automate.Framework;

/// <summary>Manages access to items in the underlying containers.</summary>
internal class StorageManager : IStorage
{
    /*********
    ** Fields
    *********/
    /// <summary>The index of the first container in <see cref="InputContainers"/> which doesn't match <see cref="ContainerExtensions.StoragePreferred"/>.</summary>
    private int FirstNonPreferredInput;


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
            .OrderByDescending(p => p.StoragePreferred())
            .ThenBy(p => p.IsJunimoChest) // push items into Junimo chests last
            .ToArray();

        this.OutputContainers = containerCollection
            .Where(p => p.TakingItemsAllowed())
            .OrderByDescending(p => p.TakingItemsPreferred())
            .ThenByDescending(p => p.IsJunimoChest) // take items from Junimo chests first
            .ToArray();

        this.FirstNonPreferredInput = this.InputContainers.Length;
        for (int i = 0; i < this.InputContainers.Length; i++)
        {
            if (!this.InputContainers[i].StoragePreferred())
            {
                this.FirstNonPreferredInput = i;
                break;
            }
        }
    }


    /****
    ** GetItems
    ****/
    /// <inheritdoc />
    public bool HasLockedContainers()
    {
        foreach (IContainer container in this.InputContainers)
        {
            if (container.IsLocked)
                return true;
        }

        foreach (IContainer container in this.OutputContainers)
        {
            if (container.IsLocked)
                return true;
        }

        return false;
    }

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
        Dictionary<IRecipe, StackAccumulator> accumulator = recipes.ToDictionary(req => req, _ => new StackAccumulator());

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
        if (item is not { Count: > 0 })
            return false;

        // try chests marked "put items in this chest first"
        int fallbackStartAt = this.FirstNonPreferredInput;
        bool pushedToPreferred = false;
        if (fallbackStartAt > 0 && this.TryPushImpl(item, startAt: 0, endBefore: fallbackStartAt))
        {
            pushedToPreferred = true;
            if (item.Count < 1)
                return true;
        }

        // try remaining chests
        return this.TryPushImpl(item, startAt: fallbackStartAt) || pushedToPreferred;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Add item to a container within the given index range without checking the storage-preferred flag.</summary>
    /// <param name="item">The item stack to push.</param>
    /// <param name="startAt">The index in <see cref="InputContainers"/> at which to start pushing (inclusive).</param>
    /// <param name="endBefore">The index in <see cref="InputContainers"/> at which to stop pushing (exclusive), or <c>null</c> to continue to the end of the array.</param>
    /// <returns>Returns whether at least some of the item stack was received.</returns>
    private bool TryPushImpl(ITrackedStack item, int startAt, int? endBefore = null)
    {
        int originalCount = item.Count;
        IContainer[] containers = this.InputContainers;

        endBefore ??= containers.Length;
        if (startAt >= endBefore)
            return false;

        // add to chests which contain the item
        string qualifiedItemId = item.Sample.QualifiedItemId;
        int fallbackStartAt = startAt;
        for (int i = startAt; i < endBefore; i++)
        {
            IContainer container = containers[i];
            if (!container.Inventory.ContainsId(qualifiedItemId))
                continue;

            container.Store(item);
            if (item.Count < 1)
                return true;

            if (i == fallbackStartAt)
                fallbackStartAt++; // we can skip this one too since we just checked it
        }

        // else any chests with enough space
        for (int i = fallbackStartAt; i < endBefore; i++)
        {
            IContainer container = containers[i];

            container.Store(item);
            if (item.Count < 1)
                return true;
        }

        return item.Count < originalCount;
    }
}
