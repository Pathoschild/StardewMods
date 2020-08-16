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
        private readonly InvariantDictionary<ManagedManualToken> LocalTokens = new InvariantDictionary<ManagedManualToken>();


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

            foreach (ManagedManualToken managed in this.LocalTokens.Values)
                managed.ValueProvider.SetReady(false);
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
            return this.LocalTokens.TryGetValue(name, out ManagedManualToken managed)
                ? managed.Token
                : this.LastParentContext?.GetToken(name, enforceContext);
        }

        /// <inheritdoc />
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            foreach (ManagedManualToken managed in this.LocalTokens.Values)
                yield return managed.Token;

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

        /// <summary>Set a local token value.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="value">The token value.</param>
        public void SetLocalValue(string name, string value)
        {
            this.SetLocalValue(name, new LiteralString(value, new LogPathBuilder(nameof(LocalContext), this.Scope, name)));
        }

        /// <summary>Set a local token value.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="value">The token value.</param>
        public void SetLocalValue(string name, ITokenString value)
        {
            // get or create token
            ManagedManualToken managed;
            {
                if (!this.LocalTokens.TryGetValue(name, out managed))
                    this.LocalTokens[name] = managed = new ManagedManualToken(name, this.Scope);
            }

            // update values
            managed.ValueProvider.SetValue(value);
            managed.ValueProvider.SetReady(true);
        }
    }
}
