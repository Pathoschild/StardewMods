using System.Collections.Generic;
using ContentPatcher.Framework.Lexing.LexTokens;

namespace ContentPatcher.Framework
{
    /// <summary>A string value optionally containing tokens, including parsed metadata.</summary>
    internal interface IParsedTokenString : ITokenString
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        bool IsSingleTokenOnly { get; }

        /// <summary>The lexical tokens parsed from the raw string.</summary>
        ILexToken[] LexTokens { get; }

        
        /*********
        ** Methods
        *********/
        /// <summary>Recursively get the token placeholders from the given lexical tokens.</summary>
        /// <param name="recursive">Whether to scan recursively.</param>
        IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive);
    }
}
