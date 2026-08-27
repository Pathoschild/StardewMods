using System;
using StardewValley;

namespace Pathoschild.Stardew.Automate.Framework.Storage;

/// <summary>A wrapper for <see cref="ITrackedStack"/> which reserves a minimum quantity that can't be consumed.</summary>
internal class ReservedTrackedStack : ITrackedStack
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying tracked stack.</summary>
    private readonly ITrackedStack Stack;

    /// <summary>The number of items to leave in the underlying stack.</summary>
    private readonly int ReserveAmount;


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public Item Sample => this.Stack.Sample;

    /// <inheritdoc />
    public string Type => this.Stack.Type;

    /// <inheritdoc />
    public int Count => Math.Max(0, this.Stack.Count - this.ReserveAmount);


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="stack">The underlying tracked stack.</param>
    /// <param name="reserveAmount">The number of items to leave in the underlying stack.</param>
    public ReservedTrackedStack(ITrackedStack stack, int reserveAmount)
    {
        this.Stack = stack;
        this.ReserveAmount = Math.Max(0, reserveAmount);
    }

    /// <inheritdoc />
    public void Reduce(int count)
    {
        this.Stack.Reduce(Math.Min(count, this.Count));
    }

    /// <inheritdoc />
    public Item? Take(int count)
    {
        return this.Stack.Take(Math.Min(count, this.Count));
    }
}
