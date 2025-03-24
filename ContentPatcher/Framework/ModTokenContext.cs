using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework;

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
    private readonly InvariantDictionary<ManagedManualToken> DynamicTokens = new();

    /// <summary>The possible values for the <see cref="DynamicTokens"/>.</summary>
    /// <remarks>These must be stored in registration order, since each token value may affect the value of subsequent tokens.</remarks>
    private readonly List<DynamicTokenValue> DynamicTokenValues = [];

    /// <summary>The alias token names defined for the content pack.</summary>
    private readonly InvariantDictionary<string> AliasTokenNames = new();

    /// <summary>For each dynamic token name, the other token names which may change its values.</summary>
    private InvariantDictionary<MutableInvariantSet> DynamicTokenDependencies { get; } = new();

    /// <summary>For each token name, the dynamic token names whose values it may change.</summary>
    private InvariantDictionary<MutableInvariantSet> DynamicTokenDependents { get; } = new();

    /// <summary>The set of dynamic tokens which are dependencies or dependents for another dynamic token.</summary>
    private MutableInvariantSet InterdependentTokens { get; } = [];

    /// <summary>Whether any tokens haven't received a context update yet.</summary>
    private bool HasNewTokens;


    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public int UpdateTick => this.ParentContext.UpdateTick;


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
        this.LocalContext = new(this.IsModInstalled, () => this.UpdateTick);
        this.DynamicContext = new(this.IsModInstalled, () => this.UpdateTick);
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

    /// <summary>Remove a registered local token.</summary>
    /// <param name="name">The config token to remove.</param>
    public void RemoveLocalToken(string name)
    {
        this.LocalContext.Remove(name);
        this.HasNewTokens = true; // update dynamic tokens
    }

    /// <summary>Add a dynamic token value to the context.</summary>
    /// <param name="name">The token name.</param>
    /// <param name="rawValue">The token value to set.</param>
    /// <param name="conditions">The conditions that must match to set this value.</param>
    public void AddDynamicToken(string name, IManagedTokenString rawValue, Condition[] conditions)
    {
        // validate
        if (this.ParentContext.Contains(name, enforceContext: false))
            throw new InvalidOperationException($"Can't register a '{name}' token because there's a global token with that name.");
        if (this.LocalContext.Contains(name, enforceContext: false))
            throw new InvalidOperationException($"Can't register a '{name}' dynamic token because there's a config token or alias with that name.");

        // get (or create) token
        if (!this.DynamicTokens.TryGetValue(name, out ManagedManualToken? managed))
        {
            managed = new(name, isBounded: true, this.Scope);
            this.DynamicTokens[name] = managed;
            this.DynamicContext.Save(managed.Token);
        }

        // create token value handler
        var tokenValue = new DynamicTokenValue(managed, rawValue, conditions);
        IInvariantSet tokensUsed = tokenValue.GetTokensUsed().GetWithout(name);

        // save value info
        managed.ValueProvider.AddTokensUsed(tokensUsed);
        managed.ValueProvider.AddAllowedValues(rawValue);
        if (tokenValue.IsReady)
        {
            managed.ValueProvider.SetValue(tokenValue.Value);
            managed.ValueProvider.SetReady(true);
        }
        this.DynamicTokenValues.Add(tokenValue);

        // track token dependencies
        if (tokensUsed.Any())
        {
            if (!this.DynamicTokenDependencies.TryGetValue(name, out MutableInvariantSet? dependencies))
                this.DynamicTokenDependencies[name] = dependencies = [];

            Queue<string> tokenQueue = new(tokensUsed);
            while (tokenQueue.Any())
            {
                // track token => dependency
                string dependency = tokenQueue.Dequeue();
                if (!dependencies.Add(dependency))
                    continue;

                // track dependency => token
                {
                    if (!this.DynamicTokenDependents.TryGetValue(dependency, out MutableInvariantSet? dependents))
                        this.DynamicTokenDependents[dependency] = dependents = [];

                    dependents.Add(name);
                }

                // track dynamic token inter-dependencies
                if (this.DynamicTokens.ContainsKey(dependency))
                {
                    this.InterdependentTokens.Add(dependency);
                    this.InterdependentTokens.Add(name);
                }

                // queue indirect dependencies
                IInvariantSet? indirect = this.GetToken(dependency, enforceContext: false)?.GetTokensUsed();
                if (indirect?.Count > 0)
                {
                    foreach (string nextTokenName in indirect)
                        tokenQueue.Enqueue(nextTokenName);
                }
            }
        }

        // track new token
        this.HasNewTokens = true;
    }

    /// <summary>Add an alias token name to the context.</summary>
    /// <param name="alias">The custom token name.</param>
    /// <param name="actual">The token name to reference.</param>
    public void AddAliasTokenName(string alias, string actual)
    {
        this.AliasTokenNames.Add(alias, actual);
        this.HasNewTokens = true; // update dynamic tokens
    }

    /// <summary>Get the actual name referenced by a token alias.</summary>
    /// <param name="tokenName">The token name to resolve.</param>
    /// <returns>Returns the resolved token name, or the input token name if it's not an alias.</returns>
    public string ResolveAlias(string tokenName)
    {
        return this.AliasTokenNames.GetValueOrDefault(tokenName, tokenName);
    }

    /// <summary>Update the current context.</summary>
    /// <param name="globalChangedTokens">The global token values which changed.</param>
    public void UpdateContext(IInvariantSet globalChangedTokens)
    {
        bool resetDynamicTokens = this.HasNewTokens;
        MutableInvariantSet? updateDynamicTokens = null;

        // update local standard tokens
        // Some local tokens may change independently (e.g. Random), so we need to update all
        // of them here.
        foreach (IToken token in this.LocalContext.GetTokens(enforceContext: false))
        {
            if (token.IsMutable && token.UpdateContext(this))
            {
                if (!resetDynamicTokens && this.DynamicTokenDependents.TryGetValue(token.Name, out MutableInvariantSet? dependents))
                {
                    updateDynamicTokens ??= [];
                    updateDynamicTokens.AddRange(dependents);
                }
            }
        }

        // update dynamic tokens
        if (this.DynamicTokens.Any())
        {
            // find dynamic tokens affected for global token changes
            if (!resetDynamicTokens)
            {
                foreach (string token in globalChangedTokens)
                {
                    if (this.DynamicTokenDependents.TryGetValue(token, out MutableInvariantSet? dependents))
                    {
                        updateDynamicTokens ??= [];
                        updateDynamicTokens.AddRange(dependents);
                    }
                }
            }

            // trigger a full reset if interdependent tokens changed
            //
            // Since dynamic token values are affected by the order they're defined (e.g. one
            // dynamic token can use the value of another), only updating the values that
            // changed isn't trivial. Instead Content Patcher will track which global tokens
            // were used indirectly when deciding which patches to update.
            if (!resetDynamicTokens && updateDynamicTokens != null && this.InterdependentTokens.Count > 0)
            {
                foreach (string token in updateDynamicTokens)
                {
                    resetDynamicTokens = this.InterdependentTokens.Contains(token);
                    if (resetDynamicTokens)
                        break;
                }
            }

            // update dynamic tokens
            if (resetDynamicTokens || updateDynamicTokens != null)
            {
                foreach ((string name, ManagedManualToken managed) in this.DynamicTokens)
                {
                    if (resetDynamicTokens || updateDynamicTokens!.Contains(name))
                    {
                        managed.ValueProvider.SetValue(InvariantSet.Empty);
                        managed.ValueProvider.SetReady(false);
                    }
                }

                foreach (DynamicTokenValue tokenValue in this.DynamicTokenValues)
                {
                    if (!resetDynamicTokens && !updateDynamicTokens!.Contains(tokenValue.Name))
                        continue;

                    tokenValue.UpdateContext(this);
                    if (tokenValue.IsReady && tokenValue.Conditions.All(p => p.IsMatch))
                    {
                        ManualValueProvider valueProvider = tokenValue.ParentToken.ValueProvider;

                        valueProvider.SetValue(tokenValue.Value);
                        valueProvider.SetReady(true);
                    }
                }
            }
        }

        // reset tracking
        this.HasNewTokens = false;
    }

    /// <summary>Get the tokens which may affect the given token's values.</summary>
    /// <param name="token">The token name to check.</param>
    public IInvariantSet GetTokensWhichAffect(string token)
    {
        return this.DynamicTokenDependencies.TryGetValue(token, out MutableInvariantSet? tokens)
            ? tokens.GetImmutable()
            : InvariantSet.Empty;
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
    public IToken? GetToken(string name, bool enforceContext)
    {
        string targetName = this.ResolveAlias(name);

        foreach (IContext context in this.GetContexts())
        {
            IToken? token = context.GetToken(targetName, enforceContext);
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
    public IInvariantSet GetValues(string name, IInputArguments input, bool enforceContext)
    {
        IToken? token = this.GetToken(name, enforceContext);
        return token?.GetValues(input) ?? InvariantSets.Empty;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get the underlying contexts in priority order.</summary>
    private IEnumerable<IContext> GetContexts()
    {
        return [this.ParentContext, this.LocalContext, this.DynamicContext];
    }
}
