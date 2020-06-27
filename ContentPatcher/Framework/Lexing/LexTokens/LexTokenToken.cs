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
        public LexTokenType Type { get; }

        /// <summary>A text representation of the lexical token.</summary>
        public string Text { get; }

        /// <summary>The Content Patcher token name.</summary>
        public string Name { get; }

        /// <summary>The input arguments passed to the Content Patcher token.</summary>
        public LexTokenInput InputArgs { get; }

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
            this.Type = LexTokenType.Token;
            this.Text = LexTokenToken.GetRawText(name, inputArgs, impliedBraces);
            this.Name = name;
            this.InputArgs = inputArgs;
            this.ImpliedBraces = impliedBraces;
        }

        /// <summary>Get the unique ID of the mod which provides this token, if applicable.</summary>
        public string GetProviderModId()
        {
            string[] nameParts = this.Name.Split(new[] { InternalConstants.ModTokenSeparator }, 2, StringSplitOptions.None);
            return nameParts.Length == 2
                ? nameParts[0].Trim()
                : null;
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
                if (!inputArgs.Text.StartsWith(InternalConstants.NamedInputArgSeparator))
                    str.Append(InternalConstants.PositionalInputArgSeparator);
                str.Append(inputArgs.Text);
            }
            if (!impliedBraces)
                str.Append("}}");
            return str.ToString();
        }

        /// <summary>Get a string representation of the lexical token.</summary>
        public override string ToString()
        {
            return this.Text;
        }
    }
}
