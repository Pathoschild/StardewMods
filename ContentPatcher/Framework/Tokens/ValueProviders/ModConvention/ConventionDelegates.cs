using System.Collections.Generic;

namespace ContentPatcher.Framework.Tokens.ValueProviders.ModConvention
{
    /// <summary>Method delegates which represent a simplified version of <see cref="IValueProvider"/> that can be implemented by custom mod tokens through the API via <see cref="ConventionValueProvider"/>.</summary>
    /// <remarks>Methods should be kept in sync with <see cref="ConventionWrapper"/>.</remarks>
    internal static class ConventionDelegates
    {
        /****
        ** Metadata
        ****/
        /// <summary>Get whether the values may change depending on the context.</summary>
        /// <remarks>Default true.</remarks>
        internal delegate bool IsMutable();

        /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
        /// <remarks>Default false.</remarks>
        internal delegate bool AllowsInput();

        /// <summary>Whether the token requires input arguments to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
        /// <remarks>Default false.</remarks>
        internal delegate bool RequiresInput();

        /// <summary>Whether the token may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <remarks>Default true.</remarks>
        internal delegate bool CanHaveMultipleValues(string input = null);

        /// <summary>Get the set of valid input arguments if restricted, or an empty collection if unrestricted.</summary>
        /// <remarks>Default unrestricted.</remarks>
        internal delegate IEnumerable<string> GetValidInputs();

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <remarks>Default unrestricted.</remarks>
        internal delegate bool HasBoundedValues(string input, out IEnumerable<string> allowedValues);

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="HasBoundedValues"/>.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <remarks>Default false.</remarks>
        internal delegate bool HasBoundedRangeValues(string input, out int min, out int max);

        /// <summary>Validate that the provided input arguments are valid.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        /// <remarks>Default true.</remarks>
        internal delegate bool TryValidateInput(string input, out string error);

        /// <summary>Validate that the provided values are valid for the given input arguments (regardless of whether they match).</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        /// <remarks>Default true.</remarks>
        internal delegate bool TryValidateValues(string input, IEnumerable<string> values, out string error);


        /****
        ** State
        ****/
        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        internal delegate bool UpdateContext();

        /// <summary>Get whether the token is available for use.</summary>
        internal delegate bool IsReady();

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if any.</param>
        internal delegate IEnumerable<string> GetValues(string input);
    }
}
