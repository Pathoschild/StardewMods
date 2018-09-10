using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>The base class for a token.</summary>
    internal abstract class BaseToken : IToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The token name.</summary>
        public string Name { get; }

        /// <summary>Whether the token may contain multiple values.</summary>
        public bool CanHaveMultipleValues { get; }

        /// <summary>Whether this token requires subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with an <c>Abigail</c> subkey).</summary>
        public bool RequiresSubkeys { get; }

        /// <summary>Whether the token is applicable in the current context.</summary>
        public bool IsValidInContext { get; protected set; }

        /// <summary>The allowed values (or <c>null</c> if any value is allowed).</summary>
        public virtual InvariantHashSet AllowedValues { get; set; } = null;


        /*********
        ** Public methods
        *********/
        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public virtual void UpdateContext(IContext context) { }

        /// <summary>Get the current subkeys (if supported).</summary>
        public virtual IEnumerable<string> GetSubkeys()
        {
            yield break;
        }

        /// <summary>Get the current token values.</summary>
        /// <exception cref="InvalidOperationException">This token does not support subkeys (see <see cref="IToken.RequiresSubkeys"/>).</exception>
        public virtual IEnumerable<string> GetValues()
        {
            if (this.RequiresSubkeys)
                throw new InvalidOperationException($"The {this.Name} token requires a subkey.");
            yield break;
        }

        /// <summary>Get the current token values for a subkey, if <see cref="IToken.RequiresSubkeys"/> is true.</summary>
        /// <param name="subkey">The subkey to check.</param>
        /// <exception cref="InvalidOperationException">This token does not support subkeys (see <see cref="IToken.RequiresSubkeys"/>).</exception>
        public virtual IEnumerable<string> GetValues(string subkey)
        {
            if (!this.RequiresSubkeys)
                throw new InvalidOperationException($"The {this.Name} token does not support subkeys.");
            yield break;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values.</param>
        /// <param name="requiresSubkeys">Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</param>
        protected BaseToken(string name, bool canHaveMultipleValues, bool requiresSubkeys)
        {
            this.Name = name;
            this.CanHaveMultipleValues = canHaveMultipleValues;
            this.RequiresSubkeys = requiresSubkeys;
        }
    }
}
