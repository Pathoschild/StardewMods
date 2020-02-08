using System;
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
            : base(ConditionType.Range, canHaveMultipleValuesForRoot: true)
        {
            this.EnableInputArguments(required: true, canHaveMultipleValues: true);
            this.MarkReady(true);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            return false;
        }

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryValidateInput(ITokenString input, out string error)
        {
            return
                base.TryValidateInput(input, out error)
                && this.TryParseRange(input, out _, out _, out error);
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (!this.TryParseRange(input, out int min, out int max, out string error))
                return Enumerable.Empty<string>(); // error will be shown in validation

            return Enumerable.Range(start: min, count: max - min).Select(p => p.ToString());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse the numeric min/max values from a range specifier if it's valid.</summary>
        /// <param name="input">The input argument containing the range specifier.</param>
        /// <param name="min">The parsed min value, if valid.</param>
        /// <param name="max">The parsed max value, if valid.</param>
        /// <param name="error">The error indicating why the range is invalid, if applicable.</param>
        private bool TryParseRange(ITokenString input, out int min, out int max, out string error)
        {
            min = 0;
            max = 0;

            // check if input provided
            if (!input.IsMeaningful())
            {
                error = $"invalid input argument ({input.Value}), token {this.Name} requires non-blank input.";
                return false;
            }

            // split input
            string[] parts = input.SplitValuesNonUnique().ToArray();
            if (parts.Length != 2)
            {
                error = $"invalid input argument ({input.Value}), must specify a minimum and maximum value like {{{{{this.Name}:0,20}}}}.";
                return false;
            }

            // parse min/max values
            if (!int.TryParse(parts[0], out min))
            {
                error = $"invalid input argument ({input.Value}), can't parse min value '{parts[0]} as an integer.";
                return false;
            }
            if (!int.TryParse(parts[1], out max))
            {
                error = $"invalid input argument ({input.Value}), can't parse max value '{parts[1]} as an integer.";
                return false;
            }

            // validate range
            if (min > max)
            {
                error = $"invalid input argument ({input.Value}), min value '{min}' can't be greater than max value '{max}'.";
                return false;
            }

            int count = (max - min) + 1;
            if (count > RangeValueProvider.MaxCount)
            {
                error = $"invalid input argument ({input.Value}), range can't exceed {RangeValueProvider.MaxCount} numbers (specified range would contain {count} numbers).";
                return false;
            }

            error = null;
            return true;
        }
    }
}
