using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Lexing
{
    /// <summary>Handles parsing raw strings into tokens.</summary>
    internal class Lexer
    {
        /*********
        ** Fields
        *********/
        /// <summary>A regular expression which matches lexical patterns that split lexical patterns. For example, ':' is a <see cref="LexBitType.PositionalInputArgSeparator"/> pattern that splits a token name and its input arguments. The split pattern is itself a lexical pattern.</summary>
        private readonly Regex LexicalSplitPattern = new Regex(@"({{|}}|:|\|)", RegexOptions.Compiled);


        /*********
        ** Public methods
        *********/
        /// <summary>Break a raw string into its constituent lexical character patterns.</summary>
        /// <param name="rawText">The raw text to tokenize.</param>
        public IEnumerable<LexBit> TokenizeString(string rawText)
        {
            // special cases
            if (rawText == null)
                yield break;
            if (string.IsNullOrWhiteSpace(rawText))
            {
                yield return new LexBit(LexBitType.Literal, rawText);
                yield break;
            }

            // parse
            string[] parts = this.LexicalSplitPattern.Split(rawText);
            foreach (string part in parts)
            {
                if (part == "")
                    continue; // split artifact

                LexBitType type;
                switch (part)
                {
                    case "{{":
                        type = LexBitType.StartToken;
                        break;

                    case "}}":
                        type = LexBitType.EndToken;
                        break;

                    case InternalConstants.PositionalInputArgSeparator:
                        type = LexBitType.PositionalInputArgSeparator;
                        break;

                    case InternalConstants.NamedInputArgSeparator:
                        type = LexBitType.NamedInputArgSeparator;
                        break;

                    default:
                        type = LexBitType.Literal;
                        break;
                }

                yield return new LexBit(type, part);
            }
        }

        /// <summary>Parse a sequence of lexical character patterns into higher-level lexical tokens.</summary>
        /// <param name="rawText">The raw text to tokenize.</param>
        /// <param name="impliedBraces">Whether we're parsing a token context (so the outer '{{' and '}}' are implied); else parse as a tokenizable string which main contain a mix of literal and {{token}} values.</param>
        public IEnumerable<ILexToken> ParseBits(string rawText, bool impliedBraces)
        {
            IEnumerable<LexBit> bits = this.TokenizeString(rawText);
            return this.ParseBits(bits, impliedBraces);
        }

        /// <summary>Parse a sequence of lexical character patterns into higher-level lexical tokens.</summary>
        /// <param name="bits">The lexical character patterns to parse.</param>
        /// <param name="impliedBraces">Whether we're parsing a token context (so the outer '{{' and '}}' are implied); else parse as a tokenizable string which main contain a mix of literal and {{token}} values.</param>
        public IEnumerable<ILexToken> ParseBits(IEnumerable<LexBit> bits, bool impliedBraces)
        {
            return this.ParseBitQueue(new Queue<LexBit>(bits), impliedBraces, trim: false);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse a sequence of lexical character patterns into higher-level lexical tokens.</summary>
        /// <param name="input">The lexical character patterns to parse.</param>
        /// <param name="impliedBraces">Whether we're parsing a token context (so the outer '{{' and '}}' are implied); else parse as a tokenizable string which main contain a mix of literal and {{token}} values.</param>
        /// <param name="trim">Whether the value should be trimmed.</param>
        private IEnumerable<ILexToken> ParseBitQueue(Queue<LexBit> input, bool impliedBraces, bool trim)
        {
            // perform a raw parse
            IEnumerable<ILexToken> RawParse()
            {
                // 'Implied braces' means we're parsing inside a token. This necessarily starts with a token name,
                // optionally followed by input arguments.
                if (impliedBraces)
                {
                    while (input.Any())
                    {
                        // extract token
                        yield return this.ExtractToken(input, impliedBraces: true);
                        if (!input.Any())
                            yield break;

                        // throw error if there's content after the token ends
                        var next = input.Peek();
                        throw new LexFormatException($"Unexpected {next.Type}, expected {LexBitType.Literal}");
                    }
                    yield break;
                }

                // Otherwise this is a tokenizable string which may contain a mix of literal and {{token}} values.
                while (input.Any())
                {
                    LexBit next = input.Peek();
                    switch (next.Type)
                    {
                        // start token
                        case LexBitType.StartToken:
                            yield return this.ExtractToken(input, impliedBraces: false);
                            break;

                        // text/separator outside token
                        case LexBitType.Literal:
                        case LexBitType.PositionalInputArgSeparator:
                        case LexBitType.NamedInputArgSeparator:
                            input.Dequeue();
                            yield return new LexTokenLiteral(next.Text);
                            break;

                        // anything else is invalid
                        default:
                            throw new LexFormatException($"Unexpected {next.Type}, expected {LexBitType.StartToken} or {LexBitType.Literal}");
                    }
                }
            }
            string rawInput = string.Join("", input.Select(p => p.Text));
            LinkedList<ILexToken> tokens;
            try
            {
                tokens = new LinkedList<ILexToken>(RawParse());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error parsing '{rawInput}' as a tokenizable string", ex);
            }

            // normalize literal values
            ISet<LinkedListNode<ILexToken>> removeQueue = new HashSet<LinkedListNode<ILexToken>>(new ObjectReferenceComparer<LinkedListNode<ILexToken>>());
            for (LinkedListNode<ILexToken> node = tokens.First; node != null; node = node.Next)
            {
                // fetch info
                if (!(node.Value is LexTokenLiteral current))
                    continue;
                ILexToken previous = node.Previous?.Value;
                ILexToken next = node.Next?.Value;
                string newText = current.Text;

                // collapse sequential literals
                if (previous is LexTokenLiteral prevLiteral)
                {
                    newText = prevLiteral.Text + newText;
                    removeQueue.Add(node.Previous);
                }

                // trim before/after separator
                if (next?.Type == LexTokenType.TokenInput)
                    newText = newText.TrimEnd();
                if (previous?.Type == LexTokenType.TokenInput)
                    newText = newText.TrimStart();

                // trim whole result
                if (trim && (previous == null || next == null))
                {
                    if (previous == null)
                        newText = newText.TrimStart();
                    if (next == null)
                        newText = newText.TrimEnd();

                    if (newText == "")
                        removeQueue.Add(node);
                }

                // replace value if needed
                if (current.Text != newText)
                    current.MigrateTo(newText);
            }
            foreach (LinkedListNode<ILexToken> entry in removeQueue)
                tokens.Remove(entry);

            // yield result
            return tokens;
        }

        /// <summary>Extract a token from the front of a lexical input queue.</summary>
        /// <param name="input">The input from which to extract a token. The extracted lexical bits will be removed from the queue.</param>
        /// <param name="impliedBraces">Whether we're parsing a token context (so the outer '{{' and '}}' are implied); else parse as a tokenizable string which main contain a mix of literal and {{token}} values.</param>
        /// <returns>Returns the token.</returns>
        public LexTokenToken ExtractToken(Queue<LexBit> input, bool impliedBraces)
        {
            LexBit GetNextAndAssert(string expectedPhrase)
            {
                if (!input.Any())
                    throw new LexFormatException($"Reached end of input, expected {expectedPhrase}.");
                return input.Dequeue();
            }

            // start token
            if (!impliedBraces)
            {
                LexBit startToken = GetNextAndAssert("start of token ('{{')");
                if (startToken.Type != LexBitType.StartToken)
                    throw new LexFormatException($"Unexpected {startToken.Type} at start of token.");
            }

            // extract token name
            LexBit name = GetNextAndAssert("token name");
            if (name.Type != LexBitType.Literal)
                throw new LexFormatException($"Unexpected {name.Type} where token name should be.");

            // extract input arguments if present
            // Note: the positional input argument separator (:) is the 'real' separator between
            // the token name and input arguments, but a token can skip positional arguments and
            // start named arguments directly like {{TokenName |key=value}}. In that case the ':'
            // is implied, and the '|' separator *is* included in the input arguments string.
            LexTokenInput inputArgs = null;
            if (input.Any())
            {
                var next = input.Peek().Type;
                if (next == LexBitType.PositionalInputArgSeparator || next == LexBitType.NamedInputArgSeparator)
                {
                    if (next == LexBitType.PositionalInputArgSeparator)
                        input.Dequeue();
                    inputArgs = this.ExtractInputArguments(input);
                }
            }

            // end token
            if (!impliedBraces)
            {
                LexBit endToken = GetNextAndAssert("end of token ('}}')");
                if (endToken.Type != LexBitType.EndToken)
                    throw new LexFormatException($"Unexpected {endToken.Type} before end of token.");
            }

            return new LexTokenToken(name.Text.Trim(), inputArgs, impliedBraces);
        }

        /// <summary>Extract token input arguments from the front of a lexical input queue.</summary>
        /// <param name="input">The input from which to extract input arguments. The extracted lexical bits will be removed from the queue.</param>
        public LexTokenInput ExtractInputArguments(Queue<LexBit> input)
        {
            // extract input arg parts
            Queue<LexBit> inputArgBits = new Queue<LexBit>();
            int tokenDepth = 0;
            bool reachedEnd = false;
            while (!reachedEnd && input.Any())
            {
                LexBit next = input.Peek();
                switch (next.Type)
                {
                    case LexBitType.StartToken:
                        tokenDepth++;
                        inputArgBits.Enqueue(input.Dequeue());
                        break;

                    case LexBitType.EndToken:
                        tokenDepth--;

                        if (tokenDepth < 0)
                        {
                            reachedEnd = true;
                            break;
                        }

                        inputArgBits.Enqueue(input.Dequeue());
                        break;

                    default:
                        inputArgBits.Enqueue(input.Dequeue());
                        break;
                }
            }

            // parse
            ILexToken[] tokenized = this.ParseBitQueue(inputArgBits, impliedBraces: false, trim: true).ToArray();
            return new LexTokenInput(tokenized);
        }
    }
}
