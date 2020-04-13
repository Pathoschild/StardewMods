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
    internal class TokenString : IManagedTokenString
    {
        /*********
        ** Fields
        *********/
        /// <summary>The token names used in the string.</summary>
        private readonly InvariantHashSet TokensUsed = new InvariantHashSet();

        /// <summary>Diagnostic info about the contextual instance.</summary>
        private readonly ContextualState State = new ContextualState();

        /// <summary>Metadata for each lexical token in the string.</summary>
        private readonly TokenStringPart[] Parts;


        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        public string Raw { get; }

        /// <summary>The lexical tokens parsed from the raw string.</summary>
        public IEnumerable<ILexToken> LexTokens => this.Parts.Select(p => p.LexToken);

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
        public TokenString(LexTokenInputArg raw, IContext context)
            : this(lexTokens: raw?.Parts, context: context) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="lexTokens">The lexical tokens parsed from the raw string.</param>
        /// <param name="context">The available token context.</param>
        public TokenString(ILexToken[] lexTokens, IContext context)
        {
            // get lexical tokens
            this.Parts =
                (
                    from token in (lexTokens ?? new ILexToken[0])
                    select new TokenStringPart
                    {
                        LexToken = token,
                        Input = token is LexTokenToken lexToken && lexToken.InputArg != null
                            ? new TokenString(lexToken.InputArg?.Parts, context)
                            : null
                    }
                )
                .ToArray();

            // set raw value
            this.Raw = string.Join("", this.Parts.Select(p => p.LexToken.Text)).Trim();
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
                {
                    string requiredModId = lexToken.GetProviderModId();
                    if (!string.IsNullOrWhiteSpace(requiredModId) && !context.IsModInstalled(requiredModId))
                        this.State.AddUnavailableModTokens(requiredModId);
                    else
                        this.State.AddInvalidTokens(lexToken.Name);

                    isMutable = true; // can't optimize away the token value if it's invalid
                }
            }

            // set metadata
            this.IsMutable = isMutable;
            this.HasAnyTokens = hasTokens;
            this.IsSingleTokenOnly = this.Parts.Length == 1 && this.Parts.First().LexToken.Type == LexTokenType.Token;

            // set initial context
            if (this.State.IsReady)
                this.UpdateContext(context, forceUpdate: true);
        }

        /// <summary>Update the <see cref="Value"/> with the given tokens.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the value changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.UpdateContext(context, forceUpdate: false);
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

        /// <summary>Get whether a token string uses the given token.</summary>
        /// <param name="tokens">The token to find.</param>
        public bool UsesTokens(params ConditionType[] tokens)
        {
            return tokens.Any(token => this.TokensUsed.Contains(token.ToString()));
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
        /// <param name="forceUpdate">Update even if the state hasn't changed.</param>
        /// <returns>Returns whether the context changed.</returns>
        private bool UpdateContext(IContext context, bool forceUpdate)
        {
            if (!forceUpdate && (!this.IsMutable || this.State.InvalidTokens.Any() || this.State.UnavailableModTokens.Any()))
                return false;

            // reset
            string wasValue = this.Value;
            bool wasReady = this.State.IsReady;
            this.State.Reset();

            // update value
            InvariantHashSet unavailableTokens = new InvariantHashSet();
            InvariantHashSet errors = new InvariantHashSet();
            {
                StringBuilder str = new StringBuilder();
                foreach (TokenStringPart part in this.Parts)
                    str.Append(this.TryGetTokenText(context, part, unavailableTokens, errors, out string text) ? text : part.LexToken.Text);

                this.Value = !unavailableTokens.Any() && !errors.Any()
                    ? str.ToString().Trim()
                    : null;
            }

            // reapply
            if (this.Value == null)
            {
                if (!unavailableTokens.Any() && !errors.Any())
                    throw new InvalidOperationException($"Could not apply tokens to string '{this.Raw}', but no invalid tokens or errors were reported."); // sanity check, should never happen

                this.State.AddUnreadyTokens(unavailableTokens.ToArray());
                this.State.AddErrors(errors.ToArray());
            }

            return
                this.Value != wasValue
                || this.State.IsReady != wasReady;
        }

        /// <summary>Recursively get the token placeholders from the given lexical tokens.</summary>
        /// <param name="lexTokens">The lexical tokens to scan.</param>
        /// <param name="recursive">Whether to scan recursively.</param>
        private IEnumerable<LexTokenToken> GetTokenPlaceholders(IEnumerable<ILexToken> lexTokens, bool recursive)
        {
            lexTokens = lexTokens?.ToArray();

            if (lexTokens?.Any() != true)
                yield break;

            foreach (LexTokenToken token in lexTokens.OfType<LexTokenToken>())
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

        /// <summary>Get the text representation of a token's values.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="part">The token string part whose value to fetch.</param>
        /// <param name="unavailableTokens">A list of unavailable or unready token names to update if needed.</param>
        /// <param name="errors">The errors which occurred (if any).</param>
        /// <param name="text">The text representation, if available.</param>
        /// <returns>Returns true if the token is ready and <paramref name="text"/> was set, else false.</returns>
        private bool TryGetTokenText(IContext context, TokenStringPart part, InvariantHashSet unavailableTokens, InvariantHashSet errors, out string text)
        {
            switch (part.LexToken)
            {
                case LexTokenToken lexToken:
                    {
                        // get token
                        IToken token = context.GetToken(lexToken.Name, enforceContext: true);
                        if (token == null || !token.IsReady)
                        {
                            unavailableTokens.Add(lexToken.Name);
                            text = null;
                            return false;
                        }

                        // get token input
                        TokenString input = part.Input;
                        if (input != null)
                        {
                            // update input
                            input.UpdateContext(context);

                            // check for unavailable tokens
                            string[] unavailableInputTokens = input
                                .GetTokensUsed()
                                .Where(name => context.GetToken(name, enforceContext: true)?.IsReady != true)
                                .ToArray();
                            if (unavailableInputTokens.Any())
                            {
                                foreach (string tokenName in unavailableInputTokens)
                                    unavailableTokens.Add(tokenName);
                                text = null;
                                return false;
                            }
                        }

                        // validate input
                        if (!token.TryValidateInput(input, out string error))
                        {
                            errors.Add(error);
                            text = null;
                            return false;
                        }

                        // get text representation
                        string[] values = token.GetValues(input).ToArray();
                        text = string.Join(", ", values);
                        return true;
                    }

                default:
                    text = part.LexToken.Text;
                    return true;
            }
        }

        /// <summary>A lexical token component in the token string.</summary>
        private class TokenStringPart
        {
            /// <summary>The underlying lex token.</summary>
            public ILexToken LexToken { get; set; }

            /// <summary>The input to the lex token, if it's a <see cref="LexTokenType.Token"/>.</summary>
            public TokenString Input { get; set; }
        }
    }
}
