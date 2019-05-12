using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Manages the token context for a specific content pack.</summary>
    internal class ModTokenContext : IContext
    {
        /*********
        ** Fields
        *********/
        /// <summary>The namespace for tokens specific to this mod.</summary>
        private readonly string Scope;

        /// <summary>The available global tokens.</summary>
        private readonly TokenManager GlobalContext;

        /// <summary>The available local standard tokens.</summary>
        private readonly GenericTokenContext LocalContext = new GenericTokenContext();

        /// <summary>The dynamic tokens whose value depends on <see cref="DynamicTokenValues"/>.</summary>
        private readonly GenericTokenContext<DynamicToken> DynamicContext = new GenericTokenContext<DynamicToken>();

        /// <summary>The conditional values used to set the values of <see cref="DynamicContext"/> tokens.</summary>
        private readonly IList<DynamicTokenValue> DynamicTokenValues = new List<DynamicTokenValue>();

        /// <summary>The underlying token contexts in priority order.</summary>
        private readonly IContext[] Contexts;


        /*********
        ** Public methods
        *********/
        /****
        ** Token management
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="scope">The namespace for tokens specific to this mod.</param>
        /// <param name="tokenManager">Manages the available global tokens.</param>
        public ModTokenContext(string scope, TokenManager tokenManager)
        {
            this.Scope = scope;
            this.GlobalContext = tokenManager;
            this.Contexts = new IContext[] { this.GlobalContext, this.LocalContext, this.DynamicContext };
        }

        /// <summary>Add a standard token to the context.</summary>
        /// <param name="token">The config token to add.</param>
        public void Add(IToken token)
        {
            if (token.Scope != this.Scope)
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because its scope '{token.Scope}' doesn't match this mod scope '{this.Scope}.");
            if (token.Name.Contains(InternalConstants.InputArgSeparator))
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because input arguments aren't supported ({InternalConstants.InputArgSeparator} character).");
            if (this.GlobalContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because there's a global token with that name.");
            if (this.LocalContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"The '{token.Name}' token is already registered.");

            this.LocalContext.Tokens[token.Name] = token;
        }

        /// <summary>Add a dynamic token value to the context.</summary>
        /// <param name="tokenValue">The token to add.</param>
        public void Add(DynamicTokenValue tokenValue)
        {
            // validate
            if (this.GlobalContext.Contains(tokenValue.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{tokenValue}' token because there's a global token with that name.");
            if (this.LocalContext.Contains(tokenValue.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{tokenValue.Name}' dynamic token because there's a config token with that name.");

            // get (or create) token
            if (!this.DynamicContext.Tokens.TryGetValue(tokenValue.Name, out DynamicToken token))
                this.DynamicContext.Save(token = new DynamicToken(tokenValue.Name, this.Scope));

            // add token value
            token.AddAllowedValues(tokenValue.Value);
            this.DynamicTokenValues.Add(tokenValue);

            // track what tokens this token uses, if any
            Queue<string> tokenQueue = new Queue<string>();
            foreach (Condition condition in tokenValue.Conditions)
            {
                foreach (LexTokenToken lexToken in condition.Input.GetTokenPlaceholders(recursive: true))
                    tokenQueue.Enqueue(lexToken.Name);
                foreach (LexTokenToken lexToken in condition.Values.GetTokenPlaceholders(recursive: true))
                    tokenQueue.Enqueue(lexToken.Name);
            }
            foreach (LexTokenToken lexToken in tokenValue.Value.GetTokenPlaceholders(recursive: true))
                tokenQueue.Enqueue(lexToken.Name);
            while (tokenQueue.Count > 0)
            {
                string tokenName = tokenQueue.Dequeue();
                IToken curToken = this.GlobalContext.GetToken(tokenName, enforceContext: false);

                if (curToken is DynamicToken dynamicToken)
                {
                    foreach (string val in dynamicToken.GetAllowedValues(null))
                    {
                        TokenString tokenStr = new TokenString(val, this.GlobalContext);
                        foreach (LexTokenToken lexToken in tokenValue.Value.GetTokenPlaceholders(recursive: true))
                            tokenQueue.Enqueue(lexToken.Name);
                    }
                }
                else
                {
                    if (!this.GlobalContext.BasicTokensUsedBy.TryGetValue(curToken.Name, out InvariantHashSet used))
                        this.GlobalContext.BasicTokensUsedBy.Add(curToken.Name, used = new InvariantHashSet());

                    if (!used.Contains(tokenValue.Name))
                        used.Add(tokenValue.Name);
                }
            }
        }

        /// <summary>Update the current context.</summary>
        public void UpdateContext(IContext globalContext)
        {
            // update config tokens
            foreach (IToken token in this.LocalContext.Tokens.Values)
                token.UpdateContext(this);

            // reset dynamic tokens
            foreach (DynamicToken token in this.DynamicContext.Tokens.Values)
                token.SetReady(false);
            foreach (DynamicTokenValue tokenValue in this.DynamicTokenValues)
            {
                tokenValue.UpdateContext(this);
                if (tokenValue.IsReady && tokenValue.Conditions.All(p => p.IsMatch(this)))
                {
                    DynamicToken token = this.DynamicContext.Tokens[tokenValue.Name];
                    token.SetValue(tokenValue.Value);
                    token.SetReady(true);
                }
            }
        }

        /// <summary>Update the current context for certain tokens.</summary>
        public void UpdateSpecificContext(InvariantHashSet tokens)
        {
            // update config tokens
            IEnumerable<string> specific = this.LocalContext.Tokens.Keys.Intersect(tokens);
            foreach (string token in specific)
                this.LocalContext.GetToken(token, false).UpdateContext(this);

            // reset dynamic tokens
            // TODO: Only update relevant dynamic tokens
            foreach (DynamicToken token in this.DynamicContext.Tokens.Values)
                token.SetReady(false);
            foreach (DynamicTokenValue tokenValue in this.DynamicTokenValues)
            {
                tokenValue.UpdateContext(this);
                if (tokenValue.IsReady && tokenValue.Conditions.All(p => p.IsMatch(this)))
                {
                    DynamicToken token = this.DynamicContext.Tokens[tokenValue.Name];
                    token.SetValue(tokenValue.Value);
                    token.SetReady(true);
                }
            }
        }

        /****
        ** IContext
        ****/
        /// <summary>Get whether the context contains the given token.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public bool Contains(string name, bool enforceContext)
        {
            return this.Contexts.Any(p => p.Contains(name, enforceContext));
        }

        /// <summary>Get the underlying token which handles a name.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(string name, bool enforceContext)
        {
            foreach (IContext context in this.Contexts)
            {
                IToken token = context.GetToken(name, enforceContext);
                if (token != null)
                    return token;
            }

            return null;
        }

        /// <summary>Get the underlying tokens.</summary>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            foreach (IContext context in this.Contexts)
            {
                foreach (IToken token in context.GetTokens(enforceContext))
                    yield return token;
            }
        }

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Return the values of the matching token, or an empty list if the token doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">The specified token name is null.</exception>
        public IEnumerable<string> GetValues(string name, ITokenString input, bool enforceContext)
        {
            IToken token = this.GetToken(name, enforceContext);
            return token?.GetValues(input) ?? Enumerable.Empty<string>();
        }
    }
}
