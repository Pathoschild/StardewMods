using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider whose values don't change after it's initialized.</summary>
    internal class ImmutableValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
        private readonly InvariantHashSet AllowedRootValues;

        /// <summary>The current token values.</summary>
        private readonly InvariantHashSet Values;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The value provider name.</param>
        /// <param name="values">Get the current token values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        /// <param name="canHaveMultipleValues">Whether the root may contain multiple values (or <c>null</c> to set it based on the given values).</param>
        public ImmutableValueProvider(string name, InvariantHashSet values, InvariantHashSet allowedValues = null, bool? canHaveMultipleValues = null)
            : base(name, canHaveMultipleValuesForRoot: false)
        {
            this.Values = values ?? new InvariantHashSet();
            this.AllowedRootValues = allowedValues?.Any() == true ? allowedValues : null;
            this.CanHaveMultipleValuesForRoot = canHaveMultipleValues ?? (this.Values.Count > 1 || this.AllowedRootValues == null || this.AllowedRootValues.Count > 1);
            this.EnableInputArguments(required: false, canHaveMultipleValues: false);
            this.IsMutable = false;
        }

        /// <summary>Get whether the token always chooses from a set of known values for the given input.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues)
        {
            allowedValues = input.IsMeaningful()
                ? InvariantHashSet.Boolean()
                : this.AllowedRootValues;
            return allowedValues != null;
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
