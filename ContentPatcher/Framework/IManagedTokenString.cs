using System.Collections.Generic;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>A string value optionally containing tokens.</summary>
    internal interface IManagedTokenString : IContextual, ITokenString
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The lexical tokens parsed from the raw string.</summary>
        ILexToken[] LexTokens { get; }

        /// <summary>The unrecognised tokens in the string.</summary>
        InvariantHashSet InvalidTokens { get; }

        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        bool IsSingleTokenOnly { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Recursively get the token placeholders from the given lexical tokens.</summary>
        /// <param name="recursive">Whether to scan recursively.</param> 
        IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive);
    }
}
