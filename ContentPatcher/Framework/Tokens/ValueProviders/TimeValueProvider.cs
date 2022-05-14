using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for the in-game clock time.</summary>
    internal class TimeValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Handles reading info from the current save.</summary>
        private readonly TokenSaveReader SaveReader;

        /// <summary>The clock time as of the last context update.</summary>
        private string? TimeOfDay;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="saveReader">Handles reading info from the current save.</param>
        public TimeValueProvider(TokenSaveReader saveReader)
            : base(ConditionType.Time, mayReturnMultipleValuesForRoot: false)
        {
            this.SaveReader = saveReader;
            this.NormalizeValue = this.NormalizeValueImpl;
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                string? oldTime = this.TimeOfDay;
                this.TimeOfDay = this.MarkReady(this.SaveReader.IsReady)
                    ? this.NormalizeValueImpl(this.SaveReader.GetTime())
                    : null;
                return oldTime != this.TimeOfDay;
            });
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return BaseValueProvider.WrapOptionalValue(this.TimeOfDay);
        }

        /// <inheritdoc />
        public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            min = 600;
            max = 2600;
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize a time value to 24-hour military format.</summary>
        /// <param name="value">The value to normalize, already trimmed and non-empty.</param>
        private string NormalizeValueImpl(string value)
        {
            if (value.Length > 4)
                value = value.TrimStart('0');

            if (value.Length < 4)
                value = value.PadLeft(4, '0');

            return value;
        }

        /// <summary>Normalize a time value to 24-hour military format.</summary>
        /// <param name="value">The value to normalize.</param>
        private string NormalizeValueImpl(int value)
        {
            return value.ToString("D4");
        }
    }
}
