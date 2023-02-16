using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
        private readonly ContextualState State = new();


        /*********
        ** Accessor
        *********/
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool AllowsPositionalInput => this.Provider.AllowsInput();

        /// <inheritdoc />
        public bool RequiresPositionalInput => this.Provider.RequiresInput();

        /// <inheritdoc />
        public bool IsMutable => this.Provider.IsMutable();

        public bool IsDeterministicForInput => this.Provider.IsDeterministicForInput();

        /// <inheritdoc />
        /// <remarks>This is cached to ensure it never changes outside a context update (even if the mod token is otherwise incorrectly changing without a context update), since that would cause subtle hard-to-troubleshoot bugs where patches don't update correctly in some cases.</remarks>
        public bool IsReady { get; private set; }

        /// <inheritdoc />
        public bool BypassesContextValidation => false;

        /// <inheritdoc />
        public Func<string, string>? NormalizeValue => this.Provider.NormalizeValue;


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
            this.IsReady = provider.IsReady();
        }

        /// <inheritdoc />
        public bool CanHaveMultipleValues(IInputArguments input)
        {
            return this.Provider.CanHaveMultipleValues(this.ToApiInput(input));
        }

        /// <inheritdoc />
        public bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
        {
            return this.Provider.TryValidateInput(this.ToApiInput(input), out error);
        }

        /// <inheritdoc />
        public bool TryValidateValues(IInputArguments input, IInvariantSet values, [NotNullWhen(false)] out string? error)
        {
            return this.Provider.TryValidateValues(this.ToApiInput(input), values, out error);
        }

        /// <inheritdoc />
        public IInvariantSet GetValidPositionalArgs()
        {
            return InvariantSets.From(this.Provider.GetValidInputs());
        }

        /// <inheritdoc />
        public bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
        {
            if (this.Provider.HasBoundedValues(this.ToApiInput(input), out IEnumerable<string>? values))
            {
                allowedValues = InvariantSets.From(values);
                return true;
            }

            allowedValues = null;
            return false;
        }

        /// <inheritdoc />
        public bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            return this.Provider.HasBoundedRangeValues(this.ToApiInput(input), out min, out max);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetValues(IInputArguments input)
        {
            return this.Provider.GetValues(this.ToApiInput(input));
        }

        /// <inheritdoc />
        public bool UpdateContext(IContext context)
        {
            bool wasReady = this.IsReady;

            bool changed = this.Provider.UpdateContext();
            this.IsReady = this.Provider.IsReady();

            return changed || this.IsReady != wasReady;
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.State;
        }

        /// <inheritdoc />
        public IInvariantSet GetTokensUsed()
        {
            return InvariantSets.Empty;
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Convert input arguments into the format used by the mod API.</summary>
        /// <param name="input">The input arguments.</param>
        private string? ToApiInput(IInputArguments input)
        {
            if (!input.IsReady)
                return null;

            StringBuilder inputStr = new();

            inputStr.Append(string.Join(", ", input.PositionalArgs));
            foreach (var arg in input.NamedArgs)
                inputStr.Append($" |{arg.Key}={string.Join(", ", arg.Value.Parsed)}");

            return inputStr.Length > 0
                ? inputStr.ToString()
                : null;
        }
    }
}
