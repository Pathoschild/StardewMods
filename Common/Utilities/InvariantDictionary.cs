using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Common.Utilities
{
    /// <summary>An implementation of <see cref="Dictionary{TKey,TValue}"/> whose keys are guaranteed to use <see cref="StringComparer.OrdinalIgnoreCase"/>.</summary>
    internal class InvariantDictionary<TValue> : Dictionary<string, TValue>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public InvariantDictionary()
            : base(StringComparer.OrdinalIgnoreCase) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="values">The values to add.</param>
        public InvariantDictionary(IDictionary<string, TValue> values)
            : base(values, StringComparer.OrdinalIgnoreCase) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="values">The values to add.</param>
        public InvariantDictionary(IEnumerable<KeyValuePair<string, TValue>> values)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            foreach (var entry in values)
                this.Add(entry.Key, entry.Value);
        }

        /// <summary>Get a new dictionaries with values copied from this one.</summary>
        /// <param name="cloneValue">Custom logic to clone the values if needed.</param>
        public InvariantDictionary<TValue> Clone(Func<TValue, TValue> cloneValue = null)
        {
            return cloneValue != null
                ? new InvariantDictionary<TValue>(this.ToDictionary(p => p.Key, p => cloneValue(p.Value)))
                : new InvariantDictionary<TValue>(this);
        }
    }
}
