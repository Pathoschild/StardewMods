using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token whose value may change depending on the current context.</summary>
    internal interface IToken : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</summary>
        string Scope { get; }

        /// <summary>The token name.</summary>
        string Name { get; }

        /// <summary>Whether this token recognizes input arguments (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with an <c>Abigail</c> input).</summary>
        bool CanHaveInput { get; }

        /// <summary>Whether this token is only valid with an input argument (see <see cref="CanHaveInput"/>).</summary>
        bool RequiresInput { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Whether the token may return multiple values for the given name.</summary>
        /// <param name="input">The input arguments.</param>
        bool CanHaveMultipleValues(IInputArguments input);

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        bool TryValidateInput(IInputArguments input, out string error);

        /// <summary>Validate that the provided values are valid for the input argument (regardless of whether they match).</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        bool TryValidateValues(IInputArguments input, InvariantHashSet values, IContext context, out string error);

        /// <summary>Get the allowed input arguments, if supported and restricted to a specific list.</summary>
        InvariantHashSet GetAllowedInputArguments();

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input doesn't match this value provider, or doesn't respect <see cref="CanHaveInput"/> or <see cref="RequiresInput"/>.</exception>
        bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues);

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="HasBoundedValues"/>.</summary>
        /// <param name="input">The input arguments.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <exception cref="InvalidOperationException">The input doesn't match this value provider, or doesn't respect <see cref="CanHaveInput"/> or <see cref="RequiresInput"/>.</exception>
        bool HasBoundedRangeValues(IInputArguments input, out int min, out int max);

        /// <summary>Get the current token values.</summary>
        /// <param name="input">The input arguments.</param>
        /// <exception cref="InvalidOperationException">The input doesn't respect <see cref="IToken.CanHaveInput"/> or <see cref="IToken.RequiresInput"/>.</exception>
        IEnumerable<string> GetValues(IInputArguments input);
    }
}
