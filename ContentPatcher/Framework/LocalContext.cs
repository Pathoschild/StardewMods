using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>A context which provides temporary tokens specific to the current patch or field.</summary>
    internal class LocalContext : IContext
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod namespace in which the token is accessible.</summary>
        private readonly string Scope;

        /// <summary>The parent context that provides non-patch-specific tokens.</summary>
        private IContext LastParentContext;

        /// <summary>The local token values.</summary>
        private readonly InvariantDictionary<DynamicToken> LocalTokens = new InvariantDictionary<DynamicToken>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="scope">The mod namespace in which the token is accessible.</param>
        /// <param name="parentContext">The initial parent context that provides non-patch-specific tokens, if any.</param>
        public LocalContext(string scope, IContext parentContext = null)
        {
            this.Scope = scope;
            this.LastParentContext = parentContext;
        }

        /****
        ** IContext
        ****/
        /// <summary>Update the patch context.</summary>
        /// <param name="parentContext">The parent context that provides non-patch-specific tokens.</param>
        public void Update(IContext parentContext)
        {
            this.LastParentContext = parentContext;

            foreach (DynamicToken token in this.LocalTokens.Values)
                token.SetReady(false);
        }

        /// <summary>Get whether a mod is installed.</summary>
        /// <param name="id">The mod ID.</param>
        public bool IsModInstalled(string id)
        {
            return this.LastParentContext?.IsModInstalled(id) ?? false;
        }

        /// <summary>Get whether the context contains the given token.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public bool Contains(string name, bool enforceContext)
        {
            return this.GetToken(name, enforceContext) != null;
        }

        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(string name, bool enforceContext)
        {
            return this.LocalTokens.TryGetValue(name, out DynamicToken token)
                ? token
                : this.LastParentContext?.GetToken(name, enforceContext);
        }

        /// <summary>Get the underlying tokens.</summary>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            foreach (DynamicToken token in this.LocalTokens.Values)
                yield return token;

            if (this.LastParentContext != null)
            {
                foreach (IToken token in this.LastParentContext.GetTokens(enforceContext))
                    yield return token;
            }
        }

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Return the values of the matching token, or an empty list if the token doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        public IEnumerable<string> GetValues(string name, ITokenString input, bool enforceContext)
        {
            IToken token = this.GetToken(name, enforceContext);
            return token?.GetValues(input) ?? new string[0];
        }

        /// <summary>Set a dynamic token value.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="value">The token value.</param>
        public void SetLocalValue(string name, string value)
        {
            this.SetLocalValue(name, new LiteralString(value));
        }

        /// <summary>Set a dynamic token value.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="value">The token value.</param>
        public void SetLocalValue(string name, ITokenString value)
        {
            if (!this.LocalTokens.TryGetValue(name, out DynamicToken token))
                this.LocalTokens[name] = token = new DynamicToken(name, this.Scope);

            token.SetValue(value);
            token.SetReady(true);
        }
    }
}
