using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides extension methods for collection types.</summary>
    internal static class CollectionExtensions
    {
        /*********
        ** Public methods
        *********/
        /****
        ** Nullability
        ****/
        /// <summary>Get all non-null values from the collection.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="values">The values to filter.</param>
        /// <remarks>This avoids a limitation with nullable reference types where the values are still marked nullable after <c>Where(p => p != null)</c>.</remarks>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> values)
            where T : class
        {
            return values.Where(p => p != null)!;
        }

        /****
        ** Case insensitivity
        ****/
        /// <summary>Get a case-insensitive collection, copying the original collection if needed.</summary>
        /// <param name="collection">The collection to return or copy.</param>
        /// <returns>Returns the original collection if it's non-null and case-insensitive, else a new collection.</returns>
        public static HashSet<string> ToNonNullCaseInsensitive(this HashSet<string>? collection)
        {
            if (collection == null)
                return new(StringComparer.OrdinalIgnoreCase);

            if (!object.ReferenceEquals(collection.Comparer, StringComparer.OrdinalIgnoreCase))
                return new(collection, StringComparer.OrdinalIgnoreCase);

            return collection;
        }

        /// <summary>Get a case-insensitive collection, copying the original collection if needed.</summary>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <param name="collection">The collection to return or copy.</param>
        /// <returns>Returns the original collection if it's non-null and case-insensitive, else a new collection.</returns>
        public static Dictionary<string, TValue> ToNonNullCaseInsensitive<TValue>(this Dictionary<string, TValue>? collection)
        {
            if (collection == null)
                return new(StringComparer.OrdinalIgnoreCase);

            if (!object.ReferenceEquals(collection.Comparer, StringComparer.OrdinalIgnoreCase))
                return new(collection, StringComparer.OrdinalIgnoreCase);

            return collection;
        }
    }
}
