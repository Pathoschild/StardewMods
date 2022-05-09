using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>Provides values for a token name with optional input.</summary>
    internal interface IValueProvider : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The value provider name.</summary>
        string Name { get; }

        /// <summary>Whether the value provider allows positional input arguments (e.g. an NPC name for a relationship token).</summary>
        bool AllowsPositionalInput { get; }

        /// <summary>Whether the value provider requires positional input arguments, and does not provide values without it (see <see cref="AllowsPositionalInput"/>).</summary>
        bool RequiresPositionalInput { get; }

        /// <summary>Whether to allow using this token in any value context (e.g. as a number or boolean) without validating ahead of time.</summary>
        bool BypassesContextValidation { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Whether the value provider may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments.</param>
        bool CanHaveMultipleValues(IInputArguments input);

        /// <summary>Validate that the provided input arguments are valid.</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error);

        /// <summary>Validate that the provided values are valid for the given input arguments (regardless of whether they match).</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        bool TryValidateValues(IInputArguments input, InvariantHashSet values, [NotNullWhen(false)] out string? error);

        /// <summary>Get the set of valid input arguments if restricted, or null/empty if unrestricted.</summary>
        InvariantHashSet? GetValidPositionalArgs();

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input doesn't match this value provider.</exception>
        bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out InvariantHashSet? allowedValues);

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="HasBoundedValues"/>.</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <exception cref="InvalidOperationException">The input doesn't match this value provider.</exception>
        bool HasBoundedRangeValues(IInputArguments input, out int min, out int max);

        /// <summary>Normalize a raw value so it can be compared with the token values.</summary>
        /// <param name="value">The raw value.</param>
        string? NormalizeValue(string? value);

        /// <summary>Get the current values in the expected sort order for indexing.</summary>
        /// <param name="input">The input arguments.</param>
        /// <exception cref="InvalidOperationException">The input doesn't match this value provider.</exception>
        IEnumerable<string> GetValues(IInputArguments input);
    }
}
