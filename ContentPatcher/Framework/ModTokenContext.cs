using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework
{
    /// <summary>Manages the token context for a specific content pack.</summary>
    internal class ModTokenContext : IContext
    {
        /*********
        ** Properties
        *********/
        /// <summary>The available global tokens.</summary>
        private readonly IContext GlobalContext;

        /// <summary>The available player config tokens.</summary>
        private readonly GenericTokenContext ConfigContext = new GenericTokenContext();

        /// <summary>The dynamic tokens.</summary>
        private readonly GenericTokenContext<ManualToken> DynamicContext = new GenericTokenContext<ManualToken>();

        /// <summary>The conditional values which affect dynamic tokens.</summary>
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
        /// <param name="tokenManager">Manages the available global tokens.</param>
        public ModTokenContext(TokenManager tokenManager)
        {
            this.GlobalContext = tokenManager;
            this.Contexts = new[] { this.GlobalContext, this.ConfigContext, this.DynamicContext };
        }

        /// <summary>Add a config token to the context.</summary>
        /// <param name="token">The config token to add.</param>
        public void AddConfig(IToken token)
        {
            if (token.Name.HasSubkey())
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because subkeys aren't supported.");
            if (this.GlobalContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because there's a global token with that name.");
            if (this.ConfigContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"The '{token.Name}' token is already registered.");

            this.ConfigContext.Tokens[token.Name] = token;
        }

        /// <summary>Add a dynamic token value to the context.</summary>
        /// <param name="tokenValue">The token to add.</param>
        public void AddDynamic(DynamicTokenValue tokenValue)
        {
            // validate
            if (this.GlobalContext.Contains(tokenValue.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{tokenValue.Name}' token because there's a global token with that name.");
            if (this.ConfigContext.Contains(tokenValue.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{tokenValue.Name}' dynamic token because there's a config token with that name.");

            // get (or create) token
            if (!this.DynamicContext.Tokens.TryGetValue(tokenValue.Name, out ManualToken token))
                this.DynamicContext.Save(token = new ManualToken(tokenValue.Name));

            // add token value
            foreach (string value in tokenValue.Value)
                token.AllowedValues.Add(value);
            token.CanHaveMultipleValues = token.CanHaveMultipleValues || tokenValue.Value.Count > 1;
            this.DynamicTokenValues.Add(tokenValue);
        }

        /// <summary>Update the current context.</summary>
        public void UpdateContext(IContext globalContext)
        {
            // update config tokens
            foreach (IToken token in this.ConfigContext.Tokens.Values)
                token.UpdateContext(this);

            // reset dynamic tokens
            foreach (ManualToken token in this.DynamicContext.Tokens.Values)
            {
                token.IsValidInContext = false;
                token.Values = null;
            }
            foreach (DynamicTokenValue tokenValue in this.DynamicTokenValues)
            {
                if (tokenValue.Conditions.Values.All(p => p.IsMatch(this)))
                {
                    ManualToken token = this.DynamicContext.Tokens[tokenValue.Name];
                    token.Values = tokenValue.Value;
                    token.IsValidInContext = true;
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
        public bool Contains(TokenName name, bool enforceContext)
        {
            return this.Contexts.Any(p => p.Contains(name, enforceContext));
        }

        /// <summary>Get the underlying token which handles a name.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(TokenName name, bool enforceContext)
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
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Return the values of the matching token, or an empty list if the token doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">The specified token name is null.</exception>
        public IEnumerable<string> GetValues(TokenName name, bool enforceContext)
        {
            IToken token = this.GetToken(name, enforceContext);
            return token?.GetValues(name) ?? Enumerable.Empty<string>();
        }

        /// <summary>Get the tokens that can only contain one value.</summary>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public IEnumerable<IToken> GetSingleValues(bool enforceContext)
        {
            foreach (IContext context in this.Contexts)
            {
                foreach (IToken token in context.GetSingleValues(enforceContext))
                    yield return token;
            }
        }
    }
}
