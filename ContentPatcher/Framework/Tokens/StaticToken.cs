using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values.</param>
        /// <param name="values">Get the current token values.</param>
        public StaticToken(string name, bool canHaveMultipleValues, InvariantHashSet values)
            : base(name, canHaveMultipleValues, requiresSubkeys: false)
        {
            this.Values = values;
            this.IsValidInContext = true;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName? name = null)
        {
            this.AssertTokenName(name);

            if (this.Values != null)
                return this.Values;
            return Enumerable.Empty<string>();
        }
    }
}
