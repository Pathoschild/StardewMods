using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>The base class for a token.</summary>
    internal abstract class BaseToken : IToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>Whether the root token may contain multiple values.</summary>
        protected bool CanHaveMultipleRootValues { get; set; }

        /// <summary>Whether the token subkeys may contain multiple values.</summary>
        protected bool CanHaveMultipleSubkeyValues { get; set; }


        /*********
        ** Accessors
        *********/
        /// <summary>The token name.</summary>
        public TokenName Name { get; }

        /// <summary>Whether the value can change after it's initialised.</summary>
        public bool IsMutable { get; protected set; } = true;

        /// <summary>Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</summary>
        public bool CanHaveSubkeys { get; private set; }

        /// <summary>Whether this token only allows subkeys (see <see cref="IToken.CanHaveSubkeys"/>).</summary>
        public bool RequiresSubkeys { get; private set; }

        /// <summary>Whether the token is applicable in the current context.</summary>
        public bool IsValidInContext { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public virtual void UpdateContext(IContext context) { }

        /// <summary>Whether the token may return multiple values for the given name.</summary>
        /// <param name="name">The token name.</param>
        public bool CanHaveMultipleValues(TokenName name)
        {
            return name.HasSubkey()
                ? this.CanHaveMultipleSubkeyValues
                : this.CanHaveMultipleRootValues;
        }

        /// <summary>Perform custom validation on a set of input values.</summary>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public virtual bool TryCustomValidation(InvariantHashSet values, out string error)
        {
            error = null;
            return true;
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public virtual IEnumerable<TokenName> GetSubkeys()
        {
            yield break;
        }

        /// <summary>Get the allowed values for a token name (or <c>null</c> if any value is allowed).</summary>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public virtual InvariantHashSet GetAllowedValues(TokenName name)
        {
            return null;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public virtual IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);
            yield break;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleRootValues">Whether the root token may contain multiple values.</param>
        protected BaseToken(TokenName name, bool canHaveMultipleRootValues)
        {
            this.Name = name;
            this.CanHaveMultipleRootValues = canHaveMultipleRootValues;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleRootValues">Whether the root token may contain multiple values.</param>
        protected BaseToken(string name, bool canHaveMultipleRootValues)
            : this(TokenName.Parse(name), canHaveMultipleRootValues) { }

        /// <summary>Enable subkeys for this token.</summary>
        /// <param name="required">Whether a subkey is required when using this token.</param>
        /// <param name="canHaveMultipleValues">Whether a token subkey may return multiple values.</param>
        protected void EnableSubkeys(bool required, bool canHaveMultipleValues)
        {
            this.CanHaveSubkeys = true;
            this.RequiresSubkeys = required;
            this.CanHaveMultipleSubkeyValues = canHaveMultipleValues;
        }

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
                if (!this.CanHaveSubkeys && name.Value.HasSubkey())
                    throw new InvalidOperationException($"The '{this.Name}' token does not allow subkeys (:).");
            }
        }

        /// <summary>Try to parse a raw case-insensitive string into an enum value.</summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="raw">The raw string to parse.</param>
        /// <param name="result">The resulting enum value.</param>
        /// <param name="mustBeNamed">When parsing a numeric value, whether it must match one of the named enum values.</param>
        protected bool TryParseEnum<TEnum>(string raw, out TEnum result, bool mustBeNamed = true) where TEnum : struct
        {
            if (!Enum.TryParse(raw, true, out result))
                return false;

            if (mustBeNamed && !Enum.IsDefined(typeof(TEnum), result))
                return false;

            return true;
        }
    }
}
