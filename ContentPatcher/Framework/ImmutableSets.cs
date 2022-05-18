using System.Collections.Generic;
using System.Collections.Immutable;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Provides singleton instances of <see cref="ImmutableHashSet{T}"/> for common values when possible.</summary>
    internal static class ImmutableSets
    {
        /*********
        ** Fields
        *********/
        /// <summary>A set containing only <see cref="string.Empty"/>.</summary>
        private static readonly IInvariantSet BlankString = new InvariantSet(string.Empty);

        /// <summary>A set containing only '0'.</summary>
        private static readonly IInvariantSet Zero = new InvariantSet("0");

        /// <summary>A set containing only '1'.</summary>
        private static readonly IInvariantSet One = new InvariantSet("1");

        /// <summary>A set containing only 'spring'.</summary>
        private static readonly IInvariantSet Spring = new InvariantSet("spring");

        /// <summary>A set containing only 'summer'.</summary>
        private static readonly IInvariantSet Summer = new InvariantSet("summer");

        /// <summary>A set containing only 'fall'.</summary>
        private static readonly IInvariantSet Fall = new InvariantSet("fall");

        /// <summary>A set containing only 'winter'.</summary>
        private static readonly IInvariantSet Winter = new InvariantSet("winter");

        /*********
        ** Accessors
        *********/
        /// <summary>An empty set.</summary>
        public static readonly IInvariantSet Empty = InvariantSet.Empty;

        /// <summary>A set containing only 'true'.</summary>
        public static readonly IInvariantSet True = new InvariantSet("true");

        /// <summary>A set containing only 'false'.</summary>
        public static readonly IInvariantSet False = new InvariantSet("false");

        /// <summary>A set containing only 'true' and 'false'.</summary>
        public static readonly IInvariantSet Boolean = new InvariantSet(new[] { "true", "false" });


        /*********
        ** Public methods
        *********/
        /// <summary>Get an immutable set for the given values.</summary>
        /// <param name="values">The values for which to get a set.</param>
        public static IInvariantSet From(IEnumerable<string> values)
        {
            // shortcut predefined values
            switch (values)
            {
                // already an invariant set
                case IInvariantSet set:
                    return set;

                // use predefined set if possible
                case IList<string> list:
                    switch (list.Count)
                    {
                        case 0:
                            return ImmutableSets.Empty;

                        case 1:
                            return ImmutableSets.FromValue(list[0]);

                        case 2 when bool.TryParse(list[0], out bool left) && bool.TryParse(list[1], out bool right):
                            if (left != right)
                                return ImmutableSets.Boolean;
                            return left
                                ? ImmutableSets.True
                                : ImmutableSets.False;
                    }
                    break;
            }

            // create custom set
            IInvariantSet result = values is MutableInvariantSet mutableSet
                ? mutableSet.GetImmutable()
                : new InvariantSet(values);
            return result.Count == 0
                ? ImmutableSets.Empty
                : result;
        }

        /// <summary>Get an immutable set containing only the given value.</summary>
        /// <param name="value">The set value.</param>
        public static IInvariantSet FromValue(bool value)
        {
            return value
                ? ImmutableSets.True
                : ImmutableSets.False;
        }

        /// <summary>Get an immutable set containing only the given value.</summary>
        /// <param name="value">The set value.</param>
        public static IInvariantSet FromValue(int value)
        {
            return value switch
            {
                0 => ImmutableSets.Zero,
                1 => ImmutableSets.One,
                _ => new InvariantSet(value.ToString())
            };
        }

        /// <summary>Get an immutable set containing only the given value.</summary>
        /// <param name="value">The set value.</param>
        public static IInvariantSet FromValue(string value)
        {
            return
                ImmutableSets.FromPredefinedValueOnly(value)
                ?? new InvariantSet(value);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get an immutable set containing the given value, if it matches any predefined value.</summary>
        /// <param name="value">The value for which to get a set.</param>
        private static IInvariantSet? FromPredefinedValueOnly(string value)
        {
            if (bool.TryParse(value, out bool boolean))
            {
                return boolean
                    ? ImmutableSets.True
                    : ImmutableSets.False;
            }

            return value switch
            {
                // blank string
                "" => ImmutableSets.BlankString,

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
