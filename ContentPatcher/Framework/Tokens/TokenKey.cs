using System;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>Represents a token key and subkey if applicable (e.g. <c>Relationship:Abigail</c> is token key <c>Relationship</c> and subkey <c>Abigail</c>).</summary>
    internal struct TokenKey : IEquatable<TokenKey>
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Predefined keys
        ****/
        /// <summary>A predefined condition key for <see cref="ConditionType.Day"/>.</summary>
        public static TokenKey Day { get; } = new TokenKey(ConditionType.Day);

        /// <summary>A predefined condition key for <see cref="ConditionType.DayOfWeek"/>.</summary>
        public static TokenKey DayOfWeek { get; } = new TokenKey(ConditionType.DayOfWeek);

        /// <summary>A predefined condition key for <see cref="ConditionType.Language"/>.</summary>
        public static TokenKey Language { get; } = new TokenKey(ConditionType.Language);

        /// <summary>A predefined condition key for <see cref="ConditionType.Season"/>.</summary>
        public static TokenKey Season { get; } = new TokenKey(ConditionType.Season);

        /// <summary>A predefined condition key for <see cref="ConditionType.Weather"/>.</summary>
        public static TokenKey Weather { get; } = new TokenKey(ConditionType.Weather);

        /// <summary>A predefined condition key for <see cref="ConditionType.Year"/>.</summary>
        public static TokenKey Year { get; } = new TokenKey(ConditionType.Year);

        /****
        ** Properties
        ****/
        /// <summary>The token type.</summary>
        public string Key { get; }

        /// <summary>The token subkey, if applicable  indicating which in-game object the condition type applies to. For example, the NPC name when <see cref="Key"/> is <see cref="ConditionType.Relationship"/>.</summary>
        public string Subkey { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenKey">The condition type.</param>
        /// <param name="subkey">A unique key indicating which in-game object the condition type applies to. For example, the NPC name when <paramref name="tokenKey"/> is <see cref="ConditionType.Relationship"/>.</param>
        public TokenKey(string tokenKey, string subkey = null)
        {
            this.Key = tokenKey?.Trim();
            this.Subkey = subkey?.Trim();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="tokenKey">The condition type.</param>
        /// <param name="subkey">A unique key indicating which in-game object the condition type applies to. For example, the NPC name when <paramref name="tokenKey"/> is <see cref="ConditionType.Relationship"/>.</param>
        public TokenKey(ConditionType tokenKey, string subkey = null)
            : this(tokenKey.ToString(), subkey) { }

        /// <summary>Get a string representation for this instance.</summary>
        public override string ToString()
        {
            return this.Subkey != null
                ? $"{this.Key}:{this.Subkey}"
                : this.Key;
        }

        /****
        ** IEquatable
        ****/
        /// <summary>Get whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(TokenKey other)
        {
            return
                this.Key.Equals(other.Key, StringComparison.InvariantCultureIgnoreCase)
                && (
                    this.Subkey?.Equals(other.Subkey, StringComparison.CurrentCultureIgnoreCase)
                    ?? other.Subkey == null
                );
        }

        /// <summary>Get whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        public override bool Equals(object obj)
        {
            return obj is TokenKey other && this.Equals(other);
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
        public static TokenKey Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentNullException(nameof(raw));

            // extract parts
            string key;
            string subkey;
            {
                string[] parts = raw.Trim().Split(new[] { ':' }, 2);

                key = parts[0].Trim();
                if (key == "")
                    throw new ArgumentException($"The main key in '{raw}' can't be blank.");

                subkey = parts.Length == 2 ? parts[1].Trim() : null;
                if (subkey == "")
                    subkey = null;
            }

            // create instance
            return new TokenKey(key, subkey);
        }

        /// <summary>Parse a raw string into a condition key if it's valid.</summary>
        /// <param name="raw">The raw string.</param>
        /// <param name="key">The parsed condition key.</param>
        /// <returns>Returns true if <paramref name="raw"/> was successfully parsed, else false.</returns>
        public static bool TryParse(string raw, out TokenKey key)
        {
            try
            {
                key = TokenKey.Parse(raw);
                return true;
            }
            catch
            {
                key = default(TokenKey);
                return false;
            }
        }
    }
}
