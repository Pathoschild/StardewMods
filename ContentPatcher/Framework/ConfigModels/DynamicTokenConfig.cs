using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.ConfigModels;

/// <summary>A user-defined token whose value may depend on other tokens.</summary>
internal class DynamicTokenConfig
{
    /*********
    ** Accessors
    *********/
    /// <summary>The name of the token to set.</summary>
    public string? Name { get; }

    /// <summary>The value to set.</summary>
    public string? Value { get; }

    /// <summary>If set, include tokens from a dynamic token include file.</summary>
    public string? IncludeFromFile { get; }

    /// <summary>The criteria to apply. See the README for valid values.</summary>
    public InvariantDictionary<string?> When { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="name">The name of the token to set.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="when">The criteria to apply. See the README for valid values.</param>
    public DynamicTokenConfig(string name, string value, string? includeFromFile, InvariantDictionary<string?>? when)
    {
        this.Name = name;
        this.Value = value;
        this.IncludeFromFile = PathUtilities.NormalizePath(includeFromFile);
        this.When = when ?? new();
    }
}
