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
            : base(ConditionType.Query, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: 1);
            this.MarkReady(true);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            this.Cache.Clear();
            return false;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            yield return this.TryCalculate(input.GetFirstPositionalArg(), out object result, out _)
                ? (result is IConvertible convertible ? convertible.ToString(CultureInfo.InvariantCulture) : result.ToString())
                : "0";
        }

        /// <inheritdoc />
        public override bool TryValidateInput(IInputArguments input, out string error)
        {
            if (!base.TryValidateInput(input, out error))
                return false;

            if (input.IsReady)
                return this.TryCalculate(input.GetFirstPositionalArg(), out _, out error);

            return true;
        }

        /// <inheritdoc />
        public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
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
        private bool TryCalculate(string input, out object result, out string error)
        {
            // get cached value
            result = 0;
            if (string.IsNullOrWhiteSpace(input) || this.Cache.TryGetValue(input, out result))
            {
                error = null;
                return true;
            }

            // recalculate
            try
            {
                this.Cache[input] = result = this.DataTable.Compute(input, string.Empty);
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                string reason = ex.Message;
                reason = Regex.Replace(reason, @"Cannot find column \[([^\]]+)]\.", "invalid expression '$1'.");

                result = 0;
                error = $"Can't parse '{input}' as a math expression: {reason}";
                return false;
            }
        }
    }
}
