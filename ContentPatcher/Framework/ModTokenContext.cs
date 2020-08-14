using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
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

        /// <summary>The parent token context.</summary>
        private readonly IContext ParentContext;

        /// <summary>The standard local tokens for this content pack.</summary>
        /// <remarks>This includes config tokens and per-mod core tokens like <see cref="ConditionType.HasFile"/>.</remarks>
        private readonly GenericTokenContext LocalContext;

        /// <summary>The dynamic tokens for this content pack.</summary>
        private readonly GenericTokenContext DynamicContext;

        /// <summary>The dynamic tokens stored in the <see cref="DynamicContext"/>.</summary>
        private readonly InvariantDictionary<ManagedManualToken> DynamicTokens = new InvariantDictionary<ManagedManualToken>();

        /// <summary>The possible values for the <see cref="DynamicTokens"/>.</summary>
        /// <remarks>These must be stored in registration order, since each token value may affect the value of subsequent tokens.</remarks>
        private readonly IList<DynamicTokenValue> DynamicTokenValues = new List<DynamicTokenValue>();

        /// <summary>Maps tokens to those affected by changes to their value in the mod context.</summary>
        private InvariantDictionary<InvariantHashSet> TokenDependents { get; } = new InvariantDictionary<InvariantHashSet>();

        /// <summary>Whether any tokens haven't received a context update yet.</summary>
        private bool HasNewTokens;


        /*********
        ** Public methods
        *********/
        /****
        ** Token management
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="scope">The namespace for tokens specific to this mod.</param>
        /// <param name="parentContext">The parent token context.</param>
        public ModTokenContext(string scope, IContext parentContext)
        {
            this.Scope = scope;
            this.ParentContext = parentContext;
            this.LocalContext = new GenericTokenContext(this.IsModInstalled);
            this.DynamicContext = new GenericTokenContext(this.IsModInstalled);
        }

        /// <summary>Add a standard token to the context.</summary>
        /// <param name="token">The config token to add.</param>
        public void AddLocalToken(IToken token)
        {
            if (token.Scope != this.Scope)
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because its scope '{token.Scope}' doesn't match this mod scope '{this.Scope}.");
            if (token.Name.Contains(InternalConstants.PositionalInputArgSeparator))
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because positional input arguments aren't supported ({InternalConstants.PositionalInputArgSeparator} character).");
            if (this.ParentContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because there's a global token with that name.");
            if (this.LocalContext.Contains(token.Name, enforceContext: false))
                throw new InvalidOperationException($"The '{token.Name}' token is already registered.");

            this.LocalContext.Save(token);
            this.HasNewTokens = true;
        }

        /// <summary>Add a dynamic token value to the context.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="rawValue">The token value to set.</param>
        /// <param name="conditions">The conditions that must match to set this value.</param>
        public void AddDynamicToken(string name, IManagedTokenString rawValue, IEnumerable<Condition> conditions)
        {
            // validate
            if (this.ParentContext.Contains(name, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{name}' token because there's a global token with that name.");
            if (this.LocalContext.Contains(name, enforceContext: false))
                throw new InvalidOperationException($"Can't register a '{name}' dynamic token because there's a config token with that name.");

            // get (or create) token
            if (!this.DynamicTokens.TryGetValue(name, out ManagedManualToken managed))
            {
                managed = new ManagedManualToken(name, this.Scope);
                this.DynamicTokens[name] = managed;
                this.DynamicContext.Save(managed.Token);
            }

            // create token value handler
            var tokenValue = new DynamicTokenValue(managed, rawValue, conditions);
            string[] tokensUsed = tokenValue.GetTokensUsed().ToArray();

            // save value info
            managed.ValueProvider.AddTokensUsed(tokensUsed);
            managed.ValueProvider.AddAllowedValues(rawValue);
            this.DynamicTokenValues.Add(tokenValue);

            // track tokens which should trigger an update to this token
            Queue<string> tokenQueue = new Queue<string>(tokensUsed);
            InvariantHashSet visited = new InvariantHashSet();
            while (tokenQueue.Any())
            {
                // get token name
                string usedTokenName = tokenQueue.Dequeue();
                if (!visited.Add(usedTokenName))
                    continue;

                // if the used token uses other tokens, they may affect the one being added too
                IToken usedToken = this.GetToken(usedTokenName, enforceContext: false);
                foreach (string nextTokenName in usedToken.GetTokensUsed())
                    tokenQueue.Enqueue(nextTokenName);

                // add new token as a dependent of the used token
                if (!this.TokenDependents.TryGetValue(usedToken.Name, out InvariantHashSet used))
                    this.TokenDependents.Add(usedToken.Name, used = new InvariantHashSet());
                used.Add(name);
            }

            // track new token
            this.HasNewTokens = true;
        }

        /// <summary>Update the current context.</summary>
        /// <param name="globalChangedTokens">The global token values which changed.</param>
        public void UpdateContext(InvariantHashSet globalChangedTokens)
        {
            // update local standard tokens
            //
            // Some local tokens may change independently (e.g. Random), so we need to update all
            // standard tokens here.
            bool localTokensChanged = false;
            foreach (IToken token in this.LocalContext.GetTokens(enforceContext: false))
            {
                if (token.IsMutable)
                    localTokensChanged |= token.UpdateContext(this);
            }

            // reset dynamic tokens
            //
            // Since dynamic token values are affected by the order they're defined (e.g. one
            // dynamic token can use the value of another), only updating tokens affected by
            // globalChangedTokens isn't trivial. Instead we track which global tokens are used
            // indirectly through dynamic tokens via AddDynamicToken, and use that to decide which
            // patches to update.
            if (globalChangedTokens.Any() || localTokensChanged || this.HasNewTokens)
            {
                foreach (ManagedManualToken managed in this.DynamicTokens.Values)
                {
                    managed.ValueProvider.SetValue(null);
                    managed.ValueProvider.SetReady(false);
                }

                foreach (DynamicTokenValue tokenValue in this.DynamicTokenValues)
                {
                    tokenValue.UpdateContext(this);
                    if (tokenValue.IsReady && tokenValue.Conditions.All(p => p.IsMatch))
                    {
                        ManualValueProvider valueProvider = tokenValue.ParentToken.ValueProvider;

                        valueProvider.SetValue(tokenValue.Value);
                        valueProvider.SetReady(true);
                    }
                }
            }

            // reset tracking
            this.HasNewTokens = false;
        }

        /// <summary>Get the tokens affected by changes to a given token.</summary>
        /// <param name="token">The token name to check.</param>
        public IEnumerable<string> GetTokensAffectedBy(string token)
        {
            return this.TokenDependents.TryGetValue(token, out InvariantHashSet affectedTokens)
                ? affectedTokens
                : Enumerable.Empty<string>();
        }

        /****
        ** IContext
        ****/
        /// <inheritdoc />
        public bool IsModInstalled(string id)
        {
            return this.ParentContext.IsModInstalled(id);
        }

        /// <inheritdoc />
        public bool Contains(string name, bool enforceContext)
        {
            return this.GetContexts().Any(p => p.Contains(name, enforceContext));
        }

        /// <inheritdoc />
        public IToken GetToken(string name, bool enforceContext)
        {
            foreach (IContext context in this.GetContexts())
            {
                IToken token = context.GetToken(name, enforceContext);
                if (token != null)
                    return token;
            }

            return null;
        }

        /// <inheritdoc />
        public IEnumerable<IToken> GetTokens(bool enforceContext)
        {
            foreach (IContext context in this.GetContexts())
            {
                foreach (IToken token in context.GetTokens(enforceContext))
                    yield return token;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetValues(string name, IInputArguments input, bool enforceContext)
        {
            IToken token = this.GetToken(name, enforceContext);
            return token?.GetValues(input) ?? Enumerable.Empty<string>();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the underlying contexts in priority order.</summary>
        private IEnumerable<IContext> GetContexts()
        {
            yield return this.ParentContext;
            yield return this.LocalContext;
            yield return this.DynamicContext;
        }
    }
}
