using System;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.Json
{
    /// <summary>Tracks a instance whose value is set by a tokenisable <see cref="TokenString"/>.</summary>
    internal class TokenisableProxy : IContextual
    {
        /*********
        ** Access
        *********/
        /// <summary>The token string which provides the field value.</summary>
        public TokenString TokenString { get; }

        /// <summary>Set the instance value.</summary>
        public Action<string> SetValue { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.TokenString.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.TokenString.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenString">The token string which provides the field value.</param>
        /// <param name="setValue">Set the instance value.</param>
        public TokenisableProxy(TokenString tokenString, Action<string> setValue)
        {
            this.TokenString = tokenString;
            this.SetValue = setValue;
        }


        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            bool changed = this.TokenString.UpdateContext(context);
            this.SetValue(this.TokenString.Value);
            return changed;
        }
    }
}
