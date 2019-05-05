using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A conditional value for a dynamic token.</summary>
    internal class DynamicTokenValue
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name of the token whose value to set.</summary>
        public TokenName Name { get; }

        /// <summary>The token value to set.</summary>
        public InvariantHashSet Value { get; }

        /// <summary>The conditions that must match to set this value.</summary>
        public Condition[] Conditions { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The name of the token whose value to set.</param>
        /// <param name="value">The token value to set.</param>
        /// <param name="conditions">The conditions that must match to set this value.</param>
        public DynamicTokenValue(TokenName key, InvariantHashSet value, IEnumerable<Condition> conditions)
        {
            this.Name = key;
            this.Value = value;
            this.Conditions = conditions.ToArray();
        }
    }
}
