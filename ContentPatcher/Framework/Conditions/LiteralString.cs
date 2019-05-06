using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A literal string value.</summary>
    internal class LiteralString : ITokenString
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        public string Raw { get; }

        /// <summary>The lexical tokens parsed from the raw string.</summary>
        public ILexToken[] LexTokens { get; }

        /// <summary>The unrecognised tokens in the string.</summary>
        public InvariantHashSet InvalidTokens { get; } = new InvariantHashSet();

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        public bool HasAnyTokens { get; } = false;

        /// <summary>Whether the token string value may change depending on the context.</summary>
        public bool IsMutable { get; } = false;

        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        public bool IsSingleTokenOnly { get; } = false;

        /// <summary>The string with tokens substituted for the last context update.</summary>
        public string Value { get; }

        /// <summary>Whether all tokens in the value have been replaced.</summary>
        public bool IsReady { get; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="value">The literal string value.</param>
        public LiteralString(string value)
        {
            this.Raw = value;
            this.Value = value;
            this.LexTokens = new ILexToken[] { new LexTokenLiteral(value) };
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return false;
        }
    }
}
