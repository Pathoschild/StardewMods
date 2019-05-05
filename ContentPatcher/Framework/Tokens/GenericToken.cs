using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A combination of one or more value providers.</summary>
    internal class GenericToken : IToken
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying value provider.</summary>
        protected IValueProvider Values { get; }

        /// <summary>Whether the root token may contain multiple values.</summary>
        protected bool CanHaveMultipleRootValues { get; set; }

        /// <summary>Backing field for <see cref="GetTokenString"/>.</summary>
        [Obsolete("This is a transitional field for tracking.")]
        private IContext LastContext;


        /*********
        ** Accessors
        *********/
        /// <summary>The token name.</summary>
        public TokenName Name { get; }

        /// <summary>Whether the value can change after it's initialised.</summary>
        public bool IsMutable => this.Values.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Values.IsReady;

        /// <summary>Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</summary>
        public bool CanHaveSubkeys => this.Values.AllowsInput;

        /// <summary>Whether this token only allows subkeys (see <see cref="IToken.CanHaveSubkeys"/>).</summary>
        public bool RequiresSubkeys => this.Values.RequiresInput;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="provider">The underlying value provider.</param>
        public GenericToken(IValueProvider provider)
        {
            this.Values = provider;

            this.Name = TokenName.Parse(provider.Name);
            this.CanHaveMultipleRootValues = provider.CanHaveMultipleValues();
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public bool UpdateContext(IContext context)
        {
            this.LastContext = context;

            bool changed = false;
            if (this.Values.IsMutable)
            {
                if (this.Values.UpdateContext(context))
                    changed = true;
            }
            return changed;
        }

        /// <summary>Whether the token may return multiple values for the given name.</summary>
        /// <param name="name">The token name.</param>
        public bool CanHaveMultipleValues(TokenName name)
        {
            return this.Values.CanHaveMultipleValues(this.GetTokenString(name.Subkey));
        }

        /// <summary>Perform custom validation.</summary>
        /// <param name="name">The token name to validate.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryValidate(TokenName name, InvariantHashSet values, out string error)
        {
            // validate subkey
            if (name.HasSubkey())
            {
                // check if subkey allowed
                if (!this.CanHaveSubkeys)
                {
                    error = $"invalid subkey ({name}); token does not support subkeys.";
                    return false;
                }

                // check subkey value
                InvariantHashSet validKeys = this.GetAllowedSubkeys();
                if (validKeys?.Any() == true && !validKeys.Contains(name.Key))
                {
                    error = $"invalid subkey ({name}), expected one of {string.Join(", ", validKeys)}";
                    return false;
                }
            }

            // custom validation
            if (!this.Values.TryValidate(this.GetTokenString(name.Subkey), values, out error))
                return false;

            // no issues found
            error = null;
            return true;
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public virtual IEnumerable<TokenName> GetSubkeys()
        {
            return this.Values.GetValidInputs()?.Select(input => new TokenName(this.Name.Key, input));
        }

        /// <summary>Get the allowed values for a token name (or <c>null</c> if any value is allowed).</summary>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public virtual InvariantHashSet GetAllowedValues(TokenName name)
        {
            return this.Values.GetAllowedValues(this.GetTokenString(name.Subkey));
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public virtual IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);
            return this.Values.GetValues(this.GetTokenString(name.Subkey));
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get the allowed subkeys (or <c>null</c> if any value is allowed).</summary>
        protected virtual InvariantHashSet GetAllowedSubkeys()
        {
            return this.Values.GetValidInputs();
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

        /// <summary>Transitional method to convert a string input to <see cref="TokenString"/>.</summary>
        /// <param name="value">The value to convert.</param>
        [Obsolete("This is a transitional method for tracking.")]
        private ITokenString GetTokenString(string value)
        {
            return new TokenString(value, this.LastContext);
        }
    }
}
