using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for user-defined dynamic tokens.</summary>
    internal class ManualValueProvider : BaseValueProvider
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
        public ManualValueProvider(string name)
            : base(name, mayReturnMultipleValuesForRoot: false)
        {
            this.AllowedRootValues = new InvariantHashSet();
            this.SetReady(false); // not ready until initialized
        }

        /// <summary>Add a set of possible values.</summary>
        /// <param name="possibleValues">The possible values to add.</param>
        public void AddAllowedValues(ITokenString possibleValues)
        {
            // can't reasonably generate known values if tokens are involved
            if (possibleValues.IsMutable || this.AllowedRootValues == null)
            {
                this.AllowedRootValues = null;
                this.MayReturnMultipleValuesForRoot = true;
                return;
            }

            // get possible values from literal token
            InvariantHashSet splitValues = possibleValues.SplitValuesUnique();
            foreach (string value in splitValues)
                this.AllowedRootValues.Add(value.Trim());
            this.MayReturnMultipleValuesForRoot = this.MayReturnMultipleValuesForRoot || splitValues.Count > 1;
        }

        /// <summary>Set the current values.</summary>
        /// <param name="values">The values to set.</param>
        public void SetValue(ITokenString values)
        {
            this.Values = values.SplitValuesUnique();
        }

        /// <summary>Set whether the token is valid for the current context.</summary>
        /// <param name="ready">The value to set.</param>
        public void SetReady(bool ready)
        {
            this.MarkReady(ready);
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            allowedValues = this.AllowedRootValues;
            return allowedValues != null;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);
            return this.Values;
        }
    }
}
