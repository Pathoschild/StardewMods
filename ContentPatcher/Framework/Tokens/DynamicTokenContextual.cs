using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens;

/// <summary>A holder for dynamic token conditions.</summary>
internal class DynamicTokenContextual : IContextual
{
    /// <summary>Namespace used for dynamic token includes</summary>
    internal const string DynamicTokenIncludePrefix = "<CP-DT-Include>";

    /*********
    ** Fields
    *********/
    /// <summary>Backing field for name</summary>
    private readonly string NameImpl = string.Empty;

    /// <summary>The underlying contextual values.</summary>
    protected readonly AggregateContextual Contextuals = new();

    /*********
    ** Accessors
    *********/
    /// <summary>The conditions that must match to set this value.</summary>
    public Condition[] Conditions { get; }

    /// <summary>The name of the token whose value to set.</summary>
    public virtual string Name => this.NameImpl;

    /// <summary>A parent contextual whose conditions must all match as well</summary>
    public DynamicTokenContextual? Parent { get; }

    /// <inheritdoc />
    public bool IsMutable => (this.Parent?.IsMutable ?? false) || this.Contextuals.IsMutable;

    /// <inheritdoc />
    public bool IsReady => (this.Parent?.IsReady ?? true) && this.Contextuals.IsReady;

    /*********
    ** Protected methods
    *********/
    /// <summary>Create a new DynamicTokenContextual</summary>
    /// <param name="conditions">list of conditions</param>
    /// <param name="parent">parent contextual</param>
    protected DynamicTokenContextual(Condition[] conditions, DynamicTokenContextual? parent)
    {
        this.Parent = parent;
        this.Conditions = conditions;
        this.Contextuals.Add(this.Conditions);
    }
    /*********
    ** Public methods
    *********/
    /// <summary>Create a new DynamicTokenContextual that is named</summary>
    /// <param name="name">name of this context</param>
    /// <param name="conditions">list of conditions</param>
    /// <param name="parent">parent contextual</param>
    public DynamicTokenContextual(string name, Condition[] conditions, DynamicTokenContextual? parent) : this(conditions, parent)
    {
        this.NameImpl = string.Concat(DynamicTokenIncludePrefix, name);
    }

    /// <summary>Check that all conditions of this contextual as it's parent matches</summary>
    /// <returns>match state</returns>
    public bool AllConditionsMatch()
    {
        return (this.Parent?.AllConditionsMatch() ?? true) && this.Conditions.All(p => p.IsMatch);
    }

    /// <inheritdoc />
    public bool UpdateContext(IContext context)
    {
        return this.Contextuals.UpdateContext(context);
    }

    /// <inheritdoc />
    public IInvariantSet GetTokensUsed()
    {
        MutableInvariantSet tokensUsed = this.Contextuals.GetMutableTokensUsed();
        if (this.Parent != null)
        {
            tokensUsed.AddRange(this.Parent.Contextuals.GetMutableTokensUsed());
        }
        return tokensUsed.Lock();
    }

    /// <inheritdoc />
    public IContextualState GetDiagnosticState()
    {
        return this.Contextuals.GetDiagnosticState();
    }
}
