#nullable disable

using System.Collections.Generic;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides extension methods for <see cref="IList{T}"/>.</summary>
    internal static class ListExtensions
    {
        /*********
        ** Public fields
        *********/
        /// <summary>Get whether the given index is in range for the list.</summary>
        /// <typeparam name="T">The list value type.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="index">The item index within the list.</param>
        public static bool IsInRange<T>(this IList<T> list, int index)
        {
            return
                index >= 0
                && index < list.Count;
        }

        /// <summary>Get a value from the list if it's in range.</summary>
        /// <typeparam name="T">The list value type.</typeparam>
        /// <param name="list">The list instance.</param>
        /// <param name="index">The item index within the list.</param>
        /// <param name="value">The retrieved value, if applicable.</param>
        /// <returns>Returns whether the index was in range.</returns>
        public static bool TryGetIndex<T>(this IList<T> list, int index, out T value)
        {
            if (!list.IsInRange(index))
            {
                value = default;
                return false;
            }

            value = list[index];
            return true;
        }
    }
}
