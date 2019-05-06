namespace ContentPatcher.Framework.Lexing.LexTokens
{
    /// <summary>A lexical token type.</summary>
    public enum LexTokenType
    {
        /// <summary>A literal string.</summary>
        Literal,

        /// <summary>A Content Patcher token.</summary>
        Token,

        /// <summary>The input argument to a Content Patcher token.</summary>
        TokenInput
    }
}
