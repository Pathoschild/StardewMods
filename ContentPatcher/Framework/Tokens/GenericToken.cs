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
        /// <inheritdoc />
        public string Scope { get; }

        /// <inheritdoc />
        public virtual string Name { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Values.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Values.IsReady;

        /// <inheritdoc />
        public bool RequiresInput => this.Values.RequiresPositionalInput;


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
            this.CanHaveMultipleRootValues = provider.CanHaveMultipleValues(InputArguments.Empty);
        }

        /// <inheritdoc />
        public virtual bool UpdateContext(IContext context)
        {
            return this.Values.UpdateContext(context);
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetTokensUsed()
        {
            return this.Values.GetTokensUsed();
        }

        /// <inheritdoc />
        public virtual IContextualState GetDiagnosticState()
        {
            return this.Values.GetDiagnosticState();
        }

        /// <inheritdoc />
        public virtual bool CanHaveMultipleValues(IInputArguments input)
        {
            return this.Values.CanHaveMultipleValues(input);
        }

        /// <inheritdoc />
        public virtual bool TryValidateInput(IInputArguments input, out string error)
        {
            return this.Values.TryValidateInput(input, out error);
        }

        /// <inheritdoc />
        public virtual bool TryValidateValues(IInputArguments input, InvariantHashSet values, IContext context, out string error)
        {
            if (!this.TryValidateInput(input, out error) || !this.Values.TryValidateValues(input, values, out error))
                return false;

            error = null;
            return true;
        }

        /// <inheritdoc />
        public virtual InvariantHashSet GetAllowedInputArguments()
        {
            return this.Values.GetValidPositionalArgs();
        }

        /// <inheritdoc />
        public virtual bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            return this.Values.HasBoundedValues(input, out allowedValues);
        }

        /// <inheritdoc />
        public virtual bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            return this.Values.HasBoundedRangeValues(input, out min, out max);
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetValues(IInputArguments input)
        {
            return this.Values.GetValues(input);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="provider">The underlying value provider.</param>
        /// <param name="scope">The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</param>
        protected GenericToken(string name, IValueProvider provider, string scope = null)
            : this(provider, scope)
        {
            this.Name = name;
        }
    }
}
