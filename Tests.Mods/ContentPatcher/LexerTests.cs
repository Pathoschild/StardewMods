using System.Linq;
using System.Text;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using FluentAssertions;
using NUnit.Framework;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher
{
    /// <summary>Unit tests for <see cref="Lexer"/>.</summary>
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    class LexerTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Test that <see cref="Lexer.TokenizeString"/> generates the expected low-level structure.</summary>
        /// <remarks>
        /// 
        /// </remarks>
        [TestCase(
            "", // input
            "[]", // bits
            "[]" // tokens
        )]
        [TestCase(
            "   ",
            "[   ]",
            "[   ]"
        )]
        [TestCase(
            "boop",
            "[boop]",
            "[boop]"
        )]
        [TestCase(
            "  assets/boop.png  ",
            "[  assets/boop.png  ]",
            "[  assets/boop.png  ]"
        )]
        [TestCase(
            "  inner whitespace with ~!@#$%^&*()_=[]{}':;\"',.<>/ characters",
            "[  inner whitespace with ~!@#$%^&*()_=[]{}']<PositionalInputArgSeparator::>[;\"',.<>/ characters]",
            "[  inner whitespace with ~!@#$%^&*()_=[]{}':;\"',.<>/ characters]"
        )]
        [TestCase(
            "{{token}}",
            "<StartToken:{{>[token]<EndToken:}}>",
            "<Token:token>"
        )]
        [TestCase(
            " {{  token }}   ",
            "[ ]<StartToken:{{>[  token ]<EndToken:}}>[   ]",
            "[ ]<Token:token>[   ]"
        )]
        [TestCase(
            " {{  Relationship : Abigail }}   ",
            "[ ]<StartToken:{{>[  Relationship ]<PositionalInputArgSeparator::>[ Abigail ]<EndToken:}}>[   ]",
            "[ ]<Token:Relationship input=<input:[Abigail]>>[   ]"
        )]
        public void ParseTokenizedString(string input, string expectedBits, string expectedTokens)
        {
            // act
            this.GetLexInfo(input, false, out LexBit[] bits, out ILexToken[] tokens);

            // assert
            this.GetComparableShorthand(bits).Should().Be(expectedBits);
            this.GetComparableShorthand(tokens).Should().Be(expectedTokens);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get lexical info for an input string.</summary>
        /// <param name="input">The raw text to tokenize.</param>
        /// <param name="impliedBraces">Whether we're parsing a token context (so the outer '{{' and '}}' are implied); else parse as a tokenizable string which main contain a mix of literal and {{token}} values.</param>
        /// <param name="bits">The low-level lexical character patterns.</param>
        /// <param name="tokens">The higher-level lexical tokens.</param>
        private void GetLexInfo(string input, bool impliedBraces, out LexBit[] bits, out ILexToken[] tokens)
        {
            Lexer lexer = new Lexer();
            bits = lexer.TokenizeString(input).ToArray();
            tokens = lexer.ParseBits(bits, impliedBraces).ToArray();
        }

        /// <summary>Get a comparable representation for a sequence of lexical bits for comparison in unit tests.</summary>
        /// <param name="input">The lexical bits to represent.</param>
        private string GetComparableShorthand(params LexBit[] input)
        {
            return string.Join("", input.Select(bit => bit.Type == LexBitType.Literal ? $"[{bit.Text}]" : $"<{bit.Type}:{bit.Text}>"));
        }

        /// <summary>Get a comparable representation for a sequence of lexical tokens for comparison in unit tests.</summary>
        /// <param name="input">The lexical bits to represent.</param>
        private string GetComparableShorthand(params ILexToken[] input)
        {
            return string.Join("", input.Select(bit =>
            {
                switch (bit)
                {
                    case LexTokenToken token:
                        {
                            StringBuilder str = new StringBuilder();

                            str.Append($"<{token.Type}:{token.Name}");
                            if (token.InputArgs != null)
                                str.Append($" input={this.GetComparableShorthand(token.InputArgs)}");

                            str.Append(">");
                            return str.ToString();
                        }

                    case LexTokenInput inputArgs:
                        return $"<input:{this.GetComparableShorthand(inputArgs.Parts)}>";

                    case LexTokenLiteral _:
                        return $"[{bit}]";

                    default:
                        return $"<{bit.Type}:{bit}>";
                }
            }));
        }
    }
}
