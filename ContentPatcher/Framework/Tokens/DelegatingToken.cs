using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token which delegates implementation to a nested instance.</summary>
    internal abstract class DelegatingToken : IToken
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public virtual bool IsMutable => this.Token.IsMutable;

        /// <inheritdoc />
        public virtual bool IsReady => this.Token.IsReady;

        /// <inheritdoc />
        public virtual string Scope => this.Token.Scope;

        /// <inheritdoc />
        public virtual string Name => this.Token.Name;

        /// <inheritdoc />
        public virtual bool RequiresInput => this.Token.RequiresInput;

        /// <summary>The wrapped token instance.</summary>
        public IToken Token { get; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual IEnumerable<string> GetTokensUsed()
        {
            return this.Token.GetTokensUsed();
        }

        /// <inheritdoc />
        public virtual bool UpdateContext(IContext context)
        {
            return this.Token.UpdateContext(context);
        }

        /// <inheritdoc />
        public virtual IContextualState GetDiagnosticState()
        {
            return this.Token.GetDiagnosticState();
        }

        /// <inheritdoc />
        public virtual bool CanHaveMultipleValues(IInputArguments input)
        {
            return this.Token.CanHaveMultipleValues(input);
        }

        /// <inheritdoc />
        public virtual bool TryValidateInput(IInputArguments input, out string error)
        {
            return this.Token.TryValidateInput(input, out error);
        }

        /// <inheritdoc />
        public virtual bool TryValidateValues(IInputArguments input, InvariantHashSet values, IContext context, out string error)
        {
            return this.Token.TryValidateValues(input, values, context, out error);
        }

        /// <inheritdoc />
        public virtual InvariantHashSet GetAllowedInputArguments()
        {
            return this.Token.GetAllowedInputArguments();
        }

        /// <inheritdoc />
        public virtual bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            return this.Token.HasBoundedValues(input, out allowedValues);
        }

        /// <inheritdoc />
        public virtual bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            return this.Token.HasBoundedRangeValues(input, out min, out max);
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetValues(IInputArguments input)
        {
            return this.Token.GetValues(input);
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="token">The wrapped token instance.</param>
        protected DelegatingToken(IToken token)
        {
            this.Token = token;
        }
    }
}
