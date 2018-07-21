using System;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A unique identifier for a contextual condition.</summary>
    internal struct ConditionKey : IEquatable<ConditionKey>
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Predefined keys
        ****/
        /// <summary>A predefined condition key for <see cref="ConditionType.Day"/>.</summary>
        public static ConditionKey Day { get; } = new ConditionKey(ConditionType.Day);

        /// <summary>A predefined condition key for <see cref="ConditionType.DayOfWeek"/>.</summary>
        public static ConditionKey DayOfWeek { get; } = new ConditionKey(ConditionType.DayOfWeek);

        /// <summary>A predefined condition key for <see cref="ConditionType.DayEvent"/>.</summary>
        public static ConditionKey DayEvent { get; } = new ConditionKey(ConditionType.DayEvent);

        /// <summary>A predefined condition key for <see cref="ConditionType.HasFlag"/>.</summary>
        public static ConditionKey HasFlag { get; } = new ConditionKey(ConditionType.HasFlag);

        /// <summary>A predefined condition key for <see cref="ConditionType.HasMod"/>.</summary>
        public static ConditionKey HasMod { get; } = new ConditionKey(ConditionType.HasMod);

        /// <summary>A predefined condition key for <see cref="ConditionType.HasSeenEvent"/>.</summary>
        public static ConditionKey HasSeenEvent { get; } = new ConditionKey(ConditionType.HasSeenEvent);

        /// <summary>A predefined condition key for <see cref="ConditionType.Language"/>.</summary>
        public static ConditionKey Language { get; } = new ConditionKey(ConditionType.Language);

        /// <summary>A predefined condition key for <see cref="ConditionType.Season"/>.</summary>
        public static ConditionKey Season { get; } = new ConditionKey(ConditionType.Season);

        /// <summary>A predefined condition key for <see cref="ConditionType.Spouse"/>.</summary>
        public static ConditionKey Spouse { get; } = new ConditionKey(ConditionType.Spouse);

        /// <summary>A predefined condition key for <see cref="ConditionType.Weather"/>.</summary>
        public static ConditionKey Weather { get; } = new ConditionKey(ConditionType.Weather);

        /****
        ** Properties
        ****/
        /// <summary>The condition type.</summary>
        public ConditionType Type { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        public ConditionKey(ConditionType type)
        {
            this.Type = type;
        }

        /// <summary>Get a string representation for this instance.</summary>
        public override string ToString()
        {
            return this.Type.ToString();
        }

        /****
        ** IEquatable
        ****/
        /// <summary>Get whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ConditionKey other)
        {
            return this.Type == other.Type;
        }

        /// <summary>Get whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        public override bool Equals(object obj)
        {
            return obj is ConditionKey other && this.Equals(other);
        }

        /// <summary>Get the hash code for this instance.</summary>
        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }

        /****
        ** Static parsing
        ****/
        /// <summary>Parse a raw string into a condition key if it's valid.</summary>
        /// <param name="raw">The raw string.</param>
        /// <returns>Returns true if <paramref name="raw"/> was successfully parsed, else false.</returns>
        public static ConditionKey Parse(string raw)
        {
            if (Enum.TryParse(raw, true, out ConditionType type))
                return new ConditionKey(type);

            throw new FormatException($"Can't parse string '{raw}' as a {nameof(ConditionKey)} value.");
        }

        /// <summary>Parse a raw string into a condition key if it's valid.</summary>
        /// <param name="raw">The raw string.</param>
        /// <param name="key">The parsed condition key.</param>
        /// <returns>Returns true if <paramref name="raw"/> was successfully parsed, else false.</returns>
        public static bool TryParse(string raw, out ConditionKey key)
        {
            try
            {
                key = ConditionKey.Parse(raw);
                return true;
            }
            catch
            {
                key = default(ConditionKey);
                return false;
            }
        }
    }
}
