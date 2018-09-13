using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token whose value doesn't change after it's initialised.</summary>
    internal class ImmutableToken : IToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The current token values.</summary>
        private readonly InvariantHashSet Values;


        /*********
        ** Accessors
        *********/
        /// <summary>The token name.</summary>
        public TokenName Name { get; }

        /// <summary>Whether the token is applicable in the current context.</summary>
        public bool IsValidInContext { get; } = true;

        /// <summary>Whether the value can change after it's initialised.</summary>
        public bool IsMutable { get; } = false;

        /// <summary>Whether the token may contain multiple values.</summary>
        public bool CanHaveMultipleValues { get; }

        /// <summary>Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</summary>
        public bool RequiresSubkeys { get; } = false;

        /// <summary>The allowed values (or <c>null</c> if any value is allowed).</summary>
        public InvariantHashSet AllowedValues { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="values">Get the current token values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values (or <c>null</c> to set it based on the given values).</param>
        public ImmutableToken(TokenName name, InvariantHashSet values, InvariantHashSet allowedValues = null, bool? canHaveMultipleValues = null)
        {
            this.Name = name;
            this.Values = values ?? new InvariantHashSet();
            this.AllowedValues = allowedValues;
            this.CanHaveMultipleValues = canHaveMultipleValues ?? (this.Values.Count > 1 || this.AllowedValues == null || this.AllowedValues.Count > 1);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values (or <c>null</c> to set it based on the given values).</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        /// <param name="values">Get the current token values.</param>
        public ImmutableToken(string name, InvariantHashSet values, InvariantHashSet allowedValues = null, bool? canHaveMultipleValues = null)
            : this(TokenName.Parse(name), values, allowedValues, canHaveMultipleValues) { }


        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public void UpdateContext(IContext context) { }

        /// <summary>Perform custom validation on a set of input values.</summary>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryCustomValidation(InvariantHashSet values, out string error)
        {
            error = null;
            return true;
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public IEnumerable<TokenName> GetSubkeys()
        {
            yield break;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.RequiresSubkeys"/>.</exception>
        public IEnumerable<string> GetValues(TokenName? name = null)
        {
            if (name != null)
            {
                if (!this.Name.IsSameRootKey(name.Value))
                    throw new InvalidOperationException($"The specified token key ({name}) is not handled by this token ({this.Name}).");
                if (name.Value.HasSubkey())
                    throw new InvalidOperationException($"The '{this.Name}' token does not support subkeys (:).");
            }

            return this.Values;
        }
    }
}
