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
        /// <inheritdoc />
        public string Raw { get; }

        /// <inheritdoc />
        public IEnumerable<ILexToken> LexTokens => this.Parts.Select(p => p.LexToken);

        /// <inheritdoc />
        public bool HasAnyTokens { get; }

        /// <inheritdoc />
        public bool IsMutable { get; }

        /// <inheritdoc />
        public bool IsSingleTokenOnly { get; }

        /// <inheritdoc />
        public string Value { get; private set; }

        /// <inheritdoc />
        public bool IsReady => this.State.IsReady;

        /// <inheritdoc />
        public string Path { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance from raw text. This constructor bypasses migrations, so it should not be used to parse any values from a content pack.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="context">The available token context.</param>
        /// <param name="path">The path to the value from the root content file.</param>
        public TokenString(string raw, IContext context, LogPathBuilder path)
            : this(lexTokens: new Lexer().ParseBits(raw, impliedBraces: false).ToArray(), context: context, path: path) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="inputArgs">The raw token input arguments.</param>
        /// <param name="context">The available token context.</param>
        /// <param name="path">The path to the value from the root content file.</param>
        public TokenString(LexTokenInput inputArgs, IContext context, LogPathBuilder path)
            : this(lexTokens: inputArgs?.Parts, context: context, path: path) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="lexTokens">The lexical tokens parsed from the raw string.</param>
        /// <param name="context">The available token context.</param>
        /// <param name="path">The path to the value from the root content file.</param>
        public TokenString(ILexToken[] lexTokens, IContext context, LogPathBuilder path)
        {
            // get lexical tokens
            this.Parts =
                (
                    from token in (lexTokens ?? new ILexToken[0])
                    let input = token is LexTokenToken lexToken && lexToken.HasInputArgs()
                        ? new TokenString(lexToken.InputArgs.Parts, context, path.With(lexToken.Name))
                        : null
                    select new TokenStringPart(token, input)
                )
                .ToArray();

            // set raw value
            this.Raw = string.Join("", this.Parts.Select(p => p.LexToken)).Trim();
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
            this.IsSingleTokenOnly = TokenString.GetIsSingleTokenOnly(this.Parts);
            this.Path = path.ToString();

            // set initial context
            if (this.State.IsReady)
                this.UpdateContext(context, forceUpdate: true);
        }

        /// <inheritdoc />
        public bool UpdateContext(IContext context)
        {
            return this.UpdateContext(context, forceUpdate: false);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetTokensUsed()
        {
            return this.TokensUsed;
        }

        /// <inheritdoc />
        public IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive)
        {
            return this.GetTokenPlaceholders(this.LexTokens, recursive);
        }

        /// <inheritdoc />
        public bool UsesTokens(params ConditionType[] tokens)
        {
            return tokens.Any(token => this.TokensUsed.Contains(token.ToString()));
        }

        /// <inheritdoc />
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
                    str.Append(this.TryGetTokenText(context, part, unavailableTokens, errors, out string text) ? text : part.LexToken.ToString());

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

                if (recursive && token.HasInputArgs())
                {
                    foreach (LexTokenToken subtoken in this.GetTokenPlaceholders(token.InputArgs.Parts, recursive: true))
                        yield return subtoken;
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
                        if (part.Input != null)
                        {
                            // update input
                            part.Input.UpdateContext(context);

                            // check for unavailable tokens
                            string[] unavailableInputTokens = part.Input
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
                        if (!token.TryValidateInput(part.InputArgs, out string error))
                        {
                            errors.Add(error);
                            text = null;
                            return false;
                        }

                        // get text representation
                        string[] values = token.GetValues(part.InputArgs).ToArray();
                        text = string.Join(", ", values);
                        return true;
                    }

                default:
                    text = part.LexToken.ToString();
                    return true;
            }
        }

        /// <summary>Get whether a string only contains a single root token, ignoring literal whitespace.</summary>
        /// <param name="parts">The lexical string parts.</param>
        private static bool GetIsSingleTokenOnly(TokenStringPart[] parts)
        {
            bool foundToken = false;
            foreach (TokenStringPart part in parts)
            {
                // non-token content
                if (part.LexToken is LexTokenLiteral literal)
                {
                    if (!string.IsNullOrWhiteSpace(literal.Text))
                        return false;
                }

                // multiple tokens
                else if (part.LexToken.Type == LexTokenType.Token)
                {
                    if (foundToken)
                        return false;
                    foundToken = true;
                }

                // non-token content
                else
                    return false;
            }

            return foundToken;
        }

        /// <summary>A lexical token component in the token string.</summary>
        private class TokenStringPart
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The underlying lex token.</summary>
            public ILexToken LexToken { get; }

            /// <summary>The input to the lex token, if it's a <see cref="LexTokenType.Token"/>.</summary>
            public TokenString Input { get; }

            /// <summary>The parsed version of <see cref="Input"/>.</summary>
            public IInputArguments InputArgs { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="lexToken">The underlying lex token.</param>
            /// <param name="input">The parsed version of <see cref="Input"/>.</param>
            public TokenStringPart(ILexToken lexToken, TokenString input)
            {
                this.LexToken = lexToken;
                this.Input = input;
                this.InputArgs = new InputArguments(input);
            }
        }
    }
}
