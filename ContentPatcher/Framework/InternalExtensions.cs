using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

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
        /// <summary>Get whether a value is equal to another, ignoring case.</summary>
        /// <param name="value">The first value to compare.</param>
        /// <param name="other">The second value to compare.</param>
        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return
                value?.Equals(other, StringComparison.OrdinalIgnoreCase)
                ?? other == null;
        }

        /// <summary>Get whether a value contains a substring, ignoring case.</summary>
        /// <param name="value">The first value to compare.</param>
        /// <param name="other">The second value to compare.</param>
        public static bool ContainsIgnoreCase(this string value, string other)
        {
            if (value == null || other == null)
                return value == other;

            return value?.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Get the set difference of two sequences, ignoring case.</summary>
        /// <param name="source">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="other" /> is <see langword="null" />.</exception>
        public static IEnumerable<string> ExceptIgnoreCase(this IEnumerable<string> source, IEnumerable<string> other)
        {
            return source.Except(other, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Group the elements of a sequence according to a specified key selector function, comparing the keys without case.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">The sequence whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IEnumerable<IGrouping<string, TSource>> GroupByIgnoreCase<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            return source.GroupBy(keySelector, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Sort the elements of a sequence in ascending order by using a specified comparer.</summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.</exception>
        public static IOrderedEnumerable<TSource> OrderByIgnoreCase<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            return source.OrderBy(keySelector, StringComparer.OrdinalIgnoreCase);
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
        public static InvariantHashSet SplitValuesUnique(this ITokenString tokenStr)
        {
            return new InvariantHashSet(tokenStr.SplitValuesNonUnique());
        }

        /// <summary>Get comma-separated values from a token string.</summary>
        /// <param name="tokenStr">The token string to parse.</param>
        /// <exception cref="InvalidOperationException">The token string is not ready (<see cref="IContextual.IsReady"/> is false).</exception>
        public static IEnumerable<string> SplitValuesNonUnique(this ITokenString tokenStr)
        {
            if (tokenStr == null)
                return Enumerable.Empty<string>();

            if (!tokenStr.IsReady)
                throw new InvalidOperationException($"Can't get values from a non-ready token string (raw value: {tokenStr.Raw}).");

            return tokenStr.Value.SplitValuesNonUnique();
        }

        /// <summary>Get comma-separated values from a string.</summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="separator">The separator by which to split the value.</param>
        public static IEnumerable<string> SplitValuesNonUnique(this string str, string separator = ",")
        {
            if (string.IsNullOrWhiteSpace(str))
                return Enumerable.Empty<string>();

            return str
                .Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());
        }

        /****
        ** Mod manifest
        ****/
        /// <summary>Get whether the manifest lists a given mod ID as a dependency.</summary>
        /// <param name="manifest">The manifest.</param>
        /// <param name="modID">The mod ID.</param>
        /// <param name="canBeOptional">Whether the dependency can be optional.</param>
        public static bool HasDependency(this IManifest manifest, string modID, bool canBeOptional = true)
        {
            if (manifest == null)
                return false;

            // check content pack for
            if (manifest.ContentPackFor?.UniqueID?.EqualsIgnoreCase(modID) == true)
                return true;

            // check dependencies
            IManifestDependency dependency = manifest.Dependencies?.FirstOrDefault(p => p.UniqueID.EqualsIgnoreCase(modID));
            return
                dependency != null
                && (canBeOptional || dependency.IsRequired);
        }
    }
}
