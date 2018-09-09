using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token whose value doesn't change after it's initialised.</summary>
    internal class StaticToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The current token values.</summary>
        private readonly InvariantHashSet Values;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token values.</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values.</param>
        /// <param name="values">Get the current token values.</param>
        public StaticToken(string name, bool canHaveMultipleValues, InvariantHashSet values)
            : base(name, canHaveMultipleValues, requiresSubkeys: false)
        {
            this.Values = values;
        }

        /// <summary>Get the current token values.</summary>
        public override IEnumerable<string> GetValues()
        {
            return this.Values;
        }
    }
}
