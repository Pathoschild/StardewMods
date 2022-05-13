using System.Collections.Generic;
using System.Collections.Immutable;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which checks whether a given value is non-blank.</summary>
    internal class HasValueValueProvider : BaseValueProvider
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public HasValueValueProvider()
            : base(ConditionType.HasValue, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: false, mayReturnMultipleValues: false, maxPositionalArgs: null);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out IImmutableSet<string> allowedValues)
        {
            allowedValues = ImmutableSets.Boolean;
            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return ImmutableSets.FromValue(input.HasPositionalArgs);
        }
    }
}
