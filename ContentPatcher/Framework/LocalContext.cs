using System.Collections.Generic;
using System.Linq;
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
        private readonly InvariantDictionary<IHigherLevelToken<DynamicToken>> LocalTokens = new InvariantDictionary<IHigherLevelToken<DynamicToken>>();


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

            foreach (DynamicToken token in this.LocalTokens.Values.Select(p => p.Token))
                token.SetReady(false);
        }

        /// <inheritdoc />
        public bool IsModInstalled(string id)
        {
            return this.LastParentContext?.IsModInstalled(id) ?? false;
        }

        /// <inheritdoc />
        public bool Contains(string name, bool enforceContext)
        {
            return this.GetToken(name, enforceContext) != null;
        }

        /// <inheritdoc />
        public IToken GetToken(string name, bool enforceContext)
        {
            return this.LocalTokens.TryGetValue(name, out var token)
                ? token
                : this.LastParentContext?.GetToken(name, enforceContext);
        }

        /// <inheritdoc />
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            foreach (var token in this.LocalTokens.Values)
                yield return token;

            if (this.LastParentContext != null)
            {
                foreach (IToken token in this.LastParentContext.GetTokens(enforceContext))
                    yield return token;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetValues(string name, IInputArguments input, bool enforceContext)
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
            // get or create token
            DynamicToken token;
            {
                if (!this.LocalTokens.TryGetValue(name, out IHigherLevelToken<DynamicToken> wrapper))
                    this.LocalTokens[name] = wrapper = new HigherLevelTokenWrapper<DynamicToken>(new DynamicToken(name, this.Scope));
                token = wrapper.Token;
            }

            // update values
            token.SetValue(value);
            token.SetReady(true);
        }
    }
}
