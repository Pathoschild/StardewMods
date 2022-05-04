using System.Collections.Generic;
using System.Globalization;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which returns the number of values in an input.</summary>
    internal class CountValueProvider : BaseValueProvider
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public CountValueProvider()
            : base(ConditionType.Count, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: false, mayReturnMultipleValues: false, maxPositionalArgs: null);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            bool changed = !this.IsReady;
            this.MarkReady(true);
            return changed;
        }

        /// <inheritdoc />
        public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            min = 0;
            max = int.MaxValue;
            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return new[] { input.PositionalArgs.Length.ToString(CultureInfo.InvariantCulture) };
        }
    }
}
