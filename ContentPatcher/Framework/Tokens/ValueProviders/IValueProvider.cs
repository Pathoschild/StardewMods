using System;
using System.Collections.Generic;
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

        /// <summary>Whether the value provider allows an input argument (e.g. an NPC name for a relationship token).</summary>
        bool AllowsInput { get; }

        /// <summary>Whether the value provider requires an input argument to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
        bool RequiresInput { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Whether the value provider may return multiple values for the given input.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        bool CanHaveMultipleValues(ITokenString input = null);

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        bool TryValidateInput(ITokenString input, out string error);

        /// <summary>Validate that the provided values are valid for the input argument (regardless of whether they match).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        bool TryValidateValues(ITokenString input, InvariantHashSet values, out string error);

        /// <summary>Get the set of valid input arguments if restricted, or an empty collection if unrestricted.</summary>
        InvariantHashSet GetValidInputs();

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="AllowsInput"/> or <see cref="RequiresInput"/>.</exception>
        bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues);

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="HasBoundedValues"/>.</summary>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="AllowsInput"/> or <see cref="RequiresInput"/>.</exception>
        bool HasBoundedRangeValues(ITokenString input, out int min, out int max);

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="AllowsInput"/> or <see cref="RequiresInput"/>.</exception>
        IEnumerable<string> GetValues(ITokenString input);
    }
}
