using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ContentPatcher.Framework
{
    /// <summary>Provides singleton instances of <see cref="ImmutableHashSet{T}"/> for common values when possible.</summary>
    internal static class ImmutableSets
    {
        /*********
        ** Fields
        *********/
        /// <summary>The string comparer to use for all immutable sets.</summary>
        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>A set containing only <see cref="string.Empty"/>.</summary>
        private static readonly IImmutableSet<string> BlankString = new[] { string.Empty }.ToImmutableHashSet();

        /// <summary>A set containing only '0'.</summary>
        private static readonly IImmutableSet<string> Zero = new[] { "0" }.ToImmutableHashSet();

        /// <summary>A set containing only '1'.</summary>
        private static readonly IImmutableSet<string> One = new[] { "1" }.ToImmutableHashSet();

        /// <summary>A set containing only 'spring'.</summary>
        private static readonly IImmutableSet<string> Spring = new[] { "spring" }.ToImmutableHashSet();

        /// <summary>A set containing only 'summer'.</summary>
        private static readonly IImmutableSet<string> Summer = new[] { "summer" }.ToImmutableHashSet();

        /// <summary>A set containing only 'fall'.</summary>
        private static readonly IImmutableSet<string> Fall = new[] { "fall" }.ToImmutableHashSet();

        /// <summary>A set containing only 'winter'.</summary>
        private static readonly IImmutableSet<string> Winter = new[] { "winter" }.ToImmutableHashSet();

        /*********
        ** Accessors
        *********/
        /// <summary>An empty set.</summary>
        public static readonly IImmutableSet<string> Empty = Array.Empty<string>().ToImmutableHashSet(ImmutableSets.Comparer);

        /// <summary>A set containing only 'true'.</summary>
        public static readonly IImmutableSet<string> True = new[] { "true" }.ToImmutableHashSet(ImmutableSets.Comparer);

        /// <summary>A set containing only 'false'.</summary>
        public static readonly IImmutableSet<string> False = new[] { "false" }.ToImmutableHashSet(ImmutableSets.Comparer);

        /// <summary>A set containing only 'true' and 'false'.</summary>
        public static readonly IImmutableSet<string> Boolean = new[] { "true", "false" }.ToImmutableHashSet(ImmutableSets.Comparer);


        /*********
        ** Public methods
        *********/
        /// <summary>Get an immutable set for the given values.</summary>
        /// <param name="values">The values for which to get a set.</param>
        public static IImmutableSet<string> From(IEnumerable<string> values)
        {
            // shortcut predefined values
            switch (values)
            {
                // already an immutable set
                case ImmutableHashSet<string> set:
                    return set.WithComparer(ImmutableSets.Comparer);

                // use predefined set if possible
                case IList<string> list:
                    switch (list.Count)
                    {
                        case 0:
                            return ImmutableSets.Empty;

                        case 1:
                            return ImmutableSets.FromValue(list[0]);
                    }
                    break;
            }

            // create custom set
            ImmutableHashSet<string> result = values.ToImmutableHashSet(ImmutableSets.Comparer);
            return result.Count switch
            {
                0 => ImmutableSets.Empty,
                1 => ImmutableSets.FromPredefinedValueOnly(result.First()) ?? result,
                2 when (result.Contains("true") && result.Contains("false")) => ImmutableSets.Boolean,
                _ => result
            };
        }

        /// <summary>Get an immutable set containing only the given value.</summary>
        /// <param name="value">The set value.</param>
        public static IImmutableSet<string> FromValue(bool value)
        {
            return value
                ? ImmutableSets.True
                : ImmutableSets.False;
        }

        /// <summary>Get an immutable set containing only the given value.</summary>
        /// <param name="value">The set value.</param>
        public static IImmutableSet<string> FromValue(int value)
        {
            return value switch
            {
                0 => ImmutableSets.Zero,
                1 => ImmutableSets.One,
                _ => new[] { value.ToString() }.ToImmutableHashSet(ImmutableSets.Comparer)
            };
        }

        /// <summary>Get an immutable set containing only the given value.</summary>
        /// <param name="value">The set value.</param>
        public static IImmutableSet<string> FromValue(string value)
        {
            return
                ImmutableSets.FromPredefinedValueOnly(value)
                ?? new[] { value }.ToImmutableHashSet(ImmutableSets.Comparer);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get an immutable set containing the given value, if it matches any predefined value.</summary>
        /// <param name="value">The value for which to get a set.</param>
        private static IImmutableSet<string>? FromPredefinedValueOnly(string value)
        {
            return value switch
            {
                // blank string
                "" => ImmutableSets.BlankString,

                // boolean
                "true" or "True" => ImmutableSets.True,
                "false" or "False" => ImmutableSets.False,

                // common digits
                "0" => ImmutableSets.Zero,
                "1" => ImmutableSets.One,

                // season
                "spring" or "Spring" => ImmutableSets.Spring,
                "summer" or "Summer" => ImmutableSets.Summer,
                "fall" or "Fall" => ImmutableSets.Fall,
                "winter" or "Winter" => ImmutableSets.Winter,

                // custom
                _ => null
            };
        }
    }
}
