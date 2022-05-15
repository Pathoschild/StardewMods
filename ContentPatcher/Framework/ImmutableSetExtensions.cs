using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace ContentPatcher.Framework
{
    /// <summary>Provides extension methods for <see cref="IImmutableSet{T}"/> instances.</summary>
    internal static class ImmutableSetExtensions
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get a new immutable set with the given values added.</summary>
        /// <param name="set">The original set to copy if needed.</param>
        /// <param name="values">The values for which to get a set.</param>
        [Pure]
        public static IImmutableSet<string> AddMany(this IImmutableSet<string> set, IEnumerable<string> values)
        {
            if (set.Count == 0)
                return ImmutableSets.From(values);

            if (values is IList<string> list)
            {
                switch (list.Count)
                {
                    case 0:
                        return set;

                    case 1:
                        return set.Add(list[0]);
                }
            }

            return set.Union(values);
        }
    }
}
