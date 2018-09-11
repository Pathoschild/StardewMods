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
        public TokenName Name { get; }

        /// <summary>Whether the token may contain multiple values.</summary>
        public virtual bool CanHaveMultipleValues { get; }

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
        public virtual IEnumerable<TokenName> GetSubkeys()
        {
            yield break;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or this token require a subkeys and <paramref name="name"/> does not specify one.</exception>
        public virtual IEnumerable<string> GetValues(TokenName? name = null)
        {
            this.AssertTokenName(name);
            yield break;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values.</param>
        /// <param name="requiresSubkeys">Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</param>
        protected BaseToken(TokenName name, bool canHaveMultipleValues, bool requiresSubkeys)
        {
            this.Name = name;
            this.CanHaveMultipleValues = canHaveMultipleValues;
            this.RequiresSubkeys = requiresSubkeys;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values.</param>
        /// <param name="requiresSubkeys">Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</param>
        protected BaseToken(string name, bool canHaveMultipleValues, bool requiresSubkeys)
            : this(TokenName.Parse(name), canHaveMultipleValues, requiresSubkeys) { }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.RequiresSubkeys"/>.</exception>
        protected void AssertTokenName(TokenName? name)
        {
            if (name == null)
            {
                // missing subkey
                if (this.RequiresSubkeys)
                    throw new InvalidOperationException($"The '{this.Name}' token requires a subkey.");
            }
            else
            {
                // not same root key
                if (!this.Name.IsSameRootKey(name.Value))
                    throw new InvalidOperationException($"The specified token key ({name}) is not handled by this token ({this.Name}).");

                // no subkey allowed
                if (!this.RequiresSubkeys && name.Value.HasSubkey())
                    throw new InvalidOperationException($"The '{this.Name}' token does not support subkeys (:).");
            }
        }
    }
}
