using System;
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
        /// <summary>The token names used in the string.</summary>
        private readonly InvariantHashSet TokensUsed = new InvariantHashSet();

        /// <summary>Diagnostic info about the contextual instance.</summary>
        private readonly ContextualState State = new ContextualState();


        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        public string Raw { get; }

        /// <summary>The lexical tokens parsed from the raw string.</summary>
        public ILexToken[] LexTokens { get; }

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        public bool HasAnyTokens { get; }

        /// <summary>Whether the token string value may change depending on the context.</summary>
        public bool IsMutable { get; }

        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        public bool IsSingleTokenOnly { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        public string Value { get; private set; }

        /// <summary>Whether all tokens in the value have been replaced.</summary>
        public bool IsReady => this.State.IsReady;


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
                this.Value = this.Raw;
                return;
            }

            // extract tokens
            bool isMutable = false;
            bool hasTokens = false;
            foreach (LexTokenToken lexToken in this.GetTokenPlaceholders(this.LexTokens, recursive: true))
            {
                hasTokens = true;
                IToken token = context.GetToken(lexToken.Name, enforceContext: false);
                if (token != null)
                {
                    this.TokensUsed.Add(token.Name);
                    isMutable = isMutable || token.IsMutable;
                }
                else
                    this.State.AddInvalidTokens(lexToken.Name);
            }

            // set metadata
            this.IsMutable = isMutable;
            this.HasAnyTokens = hasTokens;
            this.IsSingleTokenOnly = this.LexTokens.Length == 1 && this.LexTokens.First().Type == LexTokenType.Token;

            // set initial context
            if (this.State.IsReady)
                this.ForceUpdate(context);
        }

        /// <summary>Update the <see cref="Value"/> with the given tokens.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the value changed.</returns>
        public bool UpdateContext(IContext context)
        {
            if (!this.IsMutable || this.State.InvalidTokens.Any())
                return false;

            return this.ForceUpdate(context);
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

        /// <summary>Get diagnostic info about the contextual instance.</summary>
        public IContextualState GetDiagnosticState()
        {
            return this.State.Clone();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Force a context update.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the context changed.</returns>
        private bool ForceUpdate(IContext context)
        {
            // reset
            string wasValue = this.Value;
            bool wasReady = this.State.IsReady;
            this.State.Reset();

            // reapply
            if (this.TryGetApplied(context, out string value, out InvariantHashSet unknownTokens))
                this.Value = value;
            else
            {
                this.Value = null;
                if (!unknownTokens.Any())
                    throw new InvalidOperationException($"Could not apply tokens to string '{this.Raw}', but no invalid tokens were reported."); // sanity check, should never happen
                this.State.AddUnavailableTokens(unknownTokens.ToArray());
            }

            return
                this.Value != wasValue
                || this.State.IsReady != wasReady;
        }

        /// <summary>Get a new string with tokens substituted.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="result">The input string with tokens substituted.</param>
        /// <param name="unavailableTokens">The tokens which could not be replaced (if any).</param>
        /// <returns>Returns whether all tokens in the <paramref name="result"/> were successfully replaced.</returns>
        private bool TryGetApplied(IContext context, out string result, out InvariantHashSet unavailableTokens)
        {
            StringBuilder str = new StringBuilder();
            unavailableTokens = new InvariantHashSet();
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
                            unavailableTokens.Add(lexTokenToken.Name);
                            str.Append(lexToken.Text);
                        }
                        break;

                    default:
                        str.Append(lexToken.Text);
                        break;
                }
            }

            result = str.ToString().Trim();
            return !unavailableTokens.Any();
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
