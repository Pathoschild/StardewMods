using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A generic token for a value provider.</summary>
    internal class ValueProviderToken : BaseToken
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying value provider.</summary>
        protected IValueProvider Values { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="valueProvider">The underlying value provider.</param>
        public ValueProviderToken(IValueProvider valueProvider)
            : base(valueProvider.Name, valueProvider.CanHaveMultipleValues())
        {
            this.Values = valueProvider;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);
            return this.Values.GetValues(name.Subkey);
        }

        /// <summary>Get the allowed subkeys (or <c>null</c> if any value is allowed).</summary>
        protected override InvariantHashSet GetAllowedSubkeys()
        {
            return this.Values.GetValidInputs();
        }

        /// <summary>Get the allowed values for a token name (or <c>null</c> if any value is allowed).</summary>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override InvariantHashSet GetAllowedValues(TokenName name)
        {
            return this.Values.GetAllowedValues(name.Subkey);
        }

        /// <summary>Get the current subkeys (if supported).</summary>
        public override IEnumerable<TokenName> GetSubkeys()
        {
            return this.Values.GetValidInputs()?.Select(input => new TokenName(this.Name.Key, input));
        }

        /// <summary>Perform custom validation on a subkey/value pair.</summary>
        /// <param name="name">The token name to validate.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryValidate(TokenName name, string value, out string error)
        {
            return this.Values.TryValidate(name.Subkey, new InvariantHashSet { value }, out error);
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            base.UpdateContext(context);
            if (this.Values.IsMutable)
            {
                this.Values.UpdateContext(context);
                this.IsValidInContext = this.Values.IsValidInContext;
            }
        }
    }
}
