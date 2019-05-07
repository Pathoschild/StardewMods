using ContentPatcher.Framework.Tokens.ValueProviders;

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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        public DynamicToken(string name)
            : base(new DynamicTokenValueProvider(name))
        {
            this.DynamicValues = (DynamicTokenValueProvider)base.Values;
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
    }
}
