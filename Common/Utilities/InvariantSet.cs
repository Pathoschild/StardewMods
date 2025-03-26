using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pathoschild.Stardew.Common.Utilities;

/// <summary>A hash set optimized for immutable, case-insensitive, ordered string lookups.</summary>
internal class InvariantSet : IInvariantSet
{
    /*********
    ** Fields
    *********/
    /// <summary>A singleton instance of an empty underlying set.</summary>
    private static readonly HashSet<string> EmptyHashSet = [];

    /// <summary>The underlying hash set.</summary>
    private readonly HashSet<string> Set;


    /*********
    ** Accessors
    *********/
    /// <summary>A singleton instance for an empty set.</summary>
    public static IInvariantSet Empty { get; } = new InvariantSet();

    /// <inheritdoc />
    public int Count => this.Set.Count;


    /*********
    ** Public methods
    *********/
    /****
    ** Constructors
    ****/
    /// <summary>Construct an instance.</summary>
    public InvariantSet()
    {
        this.Set = InvariantSet.EmptyHashSet;
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="values">The values to add.</param>
    public InvariantSet(IEnumerable<string> values)
    {
        this.Set = values switch
        {
            InvariantSet set => set.Set,
            HashSet<string> set => set.ToNonNullCaseInsensitive(),
            ICollection<string> { Count: 0 } => InvariantSet.EmptyHashSet,
            _ => this.CreateSet(values)
        };
    }

    /// <summary>Construct an instance.</summary>
    /// <param name="values">The single value to add.</param>
    public InvariantSet(params string[] values)
    {
        this.Set = this.CreateSet(values);
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<string> GetEnumerator()
    {
        return this.Set.GetEnumerator();
    }

    /// <inheritdoc />
    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.Set.GetEnumerator();
    }

    /// <inheritdoc />
    [Pure]
    public bool Contains(string item)
    {
        return this.Set.Contains(item);
    }

    /// <inheritdoc />
    [Pure]
    public bool IsProperSubsetOf(IEnumerable<string> other)
    {
        return this.Set.IsProperSubsetOf(other);
    }

    /// <inheritdoc />
    [Pure]
    public bool IsProperSupersetOf(IEnumerable<string> other)
    {
        return this.Set.IsProperSupersetOf(other);
    }

    /// <inheritdoc />
    [Pure]
    public bool IsSubsetOf(IEnumerable<string> other)
    {
        return this.Set.IsSubsetOf(other);
    }

    /// <inheritdoc />
    [Pure]
    public bool IsSupersetOf(IEnumerable<string> other)
    {
        return this.Set.IsSupersetOf(other);
    }

    /// <inheritdoc />
    [Pure]
    public bool Overlaps(IEnumerable<string> other)
    {
        return this.Set.Overlaps(other);
    }

    /// <inheritdoc />
    [Pure]
    public bool SetEquals(IEnumerable<string> other)
    {
        return this.Set.SetEquals(other);
    }

    /// <inheritdoc />
    [Pure]
    public IInvariantSet GetWith(string other)
    {
        if (this.Count == 0)
            return new InvariantSet(other);

        if (this.Contains(other))
            return this;

        HashSet<string> set = this.CreateSet(this.Set);
        set.Add(other);
        return new InvariantSet(set);
    }

    /// <inheritdoc cref="IInvariantSet" />
    public IInvariantSet GetWith(ICollection<string> other)
    {
        if (other.Count == 0)
            return this;

        if (this.Count == 0)
            return new InvariantSet(this.CreateSet(other));

        bool changed = false;
        HashSet<string> set = this.CreateSet(this.Set);
        foreach (string value in other)
            changed |= set.Add(value);
        return changed
            ? new InvariantSet(set)
            : this;
    }

    /// <inheritdoc cref="IInvariantSet" />
    public IInvariantSet GetWithout(string other)
    {
        if (!this.Contains(other))
            return this;

        HashSet<string> copy = this.CreateSet(this);
        copy.Remove(other);
        return new InvariantSet(copy);
    }

    /// <inheritdoc cref="IInvariantSet" />
    public IInvariantSet GetWithout(IEnumerable<string> other)
    {
        HashSet<string>? copy = null;

        foreach (string value in other)
        {
            if (copy is null)
            {
                if (!this.Contains(value))
                    continue; // avoid copying set if no items are removed

                copy = this.CreateSet(this);
            }

            copy.Remove(value);
        }

        return copy is not null
            ? new InvariantSet(copy)
            : this;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Create an underlying mutable hash set.</summary>
    /// <param name="values">The values to add to the hash set.</param>
    private HashSet<string> CreateSet(IEnumerable<string> values)
    {
        return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
    }
}
