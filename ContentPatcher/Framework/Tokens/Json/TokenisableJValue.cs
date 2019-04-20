using ContentPatcher.Framework.Conditions;
using Newtonsoft.Json.Linq;

namespace ContentPatcher.Framework.Tokens.Json
{
    /// <summary>Tracks a <see cref="JValue"/> instance whose value is set by a tokenisable <see cref="TokenString"/>.</summary>
    internal class TokenisableJValue : IContextual
    {
        /*********
        ** Access
        *********/
        /// <summary>The underlying JSON value token.</summary>
        public JValue Field { get; }

        /// <summary>The token string which provides the field value.</summary>
        public TokenString TokenString { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.TokenString.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.TokenString.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="field">The underlying JSON value.</param>
        /// <param name="tokenString">The token string which provides the field value.</param>
        public TokenisableJValue(JValue field, TokenString tokenString)
        {
            this.Field = field;
            this.TokenString = tokenString;
        }


        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            bool changed = this.TokenString.UpdateContext(context);
            this.Field.Value = this.TokenString.Value;
            return changed;
        }
    }
}
