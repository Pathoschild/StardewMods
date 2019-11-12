using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider whose values need to be changed manually.</summary>
    internal class ManualValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
        private readonly InvariantHashSet AllowedRootValues;

        /// <summary>The current token values.</summary>
        private InvariantHashSet Values;

        /// <summary>Whether the value has changed</summary>
        private bool Changed = false;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The value provider name.</param>
        /// <param name="values">Get the current token values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        /// <param name="canHaveMultipleValues">Whether the root may contain multiple values (or <c>null</c> to set it based on the given values).</param>
        public ManualValueProvider(string name, InvariantHashSet values, InvariantHashSet allowedValues = null, bool? canHaveMultipleValues = null)
            : base(name, canHaveMultipleValuesForRoot: false)
        {
            this.Values = values ?? new InvariantHashSet();
            this.AllowedRootValues = allowedValues?.Any() == true ? allowedValues : null;
            this.CanHaveMultipleValuesForRoot = canHaveMultipleValues ?? (this.Values.Count > 1 || this.AllowedRootValues == null || this.AllowedRootValues.Count > 1);
            this.EnableInputArguments(required: false, canHaveMultipleValues: false);
            this.IsMutable = true;
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

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            if (this.Changed)
            {
                this.Changed = false;
                return true;
            }
            else
                return false;
        }

        /// <summary>Update the values that this value provider provides.</summary>
        /// <param name="values">The new values.</param>
        public void UpdateValues(InvariantHashSet values)
        {
            if (!this.TryValidateValues(null, values, out string error))
                throw new InvalidOperationException($"Invalid updated value: {error}");

            this.Values = values;
            this.Changed = true;
        }
    }
}
