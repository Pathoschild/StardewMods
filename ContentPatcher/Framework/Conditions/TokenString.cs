using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A string value optionally containing tokens.</summary>
    internal class TokenString : ITokenString
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying value for <see cref="Value"/>.</summary>
        private string ValueImpl;

        /// <summary>The underlying value for <see cref="IsReady"/>.</summary>
        private bool IsReadyImpl;

        /// <summary>The token names used in the string.</summary>
        private readonly InvariantHashSet TokensUsed = new InvariantHashSet();


        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        public string Raw { get; }

        /// <summary>The lexical tokens parsed from the raw string.</summary>
        public ILexToken[] LexTokens { get; }

        /// <summary>The unrecognised tokens in the string.</summary>
        public InvariantHashSet InvalidTokens { get; } = new InvariantHashSet();

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        public bool HasAnyTokens { get; }

        /// <summary>Whether the token string value may change depending on the context.</summary>
        public bool IsMutable { get; }

        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        public bool IsSingleTokenOnly { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        public string Value => this.ValueImpl;

        /// <summary>Whether all tokens in the value have been replaced.</summary>
        public bool IsReady => this.IsReadyImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="context">The available token context.</param>
        public TokenString(string raw, IContext context)
            : this(lexTokens: new Lexer().ParseBits(raw, impliedBraces: false).ToArray(), context: context) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="raw">The raw token input argument.</param>
        /// <param name="context">The available token context.</param>
        public TokenString(LexTokenInputArg? raw, IContext context)
            : this(lexTokens: raw?.Parts, context: context) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="lexTokens">The lexical tokens parsed from the raw string.</param>
        /// <param name="context">The available token context.</param>
        public TokenString(ILexToken[] lexTokens, IContext context)
        {
            // get lexical tokens
            this.LexTokens = (lexTokens ?? new ILexToken[0])
                .Where(p => !string.IsNullOrWhiteSpace(p.Text)) // ignore whitespace-only tokens
                .ToArray();

            // set raw value
            this.Raw = string.Join("", this.LexTokens.Select(p => p.Text)).Trim();
            if (string.IsNullOrWhiteSpace(this.Raw))
            {
                this.ValueImpl = this.Raw;
                this.IsReadyImpl = true;
                return;
            }

            // extract tokens
            bool isMutable = false;
            bool hasTokens = false;
            foreach (LexTokenToken lexToken in this.LexTokens.OfType<LexTokenToken>())
            {
                hasTokens = true;
                IToken token = context.GetToken(lexToken.Name, enforceContext: false);
                if (token != null)
                {
                    this.TokensUsed.Add(token.Name);
                    isMutable = isMutable || token.IsMutable;
                }
                else
                    this.InvalidTokens.Add(lexToken.Name);
            }

            // set metadata
            this.IsMutable = isMutable;
            this.HasAnyTokens = hasTokens;
            this.IsSingleTokenOnly = this.LexTokens.Length == 1 && this.LexTokens.First().Type == LexTokenType.Token;

            // set initial context
            if (this.InvalidTokens.Any())
                this.IsReadyImpl = false;
            else
                this.GetApplied(context, out this.ValueImpl, out this.IsReadyImpl);
        }

        /// <summary>Update the <see cref="Value"/> with the given tokens.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the value changed.</returns>
        public bool UpdateContext(IContext context)
        {
            if (!this.IsMutable || this.InvalidTokens.Any())
                return false;

            string prevValue = this.Value;
            this.GetApplied(context, out this.ValueImpl, out this.IsReadyImpl);
            return this.Value != prevValue;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.TokensUsed;
        }

        /// <summary>Recursively get the token placeholders from the given lexical tokens.</summary>
        /// <param name="recursive">Whether to scan recursively.</param> 
        public IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive)
        {
            return this.GetTokenPlaceholders(this.LexTokens, recursive);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a new string with tokens substituted.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="result">The input string with tokens substituted.</param>
        /// <param name="isReady">Whether all tokens in the <paramref name="result"/> have been replaced.</param>
        private void GetApplied(IContext context, out string result, out bool isReady)
        {
            bool allReplaced = true;
            StringBuilder str = new StringBuilder();
            foreach (ILexToken lexToken in this.LexTokens)
            {
                switch (lexToken)
                {
                    case LexTokenToken lexTokenToken:
                        IToken token = context.GetToken(lexTokenToken.Name, enforceContext: true);
                        ITokenString input = new TokenString(lexTokenToken.InputArg?.Parts, context);
                        if (token != null)
                        {
                            string[] values = token.GetValues(input).ToArray();
                            str.Append(string.Join(", ", values));
                        }
                        else
                        {
                            allReplaced = false;
                            str.Append(lexToken.Text);
                        }
                        break;

                    default:
                        str.Append(lexToken.Text);
                        break;
                }
            }

            result = str.ToString().Trim();
            isReady = allReplaced;
        }

        /// <summary>Recursively get the token placeholders from the given lexical tokens.</summary>
        /// <param name="lexTokens">The lexical tokens to scan.</param>
        /// <param name="recursive">Whether to scan recursively.</param>
        private IEnumerable<LexTokenToken> GetTokenPlaceholders(ILexToken[] lexTokens, bool recursive)
        {
            if (lexTokens?.Any() != true)
                yield break;

            foreach (ILexToken lexToken in lexTokens)
            {
                if (lexToken is LexTokenToken token)
                {
                    yield return token;

                    if (recursive)
                    {
                        ILexToken[] inputLexTokens = token.InputArg?.Parts;
                        if (inputLexTokens != null)
                        {
                            foreach (LexTokenToken subtoken in this.GetTokenPlaceholders(inputLexTokens, recursive: true))
                                yield return subtoken;
                        }
                    }
                }
            }
        }
    }
}
