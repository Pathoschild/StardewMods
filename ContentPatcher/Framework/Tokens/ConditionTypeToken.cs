using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for a built-in condition whose value may change with the context.</summary>
    internal class ConditionTypeToken : BaseToken
    {
        /*********
        ** Fields
        *********/
        /// <summary>The allowed values for the root token (or <c>null</c> if any value is allowed).</summary>
        private readonly InvariantHashSet AllowedRootValues;

        /// <summary>Get the current token values.</summary>
        private readonly Func<InvariantHashSet> FetchValues;

        /// <summary>Get whether the token is applicable in the current context, or <c>null</c> if it's always applicable.</summary>
        private readonly Func<bool> IsValidInContextImpl;

        /// <summary>The token values as of the last context update.</summary>
        private readonly InvariantHashSet Values = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token values.</param>
        /// <param name="values">Get the current token value.</param>
        /// <param name="isValidInContext">Get whether the token is applicable in the current context, or <c>null</c> if it's always applicable.</param>
        /// <param name="canHaveMultipleValues">Whether the root token may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeToken(ConditionType name, Func<IEnumerable<string>> values, Func<bool> isValidInContext = null, bool canHaveMultipleValues = false, IEnumerable<string> allowedValues = null)
            : base(name.ToString(), canHaveMultipleValues)
        {
            this.IsValidInContextImpl = isValidInContext;
            this.AllowedRootValues = allowedValues != null ? new InvariantHashSet(allowedValues) : null;
            this.FetchValues = () => new InvariantHashSet(values());
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token values.</param>
        /// <param name="value">Get the current token value.</param>
        /// <param name="isValidInContext">Get whether the token is applicable in the current context, or <c>null</c> if it's always applicable.</param>
        /// <param name="canHaveMultipleValues">Whether the root token may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeToken(ConditionType name, Func<string> value, Func<bool> isValidInContext = null, bool canHaveMultipleValues = false, IEnumerable<string> allowedValues = null)
            : this(name, () => new[] { value() }, isValidInContext, canHaveMultipleValues, allowedValues) { }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.IsValidInContext = this.IsValidInContextImpl == null || this.IsValidInContextImpl();
            this.Values.Clear();
            foreach (string value in this.FetchValues())
                this.Values.Add(value);
        }

        /// <summary>Get the allowed values for a token name (or <c>null</c> if any value is allowed).</summary>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.CanHaveSubkeys"/> or <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override InvariantHashSet GetAllowedValues(TokenName name)
        {
            return name.HasSubkey()
                ? InvariantHashSet.Boolean()
                : this.AllowedRootValues;
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
