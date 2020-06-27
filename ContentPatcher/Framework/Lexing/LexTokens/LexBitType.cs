namespace ContentPatcher.Framework.Lexing.LexTokens
{
    /// <summary>A lexical character pattern type.</summary>
    public enum LexBitType
    {
        /// <summary>A literal string.</summary>
        Literal,

        /// <summary>The characters which start a token ('{{').</summary>
        StartToken,

        /// <summary>The characters which end a token ('}}').</summary>
        EndToken,

        /// <summary>The character which separates a token name from its positional input arguments (':').</summary>
        PositionalInputArgSeparator,

        /// <summary>The character which separates a token name or positional input arguments from named input arguments ('|').</summary>
        NamedInputArgSeparator
    }
}
