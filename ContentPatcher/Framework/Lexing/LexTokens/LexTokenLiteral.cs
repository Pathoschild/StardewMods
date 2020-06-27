namespace ContentPatcher.Framework.Lexing.LexTokens
{
    /// <summary>A lexical token representing a literal string value.</summary>
    internal class LexTokenLiteral : ILexToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The lexical token type.</summary>
        public LexTokenType Type { get; } = LexTokenType.Literal;

        /// <summary>A text representation of the lexical token.</summary>
        public string Text { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="text">The literal text value.</param>
        public LexTokenLiteral(string text)
        {
            this.MigrateTo(text);
        }

        /// <summary>Apply changes for a format migration.</summary>
        /// <param name="text">The new text to set.</param>
        public void MigrateTo(string text)
        {
            this.Text = text;
        }

        /// <summary>Get a text representation of the lexical token.</summary>
        public override string ToString()
        {
            return this.Text;
        }
    }
}
