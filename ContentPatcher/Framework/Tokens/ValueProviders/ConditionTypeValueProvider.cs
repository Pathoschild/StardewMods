using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for a built-in condition whose value may change with the context.</summary>
    internal class ConditionTypeValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
        private readonly IImmutableSet<string>? AllowedRootValues;

        /// <summary>Get the current values.</summary>
        private readonly Func<IImmutableSet<string>> FetchValues;

        /// <summary>Get whether the value provider is applicable in the current context, or <c>null</c> if it's always applicable.</summary>
        private readonly Func<bool>? IsValidInContextImpl;

        /// <summary>The values as of the last context update.</summary>
        private IImmutableSet<string> Values = ImmutableSets.Empty;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        /// <param name="values">Get the current values.</param>
        /// <param name="isValidInContext">Get whether the value provider is applicable in the current context, or <c>null</c> if it's always applicable.</param>
        /// <param name="mayReturnMultipleValues">Whether the root may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeValueProvider(ConditionType type, Func<IEnumerable<string>> values, Func<bool>? isValidInContext = null, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
            : base(type, mayReturnMultipleValues)
        {
            this.IsValidInContextImpl = isValidInContext;
            this.AllowedRootValues = allowedValues != null ? ImmutableSets.From(allowedValues) : null;
            this.FetchValues = () => ImmutableSets.From(values());
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type.</param>
        /// <param name="value">Get the current value.</param>
        /// <param name="isValidInContext">Get whether the value provider is applicable in the current context, or <c>null</c> if it's always applicable.</param>
        /// <param name="mayReturnMultipleValues">Whether the root may contain multiple values.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeValueProvider(ConditionType type, Func<string?> value, Func<bool>? isValidInContext = null, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
            : this(type, () => BaseValueProvider.WrapOptionalValue(value()), isValidInContext, mayReturnMultipleValues, allowedValues) { }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(this.Values, () =>
            {
                return this.Values = this.MarkReady(this.IsValidInContextImpl == null || this.IsValidInContextImpl())
                    ? ImmutableSets.From(this.FetchValues())
                    : this.Values.Clear();
            });
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IImmutableSet<string>? allowedValues)
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
