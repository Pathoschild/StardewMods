using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>The mod this token context belongs to.</summary>
        private readonly string Mod;
        
        /// <summary>The available global tokens.</summary>
        private readonly TokenManager GlobalContext;

        /// <summary>The standard self-contained tokens.</summary>
        private readonly GenericTokenContext StandardContext = new GenericTokenContext();

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
        /// <param name="mod">The mod this token context belongs to.</param>
        /// <param name="tokenManager">Manages the available global tokens.</param>
        public ModTokenContext(string mod, TokenManager tokenManager)
        {
            this.Mod = mod;
            this.GlobalContext = tokenManager;
            this.Contexts = new IContext[] { this.GlobalContext, this.StandardContext, this.DynamicContext };
        }

        /// <summary>Add a standard token to the context.</summary>
        /// <param name="token">The config token to add.</param>
        public void Add(IToken token)
        {
            if (token.Name.Contains(InternalConstants.InputArgSeparator))
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because input arguments aren't supported ({InternalConstants.InputArgSeparator} character).");
            if (this.GlobalContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because there's a global token with that name.");
            if (this.StandardContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"The '{token.Name}' token is already registered.");

            this.GlobalContext.AddModToken(this.Mod, token);
            this.StandardContext.Tokens[token.Name] = token;
        }

        /// <summary>Add a dynamic token value to the context.</summary>
        /// <param name="tokenValue">The token to add.</param>
        public void Add(DynamicTokenValue tokenValue)
        {
            // validate
            string plainTokenName = tokenValue.Name.Substring(tokenValue.Name.IndexOf('/') + 1);
            if (this.GlobalContext.Contains(plainTokenName, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{plainTokenName}' token because there's a global token with that name.");
            if (this.StandardContext.Contains(plainTokenName, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{plainTokenName}' dynamic token because there's a config token with that name.");

            // get (or create) token
            if (!this.DynamicContext.Tokens.TryGetValue(tokenValue.Name, out DynamicToken token))
            {
                this.DynamicContext.Save(token = new DynamicToken(tokenValue.Name));
                this.GlobalContext.AddModToken(this.Mod, token);
            }

            // add token value
            token.AddAllowedValues(tokenValue.Value);
            this.DynamicTokenValues.Add(tokenValue);
        }

        /// <summary>Update the current context.</summary>
        public void UpdateContext(IContext globalContext)
        {
            // update config tokens
            foreach (IToken token in this.StandardContext.Tokens.Values)
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
            IEnumerable<string> specific = this.StandardContext.Tokens.Keys.Intersect(tokens);
            foreach (string token in specific)
                this.StandardContext.GetToken(token, false).UpdateContext(this);

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

        /// <summary>Get the underlying tokens.</summary>
        /// <param name="localOnly">Whether to only return local tokens.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public IEnumerable<IToken> GetTokens(bool localOnly, bool enforceContext)
        {
            foreach (IContext context in this.Contexts)
            {
                if (localOnly && context == this.GlobalContext)
                    continue;

                foreach (IToken token in context.GetTokens(enforceContext))
                    yield return token;
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
            return this.GetTokens(localOnly: false, enforceContext: enforceContext);
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
