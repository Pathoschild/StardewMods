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

        /// <summary>A unique key indicating which in-game object the condition type applies to. For example, the NPC name when <see cref="Type"/> is <see cref="ConditionType.Relationship"/>.</summary>
        public string ForID { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        /// <param name="forID">A unique key indicating which in-game object the condition type applies to. For example, the NPC name when <paramref name="type"/> is <see cref="ConditionType.Relationship"/>.</param>
        public ConditionKey(ConditionType type, string forID = null)
        {
            this.Type = type;
            this.ForID = forID;
        }

        /// <summary>Get a string representation for this instance.</summary>
        public override string ToString()
        {
            return this.ForID != null
                ? $"{this.Type}:{this.ForID}"
                : this.Type.ToString();
        }

        /****
        ** IEquatable
        ****/
        /// <summary>Get whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ConditionKey other)
        {
            return
                this.Type == other.Type
                && this.ForID == other.ForID;
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
            return this.ToString().GetHashCode();
        }

        /****
        ** Static parsing
        ****/
        /// <summary>Parse a raw string into a condition key if it's valid.</summary>
        /// <param name="raw">The raw string.</param>
        /// <returns>Returns true if <paramref name="raw"/> was successfully parsed, else false.</returns>
        public static ConditionKey Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentNullException(nameof(raw));

            // extract parts
            ConditionType type;
            string forID;
            {
                string[] parts = raw.Trim().Split(new[] { ':' }, 2);

                // condition type
                if (!Enum.TryParse(parts[0], true, out type))
                    throw new FormatException($"Can't parse string '{parts[0]}' as a {nameof(ConditionKey)} value.");

                // for ID
                forID = parts.Length == 2 ? parts[1].Trim() : null;
                if (forID == "")
                    forID = null;
            }

            // create instance
            return new ConditionKey(type, forID);
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
