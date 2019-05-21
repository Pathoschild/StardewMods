using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A combination of one or more value providers.</summary>
    internal class GenericToken : IToken
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying value provider.</summary>
        protected IValueProvider Values { get; }

        /// <summary>Whether the root token may contain multiple values.</summary>
        protected bool CanHaveMultipleRootValues { get; set; }


        /*********
        ** Accessors
        *********/
        /// <summary>The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</summary>
        public string Scope { get; }

        /// <summary>The token name.</summary>
        public string Name { get; }

        /// <summary>Whether the value can change after it's initialised.</summary>
        public bool IsMutable => this.Values.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Values.IsReady;

        /// <summary>Whether this token recognises input arguments (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with an <c>Abigail</c> input).</summary>
        public bool CanHaveInput => this.Values.AllowsInput;

        /// <summary>Whether this token is only valid with an input argument (see <see cref="IToken.CanHaveInput"/>).</summary>
        public bool RequiresInput => this.Values.RequiresInput;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="provider">The underlying value provider.</param>
        /// <param name="scope">The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</param>
        public GenericToken(IValueProvider provider, string scope = null)
        {
            this.Values = provider;
            this.Scope = scope;
            this.Name = provider.Name;
            this.CanHaveMultipleRootValues = provider.CanHaveMultipleValues();
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Values.UpdateContext(context);
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.Values.GetTokensUsed();
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.Values.GetDiagnosticState();
        }

        /// <summary>Whether the token may return multiple values for the given name.</summary>
        /// <param name="input">The input argument, if any.</param>
        public bool CanHaveMultipleValues(ITokenString input)
        {
            return this.Values.CanHaveMultipleValues(input);
        }

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryValidateInput(ITokenString input, out string error)
        {
            return this.Values.TryValidateInput(input, out error);
        }

        /// <summary>Validate that the provided values are valid for the input argument (regardless of whether they match).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryValidateValues(ITokenString input, InvariantHashSet values, IContext context, out string error)
        {
            if (!this.TryValidateInput(input, out error) || !this.Values.TryValidateValues(input, values, out error))
                return false;

            error = null;
            return true;
        }

        /// <summary>Get the allowed input arguments, if supported and restricted to a specific list.</summary>
        public InvariantHashSet GetAllowedInputArguments()
        {
            return this.Values.GetValidInputs();
        }

        /// <summary>Get the allowed values for an input argument (or <c>null</c> if any value is allowed).</summary>
        /// <param name="input">The input argument, if any.</param>
        /// <exception cref="InvalidOperationException">The input does not respect <see cref="IToken.CanHaveInput"/> or <see cref="IToken.RequiresInput"/>.</exception>
        public virtual InvariantHashSet GetAllowedValues(ITokenString input)
        {
            return this.Values.GetAllowedValues(input);
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="input">The input to check, if any.</param>
        /// <exception cref="InvalidOperationException">The input does not respect <see cref="IToken.CanHaveInput"/> or <see cref="IToken.RequiresInput"/>.</exception>
        public virtual IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInput(input);
            return this.Values.GetValues(input);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Assert that an input argument is valid.</summary>
        /// <param name="input">The input to check, if any.</param>
        /// <exception cref="InvalidOperationException">The input does not respect <see cref="IToken.CanHaveInput"/> or <see cref="IToken.RequiresInput"/>.</exception>
        protected void AssertInput(ITokenString input)
        {
            bool hasInput = input.IsMeaningful();

            if (!this.CanHaveInput && hasInput)
                throw new InvalidOperationException($"The '{this.Name}' token does not allow input arguments ({InternalConstants.InputArgSeparator}).");
            if (this.RequiresInput && !hasInput)
                throw new InvalidOperationException($"The '{this.Name}' token requires an input argument.");
        }

        /// <summary>Try to parse a raw case-insensitive string into an enum value.</summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="raw">The raw string to parse.</param>
        /// <param name="result">The resulting enum value.</param>
        /// <param name="mustBeNamed">When parsing a numeric value, whether it must match one of the named enum values.</param>
        protected bool TryParseEnum<TEnum>(string raw, out TEnum result, bool mustBeNamed = true) where TEnum : struct
        {
            if (!Enum.TryParse(raw, true, out result))
                return false;

            if (mustBeNamed && !Enum.IsDefined(typeof(TEnum), result))
                return false;

            return true;
        }
    }
}
