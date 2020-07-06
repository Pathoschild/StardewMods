using System;
using System.Text;

namespace ContentPatcher.Framework.Lexing.LexTokens
{
    /// <summary>A lexical token representing a Content Patcher token.</summary>
    internal class LexTokenToken : ILexToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The lexical token type.</summary>
        public LexTokenType Type { get; } = LexTokenType.Token;

        /// <summary>The Content Patcher token name.</summary>
        public string Name { get; private set; }

        /// <summary>The input arguments passed to the Content Patcher token.</summary>
        public LexTokenInput InputArgs { get; private set; }

        /// <summary>Whether the token omits the start/end character patterns because it's in a token-only context.</summary>
        public bool ImpliedBraces { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The Content Patcher token name.</param>
        /// <param name="inputArgs">The input arguments passed to the Content Patcher token.</param>
        /// <param name="impliedBraces">Whether the token omits the start/end character patterns because it's in a token-only context.</param>
        public LexTokenToken(string name, LexTokenInput inputArgs, bool impliedBraces)
        {
            this.ImpliedBraces = impliedBraces;
            this.MigrateTo(name, inputArgs);
        }

        /// <summary>Apply changes for a format migration.</summary>
        /// <param name="name">The Content Patcher token name.</param>
        /// <param name="inputArgs">The input arguments passed to the Content Patcher token.</param>
        public void MigrateTo(string name, LexTokenInput inputArgs)
        {
            this.Name = name;
            this.InputArgs = inputArgs;
        }

        /// <summary>Get the unique ID of the mod which provides this token, if applicable.</summary>
        public string GetProviderModId()
        {
            string[] nameParts = this.Name.Split(new[] { InternalConstants.ModTokenSeparator }, 2, StringSplitOptions.None);
            return nameParts.Length == 2
                ? nameParts[0].Trim()
                : null;
        }

        /// <summary>Get whether the token has any input arguments.</summary>
        public bool HasInputArgs()
        {
            return this.InputArgs?.Parts.Length > 0;
        }

        /// <summary>Get a text representation of the lexical token.</summary>
        public override string ToString()
        {
            return LexTokenToken.GetRawText(this.Name, this.InputArgs, this.ImpliedBraces);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a string representation of a token.</summary>
        /// <param name="name">The Content Patcher token name.</param>
        /// <param name="inputArgs">The input arguments passed to the Content Patcher token.</param>
        /// <param name="impliedBraces">Whether the token omits the start/end character patterns because it's in a token-only context.</param>
        private static string GetRawText(string name, LexTokenInput inputArgs, bool impliedBraces)
        {
            StringBuilder str = new StringBuilder();
            if (!impliedBraces)
                str.Append("{{");
            str.Append(name);
            if (inputArgs != null)
            {
                if (!inputArgs.ToString().StartsWith(InternalConstants.NamedInputArgSeparator))
                    str.Append(InternalConstants.PositionalInputArgSeparator);
                str.Append(inputArgs);
            }
            if (!impliedBraces)
                str.Append("}}");
            return str.ToString();
        }
    }
}
