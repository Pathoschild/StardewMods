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
        ** Dictionary
        ****/
        /// <summary>Remove all matching values from the dictionary.</summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <param name="dictionary">The dictionary whose values to remove.</param>
        /// <param name="where">A callback which returns true if the entry should be removed.</param>
        /// <returns>Returns whether any values were removed.</returns>
        public static bool RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> where)
        {
            List<TKey> removeKeys = new();
            foreach ((TKey key, TValue value) in dictionary)
            {
                if (where(key, value))
                    removeKeys.Add(key);
            }

            foreach (TKey key in removeKeys)
                dictionary.Remove(key);

            return removeKeys.Count is not 0;
        }


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
