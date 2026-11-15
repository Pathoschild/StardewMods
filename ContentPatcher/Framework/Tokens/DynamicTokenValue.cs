using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens;

/// <summary>A conditional value for a dynamic token.</summary>
internal class DynamicTokenValue : DynamicTokenContextual
{
    /*********
    ** Accessors
    *********/
    /// <summary>The token for which this value is registered.</summary>
    public ManagedManualToken ParentToken { get; }

    /// <inheritdoc/>
    public override string Name => this.ParentToken.ValueProvider.Name;

    /// <summary>The token value to set.</summary>
    public ITokenString Value { get; }

    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="parentToken">The token whose value to set.</param>
    /// <param name="value">The token value to set.</param>
    /// <param name="conditions">The conditions that must match to set this value.</param>
    public DynamicTokenValue(ManagedManualToken parentToken, IManagedTokenString value, Condition[] conditions, DynamicTokenContextual? parent) : base(conditions, parent)
    {
        this.ParentToken = parentToken;
        this.Value = value;
        this.Contextuals.Add(value);
    }
}
