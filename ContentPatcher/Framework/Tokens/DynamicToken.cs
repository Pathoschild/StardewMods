using System.Collections.Generic;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A dynamic token defined by a content pack.</summary>
    internal class DynamicToken : GenericToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying value provider.</summary>
        private readonly DynamicTokenValueProvider DynamicValues;

        /// <summary>The values which this dynamic token may use.</summary>
        private readonly InvariantHashSet PossibleTokensUsed = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="scope">The mod namespace in which the token is accessible.</param>
        public DynamicToken(string name, string scope)
            : base(new DynamicTokenValueProvider(name), scope)
        {
            this.DynamicValues = (DynamicTokenValueProvider)base.Values;
        }

        /// <summary>Add token names which this dynamic token may depend on.</summary>
        /// <param name="tokens">The token names used.</param>
        public void AddTokensUsed(IEnumerable<string> tokens)
        {
            foreach (string name in tokens)
                this.PossibleTokensUsed.Add(name);
        }

        /// <summary>Add a set of possible values.</summary>
        /// <param name="possibleValues">The possible values to add.</param>
        public void AddAllowedValues(ITokenString possibleValues)
        {
            this.DynamicValues.AddAllowedValues(possibleValues);
            this.CanHaveMultipleRootValues = this.DynamicValues.CanHaveMultipleValues();
        }

        /// <summary>Set the current values.</summary>
        /// <param name="values">The values to set.</param>
        public void SetValue(ITokenString values)
        {
            this.DynamicValues.SetValue(values);
        }

        /// <summary>Set whether the token is valid for the current context.</summary>
        /// <param name="ready">The value to set.</param>
        public void SetReady(bool ready)
        {
            this.DynamicValues.SetReady(ready);
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetPossibleTokensUsed()
        {
            return this.PossibleTokensUsed;
        }
    }
}
