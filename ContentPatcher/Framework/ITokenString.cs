using System.Collections.Generic;
using ContentPatcher.Framework.Lexing.LexTokens;

namespace ContentPatcher.Framework
{
    /// <summary>A string value optionally containing tokens.</summary>
    internal interface ITokenString : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        string Raw { get; }

        /// <summary>The lexical tokens parsed from the raw string.</summary>
        ILexToken[] LexTokens { get; }

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        bool HasAnyTokens { get; }

        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        bool IsSingleTokenOnly { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        string Value { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Recursively get the token placeholders from the given lexical tokens.</summary>
        /// <param name="recursive">Whether to scan recursively.</param>
        IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive);
    }
}
