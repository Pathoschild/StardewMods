using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions;

/// <summary>A string value optionally containing tokens.</summary>
internal class TokenString : IManagedTokenString
{
    /*********
    ** Fields
    *********/
    /// <summary>Handles parsing raw strings into tokens.</summary>
    private static readonly Lexer Lexer = Lexer.Instance;

    /// <summary>The token names used in the string.</summary>
    private readonly IInvariantSet TokensUsed = InvariantSets.Empty;

    /// <summary>Diagnostic info about the contextual instance.</summary>
    private readonly ContextualState State = new();

    /// <summary>Metadata for each lexical token in the string.</summary>
    private readonly TokenStringPart[] Parts;

    /// <summary>The string builder with which to build the value.</summary>
    private StringBuilder? StringBuilder;


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
    public string? Value { get; private set; }

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
        : this(lexTokens: TokenString.Lexer.ParseBits(raw, impliedBraces: false), context: context, path: path) { }

    /// <summary>Construct an instance.</summary>
    /// <param name="lexTokens">The lexical tokens parsed from the raw string.</param>
    /// <param name="context">The available token context.</param>
    /// <param name="path">The path to the value from the root content file.</param>
    public TokenString(IEnumerable<ILexToken> lexTokens, IContext context, LogPathBuilder path)
    {
        this.Path = path.ToString();

        // get lexical tokens
        this.Parts =
            (
                from token in lexTokens
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
        List<string>? tokensUsed = null;
        foreach (LexTokenToken lexToken in this.GetTokenPlaceholders(this.LexTokens, recursive: true))
        {
            hasTokens = true;
            IToken? token = context.GetToken(lexToken.Name, enforceContext: false);
            if (token != null)
            {
                tokensUsed ??= [];
                tokensUsed.Add(token.Name);

                if (!token.IsDeterministicForInput)
                {
                    // If every token is deterministic, then the resulting string must be immutable. If this token
                    // is deterministic but it receives non-deterministic input, the string will be marked mutable
                    // when we recursively check those tokens.
                    isMutable = isMutable || token.IsMutable;
                }
            }
            else
            {
                string? requiredModId = lexToken.GetProviderModId();
                if (!string.IsNullOrWhiteSpace(requiredModId) && !context.IsModInstalled(requiredModId))
                    this.State.AddUnavailableModToken(requiredModId);
                else
                    this.State.AddInvalidToken(lexToken.Name);

                isMutable = true; // can't optimize away the token value if it's invalid
            }
        }

        // set metadata
        if (tokensUsed != null)
            this.TokensUsed = InvariantSets.From(tokensUsed);
        this.IsMutable = isMutable;
        this.HasAnyTokens = hasTokens;
        this.IsSingleTokenOnly = TokenString.GetIsSingleTokenOnly(this.Parts);

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
    public bool ShouldUpdate()
    {
        // skip if immutable
        if (!this.IsMutable)
            return false;

        // skip if we know it's still broken
        if (this.State.InvalidTokens.Any() || this.State.UnavailableModTokens.Any())
            return false;

        // otherwise try to update
        return true;
    }

    /// <inheritdoc />
    public IInvariantSet GetTokensUsed()
    {
        return this.TokensUsed;
    }

    /// <inheritdoc />
    public IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive)
    {
        return this.GetTokenPlaceholders(this.LexTokens, recursive);
    }

    /// <inheritdoc />
    public bool UsesToken(ConditionType token)
    {
        return this.TokensUsed.Contains(token.ToString());
    }

    /// <inheritdoc />
    public bool UsesTokens(IEnumerable<ConditionType> tokens)
    {
        return tokens.Any(this.UsesToken);
    }

    /// <inheritdoc />
    public IContextualState GetDiagnosticState()
    {
        return this.State.Clone();
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return this.IsReady
            ? this.Value
            : this.Raw;
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
        if (!forceUpdate && !this.ShouldUpdate())
            return false;

        // reset
        string? wasValue = this.Value;
        bool wasReady = this.State.IsReady;
        this.State.Reset();

        // update value
        if (this.Parts is [{ LexToken: LexTokenLiteral literal }])
            this.Value = literal.Text;
        else
        {
            StringBuilder str = this.StringBuilder ??= new();
            foreach (TokenStringPart part in this.Parts)
                str.Append(this.TryGetTokenText(context, part, this.State, out string? text) ? text : part.LexToken.ToString());

            this.Value = this.State.IsReady
                ? str.ToString().Trim()
                : null;

            if (this.IsMutable)
                str.Clear();
            else
                this.StringBuilder = null;
        }

        // sanity check (should never happen)
        if (this.Value == null && this.State.IsReady)
            throw new InvalidOperationException($"Could not apply tokens to string '{this.Raw}', but no invalid tokens or errors were reported.");

        return
            this.Value != wasValue
            || this.State.IsReady != wasReady;
    }

    /// <summary>Recursively get the token placeholders from the given lexical tokens.</summary>
    /// <param name="lexTokens">The lexical tokens to scan.</param>
    /// <param name="recursive">Whether to scan recursively.</param>
    private IEnumerable<LexTokenToken> GetTokenPlaceholders(IEnumerable<ILexToken>? lexTokens, bool recursive)
    {
        lexTokens = lexTokens?.ToArray();

        // no tokens
        if (lexTokens?.Any() != true)
            yield break;

        // not recursive
        if (!recursive)
        {
            foreach (LexTokenToken token in lexTokens.OfType<LexTokenToken>())
                yield return token;
            yield break;
        }

        // recursive scan
        Stack<LexTokenToken> stack = new(lexTokens.OfType<LexTokenToken>());
        while (stack.Count > 0)
        {
            LexTokenToken token = stack.Pop();
            yield return token;

            if (token.HasInputArgs())
            {
                foreach (LexTokenToken subToken in token.InputArgs.Parts.OfType<LexTokenToken>())
                    stack.Push(subToken);
            }
        }
    }

    /// <summary>Get the text representation of a token's values.</summary>
    /// <param name="context">Provides access to contextual tokens.</param>
    /// <param name="part">The token string part whose value to fetch.</param>
    /// <param name="state">The context state to update with errors, unavailable tokens, etc.</param>
    /// <param name="text">The text representation, if available.</param>
    /// <returns>Returns true if the token is ready and <paramref name="text"/> was set, else false.</returns>
    private bool TryGetTokenText(IContext context, TokenStringPart part, ContextualState state, [NotNullWhen(true)] out string? text)
    {
        switch (part.LexToken)
        {
            case LexTokenToken lexToken:
                {
                    // get token
                    IToken? token = context.GetToken(lexToken.Name, enforceContext: false);
                    if (token == null)
                    {
                        state.AddInvalidToken(lexToken.Name);
                        text = null;
                        return false;
                    }
                    if (!token.IsReady)
                    {
                        this.State.AddUnreadyToken(lexToken.Name);
                        text = null;
                        return false;
                    }

                    // get token input
                    if (part.Input != null)
                    {
                        part.Input.UpdateContext(context);
                        if (!part.Input.IsReady)
                        {
                            state.MergeFrom(part.Input.GetDiagnosticState());
                            text = null;
                            return false;
                        }
                    }

                    // validate input
                    if (!token.TryValidateInput(part.InputArgs, out string? error))
                    {
                        state.AddError(error);
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
        public TokenString? Input { get; }

        /// <summary>The parsed version of <see cref="Input"/>.</summary>
        public IInputArguments InputArgs { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="lexToken">The underlying lex token.</param>
        /// <param name="input">The parsed version of <see cref="Input"/>.</param>
        public TokenStringPart(ILexToken lexToken, TokenString? input)
        {
            this.LexToken = lexToken;
            this.Input = input;
            this.InputArgs = !string.IsNullOrWhiteSpace(input?.Raw)
                ? new InputArguments(input)
                : InputArguments.Empty;
        }
    }
}
