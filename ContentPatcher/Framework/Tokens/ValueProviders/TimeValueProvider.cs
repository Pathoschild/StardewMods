using System;
using System.Collections.Generic;
using System.Globalization;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider for the in-game clock time.</summary>
    internal class TimeValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether the basic save info is loaded.</summary>
        private readonly Func<bool> IsBasicInfoLoaded;

        /// <summary>The clock time as of the last context update.</summary>
        private string TimeOfDay;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="isBasicInfoLoaded">Whether the basic save info is loaded.</param>
        public TimeValueProvider(Func<bool> isBasicInfoLoaded)
            : base(ConditionType.Time, mayReturnMultipleValuesForRoot: false)
        {
            this.IsBasicInfoLoaded = isBasicInfoLoaded;
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                string oldTime = this.TimeOfDay;
                this.TimeOfDay = this.MarkReady(this.IsBasicInfoLoaded())
                    ? this.NormalizeValue(Game1.timeOfDay)
                    : null;
                return oldTime != this.TimeOfDay;
            });
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return new[] { this.TimeOfDay };
        }

        /// <inheritdoc />
        public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            min = 600;
            max = 2600;
            return true;
        }

        /// <inheritdoc />
        public override string NormalizeValue(string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
                return value;

            return value.TrimStart('0').PadLeft(4, '0');
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize a time value to 24-hour military format.</summary>
        /// <param name="value">The value to normalize.</param>
        private string NormalizeValue(int value)
        {
            return this.NormalizeValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
