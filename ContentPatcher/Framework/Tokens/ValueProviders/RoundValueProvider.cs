using System;
using System.Collections.Generic;
using System.Globalization;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which rounds numeric values.</summary>
    internal class RoundValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Indicates how to round a number.</summary>
        private enum RoundMode
        {
            /// <summary>The default round mode, which matches <see cref="MidpointRounding.ToEven"/>.</summary>
            Default,

            /// <summary>Round up to the next matching value.</summary>
            Up,

            /// <summary>Round down to the next matching value.</summary>
            Down
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public RoundValueProvider()
            : base(ConditionType.Round, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: null);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            bool changed = !this.IsReady;
            this.MarkReady(true);
            return changed;
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
                    && this.TryParse(input, out _, out _, out _, out error);
            }

            return true;
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            if (this.TryParseAndRound(input, out decimal value, out _))
            {
                allowedValues = new InvariantHashSet(value.ToString(CultureInfo.InvariantCulture));
                return true;
            }
            else
            {
                allowedValues = null;
                return false;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            if (!this.TryParseAndRound(input, out decimal value, out string parseError))
                throw new InvalidOperationException($"Invalid input value '{input.TokenString.Value}': {parseError}"); // should never happen

            yield return value.ToString(CultureInfo.InvariantCulture);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse input arguments and get the rounded value if valid.</summary>
        /// <param name="input">The input to parse.</param>
        /// <param name="result">The rounded value, if applicable.</param>
        /// <param name="parseError">A human-readable error indicating why parsing failed, if applicable.</param>
        private bool TryParseAndRound(IInputArguments input, out decimal result, out string parseError)
        {
            if (!this.TryParse(input, out decimal value, out int decimals, out RoundMode mode, out parseError))
            {
                result = 0;
                return false;
            }

            result = this.Round(value, decimals, mode);
            return true;
        }

        /// <summary>Round a numeric value to the given number of digits.</summary>
        /// <param name="value">The value to round.</param>
        /// <param name="decimals">The number of digits after the decimal point to keep.</param>
        /// <param name="mode">The rounding logic to apply.</param>
        private decimal Round(decimal value, int decimals, RoundMode mode)
        {
            switch (mode)
            {
                case RoundMode.Default:
                    return Math.Round(value, decimals);

                case RoundMode.Down:
                case RoundMode.Up:
                    {
                        decimal multiplier = (decimal)Math.Pow(10, decimals);
                        return mode == RoundMode.Down
                            ? Math.Floor(value * multiplier) / multiplier
                            : Math.Ceiling(value * multiplier) / multiplier;
                    }

                default:
                    throw new NotSupportedException($"Unknown round mode '{mode}'.");
            }
        }

        /// <summary>Try to parse input arguments.</summary>
        /// <param name="input">The input arguments to parse.</param>
        /// <param name="value">The value to round.</param>
        /// <param name="decimals">The number of digits after the decimal point to keep.</param>
        /// <param name="mode">The rounding logic to apply.</param>
        /// <param name="parseError">A human-readable error indicating why parsing failed, if applicable.</param>
        private bool TryParse(IInputArguments input, out decimal value, out int decimals, out RoundMode mode, out string parseError)
        {
            // set defaults
            value = 0;
            decimals = 0;
            mode = RoundMode.Default;

            // skip if not ready
            if (!input.TokenString.IsReady)
            {
                parseError = "input isn't ready";
                return false;
            }

            // parse parts
            string[] args = input.PositionalArgs;
            if (args.Length < 1 || args.Length > 3)
            {
                parseError = $"input '{input.TokenString.Value}' must have 1-3 comma-separated arguments.";
                return false;
            }
            if (!decimal.TryParse(args[0], out value))
            {
                parseError = $"value '{args[0]}' can't be parsed as a numeric value.";
                return false;
            }
            if (args.Length >= 2)
            {
                if (!int.TryParse(args[1], out decimals))
                {
                    parseError = $"digit count '{args[1]}' can't be parsed as an integer value.";
                    return false;
                }
                if (decimals < 0)
                {
                    parseError = $"digit count '{args[1]}' can't be less than zero.";
                    return false;
                }
            }
            if (args.Length >= 3)
            {
                if (!Enum.TryParse(args[2], ignoreCase: true, out mode))
                {
                    parseError = $"round mode '{args[2]}' is invalid; must be one of '{string.Join(", ", Enum.GetNames(typeof(RoundMode)))}'.";
                    return false;
                }
            }

            // valid
            parseError = null;
            return true;
        }
    }
}
