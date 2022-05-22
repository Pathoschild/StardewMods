using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pathoschild.Stardew.Common.Utilities
{
    /// <summary>A hash set optimized for immutable, case-insensitive, ordered string lookups.</summary>
    internal class MutableInvariantSet : ISet<string>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying hash set.</summary>
        private HashSet<string>? Set;

        /// <summary>A cached immutable copy of <see cref="Set"/>, if applicable.</summary>
        private IInvariantSet? CachedImmutableSet;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public int Count => this.Set?.Count ?? 0;

        /// <summary>Whether the set is locked and can't be further modified.</summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>Whether the set is empty.</summary>
        [MemberNotNullWhen(false, nameof(MutableInvariantSet.Set))]
        public bool IsEmpty => this.Set?.Count is null or 0;


        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        public MutableInvariantSet() { }

        /// <summary>Construct an instance for the given set.</summary>
        /// <param name="values">The sets with which to populate the set.</param>
        public MutableInvariantSet(IEnumerable<string> values)
        {
            this.Set = this.CreateSet(values);
        }


        /****
        ** Read methods
        ****/
        /// <inheritdoc />
        [Pure]
        public IEnumerator<string> GetEnumerator()
        {
            return this.Set?.GetEnumerator() ?? Enumerable.Empty<string>().GetEnumerator();
        }

        /// <inheritdoc />
        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Set?.GetEnumerator() ?? Enumerable.Empty<string>().GetEnumerator();
        }

        /// <inheritdoc />
        [Pure]
        public bool IsProperSubsetOf(IEnumerable<string> other)
        {
            if (this.IsEmpty && other is ICollection<string> list)
                return list.Count > 0; // empty set is a proper subset of any set except the empty set

            return this.EnsureSet().IsProperSubsetOf(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool IsProperSupersetOf(IEnumerable<string> other)
        {
            if (this.IsEmpty)
                return false; // empty set is never a proper superset

            return this.Set.IsProperSupersetOf(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool IsSubsetOf(IEnumerable<string> other)
        {
            if (this.IsEmpty)
                return true; // empty set is a subset of any other set

            return this.Set.IsSubsetOf(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool IsSupersetOf(IEnumerable<string> other)
        {
            if (this.IsEmpty && other is ICollection<string> list)
                return list.Count == 0; // empty set is only a proper subset of itself

            return this.EnsureSet().IsSupersetOf(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool Overlaps(IEnumerable<string> other)
        {
            if (this.IsEmpty)
                return false; // empty set never overlaps any other

            return this.Set.Overlaps(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool SetEquals(IEnumerable<string> other)
        {
            if (this.IsEmpty && other is ICollection<string> list)
                return list.Count == 0; // empty set only equals itself

            return this.EnsureSet().SetEquals(other);
        }

        /// <inheritdoc />
        [Pure]
        public bool Contains(string item)
        {
            if (this.IsEmpty)
                return false;

            return this.Set.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(string[] array, int arrayIndex)
        {
            if (this.IsEmpty)
                return;

            this.Set.CopyTo(array, arrayIndex);
        }


        /****
        ** Write methods
        ****/
        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        void ICollection<string>.Add(string item)
        {
            this.AssertNotLocked();

            if (this.EnsureSet().Add(item))
                this.ClearCache();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        public bool Add(string item)
        {
            this.AssertNotLocked();

            if (this.EnsureSet().Add(item))
            {
                this.ClearCache();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        public void Clear()
        {
            this.AssertNotLocked();

            if (this.IsEmpty)
                return;

            this.Set.Clear();
            this.ClearCache();
        }

        /// <inheritdoc />
        public bool Remove(string item)
        {
            this.AssertNotLocked();

            if (this.IsEmpty)
                return false;

            if (this.Set.Remove(item))
            {
                this.ClearCache();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        public void ExceptWith(IEnumerable<string> other)
        {
            this.AssertNotLocked();

            if (this.IsEmpty)
                return;

            int wasCount = this.Count;
            this.EnsureSet().ExceptWith(other);
            this.ClearCacheUnlessCount(wasCount);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        public void IntersectWith(IEnumerable<string> other)
        {
            this.AssertNotLocked();

            if (this.IsEmpty)
                return;

            int wasCount = this.Count;
            this.EnsureSet().IntersectWith(other);
            this.ClearCacheUnlessCount(wasCount);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        public void SymmetricExceptWith(IEnumerable<string> other)
        {
            this.AssertNotLocked();

            if (this.IsEmpty && other is ICollection<string> { Count: 0 })
                return; // no values would be changed

            this.EnsureSet().SymmetricExceptWith(other);
            this.ClearCache();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        public void UnionWith(IEnumerable<string> other)
        {
            this.AssertNotLocked();

            if (this.IsEmpty)
            {
                if (other is ICollection<string> { Count: 0 })
                    return;

                this.Set = this.CreateSet(other);
                this.ClearCacheUnlessCount(0);
                return;
            }

            int wasCount = this.Count;
            this.Set.UnionWith(other);
            this.ClearCacheUnlessCount(wasCount);
        }


        /****
        ** Builder methods
        ****/
        /// <summary>Lock the builder so no further changes are allowed, and return the immutable set.</summary>
        public IInvariantSet Lock()
        {
            this.IsReadOnly = true;

            return this.GetImmutable();
        }

        /// <summary>Get an immutable copy of the underlying set which won't be affected by further changes to the set.</summary>
        /// <remarks>This should only be used if further changes might be made to the immutable set; otherwise see <see cref="Lock"/>.</remarks>
        public IInvariantSet GetImmutable()
        {
            if (this.CachedImmutableSet is null)
            {
                // empty set
                if (this.Set?.Count is not > 0)
                    this.CachedImmutableSet = InvariantSet.Empty;

                // wrap underlying set
                else if (this.IsReadOnly)
                    this.CachedImmutableSet = new InvariantSet(this.Set);

                // create copy
                else
                {
                    HashSet<string> copy = new(this.Set);
                    return new InvariantSet(copy);
                }
            }

            return this.CachedImmutableSet;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Assert that the builder isn't locked and still allows changes.</summary>
        /// <exception cref="InvalidOperationException">The set is locked and doesn't allow further changes.</exception>
        private void AssertNotLocked()
        {
            if (this.IsReadOnly)
                throw new NotSupportedException("This set is locked and doesn't allow further changes.");
        }

        /// <summary>Get the underlying set, creating it if needed.</summary>
        [MemberNotNull(nameof(MutableInvariantSet.Set))]
        private HashSet<string> EnsureSet()
        {
            return this.Set ??= this.CreateSet();
        }

        /// <summary>Clear the cached immutable set.</summary>
        private void ClearCache()
        {
            this.CachedImmutableSet = null;
        }

        /// <summary>Clear the cached immutable set if the count doesn't match the given value.</summary>
        /// <param name="count">The expected number of items in the set.</param>
        private void ClearCacheUnlessCount(int count)
        {
            if (count != this.Count)
                this.CachedImmutableSet = null;
        }

        /// <summary>Create an underlying mutable hash set.</summary>
        private HashSet<string> CreateSet()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Create an underlying mutable hash set.</summary>
        /// <param name="values">The values to add to the hash set.</param>
        private HashSet<string> CreateSet(IEnumerable<string> values)
        {
            return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        }
    }
}
