using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Provides extension methods for internal use.</summary>
    internal static class InternalExtensions
    {
        /*********
        ** Public methods
        *********/
        /****
        ** Case-insensitive extensions
        ****/
        /// <summary>Get the set difference of two sequences, using the invariant culture and ignoring case.</summary>
        /// <param name="source">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="other" /> is <see langword="null" />.</exception>
        public static IEnumerable<string> ExceptIgnoreCase(this IEnumerable<string> source, IEnumerable<string> other)
        {
            return source.Except(other, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>Group the elements of a sequence according to a specified key selector function, comparing the keys using the invariant culture and ignoring case.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The sequence whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IEnumerable<IGrouping<string, TSource>> GroupByIgnoreCase<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            return source.GroupBy(keySelector, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>Sort the elements of a sequence in ascending order by using a specified comparer.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IOrderedEnumerable<TSource> OrderByIgnoreCase<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            return source.OrderBy(keySelector, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>Perform a subsequent ordering of the elements in a sequence in ascending order according to a key.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The sequence whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IOrderedEnumerable<TSource> ThenByIgnoreCase<TSource>(this IOrderedEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            return source.ThenBy(keySelector, StringComparer.InvariantCultureIgnoreCase);
        }

        /****
        ** Tokens
        ****/
        /// <summary>Get whether a token string has a meaningful value.</summary>
        /// <param name="str">The token string.</param>
        public static bool IsMeaningful(this ITokenString str)
        {
            return !string.IsNullOrWhiteSpace(str?.Value);
        }

        /// <summary>Get unique comma-separated values from a token string.</summary>
        /// <param name="tokenStr">The token string to parse.</param>
        /// <exception cref="InvalidOperationException">The token string is not ready (<see cref="IContextual.IsReady"/> is false).</exception>
        public static InvariantHashSet SplitValues(this ITokenString tokenStr)
        {
            if (string.IsNullOrWhiteSpace(tokenStr?.Value))
                return new InvariantHashSet();
            if (!tokenStr.IsReady)
                throw new InvalidOperationException($"Can't get values from a non-ready token string (raw value: {tokenStr.Raw}).");

            return new InvariantHashSet(
                tokenStr.Value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
            );
        }
    }
}
