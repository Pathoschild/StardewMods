using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token whose value doesn't change after it's initialised.</summary>
    internal class ImmutableToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The allowed values for the root token (or <c>null</c> if any value is allowed).</summary>
        private readonly InvariantHashSet AllowedRootValues;

        /// <summary>The current token values.</summary>
        private readonly InvariantHashSet Values;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="values">Get the current token values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values (or <c>null</c> to set it based on the given values).</param>
        public ImmutableToken(TokenName name, InvariantHashSet values, InvariantHashSet allowedValues = null, bool? canHaveMultipleValues = null)
            : base(name, canHaveMultipleRootValues: false)
        {
            this.Values = values ?? new InvariantHashSet();
            this.AllowedRootValues = allowedValues;
            this.CanHaveMultipleRootValues = canHaveMultipleValues ?? (this.Values.Count > 1 || this.AllowedRootValues == null || this.AllowedRootValues.Count > 1);
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
            this.IsMutable = false;
            this.IsValidInContext = true;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="canHaveMultipleValues">Whether the token may contain multiple values (or <c>null</c> to set it based on the given values).</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        /// <param name="values">Get the current token values.</param>
        public ImmutableToken(string name, InvariantHashSet values, InvariantHashSet allowedValues = null, bool? canHaveMultipleValues = null)
            : this(TokenName.Parse(name), values, allowedValues, canHaveMultipleValues) { }

        /// <summary>Get the allowed values for a token name (or <c>null</c> if any value is allowed).</summary>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override InvariantHashSet GetAllowedValues(TokenName name)
        {
            if (name.HasSubkey())
                return new InvariantHashSet { true.ToString(), false.ToString() };
            return this.AllowedRootValues;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName name)
        {
            this.AssertTokenName(name);

            if (name.HasSubkey())
                return new[] { this.Values.Contains(name.Subkey).ToString() };
            return this.Values;
        }
    }
}
