using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for user-defined dynamic tokens.</summary>
    internal class DynamicTokenValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
        private InvariantHashSet AllowedRootValues;

        /// <summary>The current values.</summary>
        private InvariantHashSet Values = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The value provider name.</param>
        public DynamicTokenValueProvider(string name)
            : base(name, canHaveMultipleValuesForRoot: false)
        {
            this.AllowedRootValues = new InvariantHashSet();
            this.EnableInputArguments(required: false, canHaveMultipleValues: false);
        }

        /// <summary>Add a set of possible values.</summary>
        /// <param name="possibleValues">The possible values to add.</param>
        public void AddAllowedValues(ITokenString possibleValues)
        {
            // can't reasonably generate known values if tokens are involved
            if (possibleValues.IsMutable || this.AllowedRootValues == null)
            {
                this.AllowedRootValues = null;
                this.CanHaveMultipleValuesForRoot = true;
                return;
            }

            // get possible values from literal token
            InvariantHashSet splitValues = possibleValues.SplitValues();
            foreach (string value in splitValues)
                this.AllowedRootValues.Add(value.Trim());
            this.CanHaveMultipleValuesForRoot = this.CanHaveMultipleValuesForRoot || splitValues.Count > 1;
        }

        /// <summary>Set the current values.</summary>
        /// <param name="values">The values to set.</param>
        public void SetValue(ITokenString values)
        {
            this.Values = values.SplitValues();
        }

        /// <summary>Set whether the token is valid for the current context.</summary>
        /// <param name="ready">The value to set.</param>
        public void SetReady(bool ready)
        {
            this.MarkReady(ready);
        }

        /// <summary>Get the allowed values for an input argument (or <c>null</c> if any value is allowed).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override InvariantHashSet GetAllowedValues(ITokenString input)
        {
            return input.IsMeaningful()
                ? InvariantHashSet.Boolean()
                : this.AllowedRootValues;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (input.IsMeaningful())
                return new[] { this.Values.Contains(input.Value).ToString() };
            return this.Values;
        }
    }
}
