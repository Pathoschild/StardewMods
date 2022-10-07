namespace ContentPatcher.Framework.Lexing.LexTokens
{
    /// <summary>A low-level character pattern within a string/</summary>
    /// <param name="Type">The lexical character pattern type.</param>
    /// <param name="Text">The raw matched text.</param>
    internal readonly record struct LexBit(LexBitType Type, string Text);
}
