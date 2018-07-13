using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A condition that can be checked against the context.</summary>
    internal class Condition
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The condition key in the context.</summary>
        public ConditionKey Key { get; }

        /// <summary>The condition values for which this condition is valid.</summary>
        public InvariantHashSet Values { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The condition key in the context.</param>
        /// <param name="values">The condition values for which this condition is valid.</param>
        public Condition(ConditionKey key, InvariantHashSet values)
        {
            this.Key = key;
            this.Values = values;
        }

        /// <summary>Whether the condition matches.</summary>
        /// <param name="context">The condition context.</param>
        public bool IsMatch(ConditionContext context)
        {
            return context
                .GetValues(this.Key)
                .Any(p => this.Values.Contains(p));
        }
    }
}
