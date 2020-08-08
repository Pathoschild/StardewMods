using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which provides a range of integer values.</summary>
    internal class RangeValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The maximum number of entries to allow in a range.</summary>
        private const int MaxCount = 5000;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public RangeValueProvider()
            : base(ConditionType.Range, mayReturnMultipleValuesForRoot: true)
        {
            this.EnableInputArguments(required: true, mayReturnMultipleValues: true, maxPositionalArgs: 2);
            this.MarkReady(true);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool TryValidateInput(IInputArguments input, out string error)
        {
            if (!base.TryValidateInput(input, out error))
                return false;

            if (input.IsReady)
            {
                return
                    base.TryValidateInput(input, out error)
                    && this.TryParseRange(input, out _, out _, out error);
            }

            return true;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            return this.TryParseRange(input, out int min, out int max, out _)
                ? Enumerable.Range(start: min, count: max - min + 1).Select(p => p.ToString())
                : Enumerable.Empty<string>(); // error will be shown in validation
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse the numeric min/max values from a range specifier if it's valid.</summary>
        /// <param name="input">The input arguments containing the range specifier.</param>
        /// <param name="min">The parsed min value, if valid.</param>
        /// <param name="max">The parsed max value, if valid.</param>
        /// <param name="error">The error indicating why the range is invalid, if applicable.</param>
        private bool TryParseRange(IInputArguments input, out int min, out int max, out string error)
        {
            min = 0;
            max = 0;
            string errorPrefix = $"invalid input ('{input.TokenString.Value}')";

            // check if input provided
            if (!input.HasPositionalArgs)
            {
                error = $"{errorPrefix}, token {this.Name} requires input arguments.";
                return false;
            }

            // validate length
            if (input.PositionalArgs.Length != 2)
            {
                error = $"{errorPrefix}, must specify a minimum and maximum value like {{{{{this.Name}:0,20}}}}.";
                return false;
            }

            // parse min/max values
            if (!int.TryParse(input.PositionalArgs[0], out min))
            {
                error = $"{errorPrefix}, can't parse min value '{input.PositionalArgs[0]}' as an integer.";
                return false;
            }
            if (!int.TryParse(input.PositionalArgs[1], out max))
            {
                error = $"{errorPrefix}, can't parse max value '{input.PositionalArgs[1]}' as an integer.";
                return false;
            }

            // validate range
            if (min > max)
            {
                error = $"{errorPrefix}, min value '{min}' can't be greater than max value '{max}'.";
                return false;
            }

            int count = (max - min) + 1;
            if (count > RangeValueProvider.MaxCount)
            {
                error = $"{errorPrefix}, range can't exceed {RangeValueProvider.MaxCount} numbers (specified range would contain {count} numbers).";
                return false;
            }

            error = null;
            return true;
        }
    }
}
