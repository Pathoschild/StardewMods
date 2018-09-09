using System.Collections.Generic;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A set of conditions that can be checked against the context.</summary>
    internal class ConditionDictionary : Dictionary<TokenKey, Condition>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Add an element with the given key and condition values.</summary>
        /// <param name="key">The key to add.</param>
        /// <param name="values">The values for the given condition.</param>
        public void Add(TokenKey key, IEnumerable<string> values)
        {
            this.Add(key, new Condition(key, new InvariantHashSet(values)));
        }
    }
}
