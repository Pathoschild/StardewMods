using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which gets the result of a dynamic query.</summary>
    internal class QueryValueProvider : BaseValueProvider
    {
        /*********
        ** Private methods
        *********/
        /// <summary>The underlying data table used to parse expressions.</summary>
        private readonly DataTable DataTable = new DataTable();

        /// <summary>A cache of calculations since the last update.</summary>
        private readonly IDictionary<string, object> Cache = new Dictionary<string, object>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public QueryValueProvider()
            : base(ConditionType.Query, canHaveMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, canHaveMultipleValues: false);
            this.MarkReady(true);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            this.Cache.Clear();
            return false;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            yield return this.TryCalculate(input, out object result, out _)
                ? (result is IConvertible convertible ? convertible.ToString(CultureInfo.InvariantCulture) : result.ToString())
                : "0";
        }

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryValidateInput(ITokenString input, out string error)
        {
            return this.TryCalculate(input, out _, out error);
        }

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="IValueProvider.HasBoundedValues"/>.</summary>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override bool HasBoundedRangeValues(ITokenString input, out int min, out int max)
        {
            // TODO: update HasBoundedRangeValue to support double?
            min = int.MinValue;
            max = int.MaxValue;
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Calculate the result of a mathematical expression.</summary>
        /// <param name="input">The input expression.</param>
        /// <param name="result">The result of the calculation.</param>
        /// <param name="error">The error indicating why parsing failed, if applicable.</param>
        private bool TryCalculate(ITokenString input, out object result, out string error)
        {
            // get cached value
            result = 0;
            if (!input.IsMeaningful() || this.Cache.TryGetValue(input.Value, out result))
            {
                error = null;
                return true;
            }

            // recalculate
            try
            {
                this.Cache[input.Value] = result = this.DataTable.Compute(input.Value, string.Empty);
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                string reason = ex.Message;
                reason = Regex.Replace(reason, @"Cannot find column \[([^\]]+)]\.", "invalid expression '$1'.");

                result = 0;
                error = $"Can't parse '{input.Value}' as a math expression: {reason}";
                return false;
            }
        }
    }
}
