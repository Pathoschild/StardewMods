using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A simple token whose value can be changed externally.</summary>
    internal class DynamicToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The allowed values for the root token (or <c>null</c> if any value is allowed).</summary>
        private readonly InvariantHashSet AllowedRootValues;

        /// <summary>The current values.</summary>
        private InvariantHashSet Values = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        public DynamicToken(TokenName name)
            : base(name, canHaveMultipleRootValues: false)
        {
            this.AllowedRootValues = new InvariantHashSet();
            this.EnableSubkeys(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Add a set of possible values.</summary>
        /// <param name="possibleValues">The possible values to add.</param>
        public void AddAllowedValues(InvariantHashSet possibleValues)
        {
            foreach (string value in possibleValues)
                this.AllowedRootValues.Add(value);
            this.CanHaveMultipleRootValues = this.CanHaveMultipleRootValues || possibleValues.Count > 1;
        }

        /// <summary>Set the current values.</summary>
        /// <param name="values">The values to set.</param>
        public void SetValue(InvariantHashSet values)
        {
            this.Values = values;
        }

        /// <summary>Set whether the token is valid in the current context.</summary>
        /// <param name="validInContext">The value to set.</param>
        public void SetValidInContext(bool validInContext)
        {
            this.IsValidInContext = validInContext;
        }

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
