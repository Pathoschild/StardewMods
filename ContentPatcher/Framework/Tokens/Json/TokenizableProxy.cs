using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Tokens.Json
{
    /// <summary>Tracks a instance whose value is set by a tokenizable <see cref="TokenString"/>.</summary>
    internal class TokenizableProxy : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The token string which provides the field value.</summary>
        public IManagedTokenString TokenString { get; }

        /// <summary>Set the instance value.</summary>
        public Action<string> SetValue { get; }

        /// <inheritdoc />
        public bool IsMutable => this.TokenString.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.TokenString.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenString">The token string which provides the field value.</param>
        /// <param name="setValue">Set the instance value.</param>
        public TokenizableProxy(IManagedTokenString tokenString, Action<string> setValue)
        {
            this.TokenString = tokenString;
            this.SetValue = setValue;
        }


        /// <inheritdoc />
        public bool UpdateContext(IContext context)
        {
            bool changed = this.TokenString.UpdateContext(context);
            this.SetValue(this.TokenString.Value);
            return changed;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetTokensUsed()
        {
            return this.TokenString.GetTokensUsed();
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.TokenString.GetDiagnosticState();
        }
    }
}
