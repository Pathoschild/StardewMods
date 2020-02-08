using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders.ModConvention
{
    /// <summary>A value provider which delegates the implementation to an underlying wrapper.</summary>
    internal class ConventionValueProvider : IValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying value provider.</summary>
        private readonly ConventionWrapper Provider;

        /// <summary>Diagnostic info about the contextual instance.</summary>
        private readonly ContextualState State = new ContextualState();


        /*********
        ** Accessor
        *********/
        /// <summary>The value provider name.</summary>
        public string Name { get; }

        /// <summary>Whether the value provider allows an input argument (e.g. an NPC name for a relationship token).</summary>
        public bool AllowsInput => this.Provider.AllowsInput();

        /// <summary>Whether the value provider requires an input argument to work, and does not provide values without it (see <see cref="IValueProvider.AllowsInput"/>).</summary>
        public bool RequiresInput => this.Provider.RequiresInput();

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Provider.IsMutable();

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Provider.IsReady();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="provider">The mod-provided value provider.</param>
        public ConventionValueProvider(string name, ConventionWrapper provider)
        {
            this.Name = name;
            this.Provider = provider;
        }


        /// <summary>Whether the value provider may return multiple values for the given input.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        public bool CanHaveMultipleValues(ITokenString input = null)
        {
            return this.Provider.CanHaveMultipleValues(input?.Value);
        }

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryValidateInput(ITokenString input, out string error)
        {
            return this.Provider.TryValidateInput(input?.Value, out error);
        }

        /// <summary>Validate that the provided values are valid for the input argument (regardless of whether they match).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public bool TryValidateValues(ITokenString input, InvariantHashSet values, out string error)
        {
            return this.Provider.TryValidateValues(input?.Value, values, out error);
        }

        /// <summary>Get the set of valid input arguments if restricted, or an empty collection if unrestricted.</summary>
        public InvariantHashSet GetValidInputs()
        {
            return new InvariantHashSet(this.Provider.GetValidInputs());
        }

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="IValueProvider.HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues)
        {
            bool bounded = this.Provider.HasBoundedValues(input?.Value, out IEnumerable<string> values);
            allowedValues = bounded
                ? new InvariantHashSet(values)
                : null;
            return bounded;
        }

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="IValueProvider.HasBoundedValues"/>.</summary>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public bool HasBoundedRangeValues(ITokenString input, out int min, out int max)
        {
            return this.Provider.HasBoundedRangeValues(input?.Value, out min, out max);
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public IEnumerable<string> GetValues(ITokenString input)
        {
            return this.Provider.GetValues(input?.Value);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Provider.UpdateContext();
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.State;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            yield break;
        }
    }
}
