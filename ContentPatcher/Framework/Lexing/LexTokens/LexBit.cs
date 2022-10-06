using System.Diagnostics;

namespace ContentPatcher.Framework.Lexing.LexTokens
{
    /// <summary>A low-level character pattern within a string/</summary>
    internal readonly record struct LexBit(LexBitType Type, string Text);
}
