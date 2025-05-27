using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

/// <summary>A value provider which gets the result of a dynamic query.</summary>
internal class QueryValueProvider : BaseValueProvider
{
    /*********
    ** Fields
    *********/
    /// <summary>A pattern which matches the 'Cannot find column' error message.</summary>
    private static readonly Regex CannotFindColumnPattern = new(@"Cannot find column \[([^\]]+)]\.", RegexOptions.Compiled);


    /*********
    ** Private methods
    *********/
    /// <summary>The underlying data table used to parse expressions.</summary>
    private readonly DataTable DataTable = new();

    /// <summary>A cache of calculations since the last update.</summary>
    private readonly Dictionary<string, object> Cache = [];


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    public QueryValueProvider()
        : base(ConditionType.Query, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
    {
        this.BypassesContextValidation = true;
        this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: null);
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

        string output = this.TryCalculate(input.GetPositionalSegment(), out object? result, out _)
            ? (result is IConvertible convertible
                ? convertible.ToString(CultureInfo.InvariantCulture)
                : (result.ToString() ?? string.Empty)
            )
            : "0";

        return output switch
        {
            "True" => InvariantSets.True,
            "False" => InvariantSets.False,
            _ => InvariantSets.FromValue(output)
        };
    }

    /// <inheritdoc />
    public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
    {
        if (!base.TryValidateInput(input, out error))
            return false;

        if (input.IsReady)
            return this.TryCalculate(input.GetPositionalSegment(), out _, out error);

        return true;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Calculate the result of a mathematical expression.</summary>
    /// <param name="input">The input expression.</param>
    /// <param name="result">The result of the calculation.</param>
    /// <param name="error">The error indicating why parsing failed, if applicable.</param>
    private bool TryCalculate(string? input, [NotNullWhen(true)] out object? result, [NotNullWhen(false)] out string? error)
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
            reason = QueryValueProvider.CannotFindColumnPattern.Replace(reason, "invalid expression '$1'.");

            result = 0;
            error = $"Can't parse '{input}' as a math expression: {reason}";
            return false;
        }
    }
}
