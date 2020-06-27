using System;
using System.Linq;

namespace ContentPatcher.Framework.Lexing.LexTokens
{
    /// <summary>A lexical token representing the input arguments for a Content Patcher token.</summary>
    internal class LexTokenInput : ILexToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The lexical token type.</summary>
        public LexTokenType Type { get; } = LexTokenType.TokenInput;

        /// <summary>The lexical tokens making up the input arguments.</summary>
        public ILexToken[] Parts { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenParts">The lexical tokens making up the input arguments.</param>
        public LexTokenInput(ILexToken[] tokenParts)
        {
            this.MigrateTo(tokenParts);
        }

        /// <summary>Apply changes for a format migration.</summary>
        /// <param name="tokenParts">The lexical token parts to set.</param>
        public void MigrateTo(ILexToken[] tokenParts)
        {
            this.Parts = tokenParts;
        }

        /// <summary>Get a text representation of the lexical token.</summary>
        public override string ToString()
        {
            return string.Join("", this.Parts.Select(p => p.ToString()));
        }
    }
}
