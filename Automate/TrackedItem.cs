using System;
using System.Diagnostics.CodeAnalysis;
using StardewValley;

namespace Pathoschild.Stardew.Automate;

/// <summary>An item stack which notifies callbacks when it's reduced.</summary>
public class TrackedItem : ITrackedStack
{
    /*********
    ** Fields
    *********/
    /// <summary>The item stack.</summary>
    private readonly Item Item;

    /// <summary>The callback invoked when the stack size is reduced (including reduced to zero).</summary>
    protected Action<TrackedItem, Item>? OnReducedImpl;

    /// <summary>The callback invoked when the stack is empty.</summary>
    protected Action<TrackedItem, Item>? OnEmptyImpl;


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public Item Sample { get; }

    /// <inheritdoc />
    public string Type { get; }

    /// <inheritdoc />
    public int Count { get; private set; }

    /// <summary>The <see cref="Count"/> for which <see cref="OnReducedImpl"/> was last called (or the initial value if this is the first call).</summary>
    public int LastCount { get; private set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="item">The item stack.</param>
    public TrackedItem(Item item)
    {
        this.Item = item ?? throw new InvalidOperationException("Can't track a null item stack.");
        this.Type = item.TypeDefinitionId;
        this.Sample = this.GetNewStack(item);
        this.Count = item.Stack; // we can't trust Item.Stack to reduce correctly (e.g. Hat.Stack always return 1), so we need to track it ourselves
        this.LastCount = this.Count;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="item">The item stack.</param>
    /// <param name="onReduced">The callback to raise when the stack size is reduced (including reduced to zero).</param>
    /// <param name="onEmpty">The callback invoked when the stack is empty.</param>
    [Obsolete($"Use {nameof(OnReduced)} or {nameof(OnEmpty)} instead of passing callbacks through the constructor.")]
    public TrackedItem(Item item, Action<Item>? onReduced = null, Action<Item>? onEmpty = null)
        : this(item)
    {
        if (onReduced != null)
            this.OnReduced((_, reducedItem) => onReduced(reducedItem));

        if (onEmpty != null)
            this.OnEmpty((_, reducedItem) => onEmpty(reducedItem));
    }

    /// <summary>Register a callback to raise when the stack size is reduced (including reduced to zero).</summary>
    /// <param name="onReduced">The callback to invoke.</param>
    /// <returns>Returns the current instance for chaining.</returns>
    public TrackedItem OnReduced(Action<TrackedItem, Item>? onReduced)
    {
        this.OnReducedImpl += onReduced;

        return this;
    }

    /// <summary>Register a callback to raise when the stack size is reduced to zero.</summary>
    /// <param name="onEmpty">The callback to invoke.</param>
    /// <returns>Returns the current instance for chaining.</returns>
    public TrackedItem OnEmpty(Action<TrackedItem, Item>? onEmpty)
    {
        this.OnEmptyImpl += onEmpty;

        return this;
    }

    /// <inheritdoc />
    public void Reduce(int count)
    {
        if (count <= 0)
            return;

        this.Item.Stack -= count;
        this.Count -= count;

        this.Delegate();
    }

    /// <inheritdoc />
    public Item? Take(int count)
    {
        if (count <= 0)
            return null;

        this.Reduce(count);
        return this.GetNewStack(this.Item, count);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Notify handlers.</summary>
    private void Delegate()
    {
        // skip if not reduced
        if (this.Count >= this.LastCount)
            return;

        // notify handlers
        this.OnReducedImpl?.Invoke(this, this.Item);
        if (this.Count <= 0)
            this.OnEmptyImpl?.Invoke(this, this.Item);

        this.LastCount = this.Count;
    }

    /// <summary>Create a new stack of the given item.</summary>
    /// <param name="original">The item stack to clone.</param>
    /// <param name="stackSize">The new stack size.</param>
    [return: NotNullIfNotNull("original")]
    private Item? GetNewStack(Item? original, int stackSize = 1)
    {
        if (original == null)
            return null;

        Item stack = original.getOne();
        stack.Stack = stackSize;
        return stack;
    }
}
