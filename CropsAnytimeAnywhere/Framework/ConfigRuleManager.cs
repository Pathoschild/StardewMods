using System.Diagnostics.CodeAnalysis;
using StardewValley;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;

/// <summary>Encapsulates access to the configuration rules.</summary>
internal class ConfigRuleManager
{
    /*********
    ** Accessors
    *********/
    /// <summary>The underlying mod configuration.</summary>
    public ModConfig Config { get; private set; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The underlying mod configuration.</param>
    public ConfigRuleManager(ModConfig config)
    {
        this.UpdateConfig(config);
    }

    /// <summary>Update the data when the config changes.</summary>
    /// <param name="config">The new config to apply.</param>
    [MemberNotNull(nameof(Config))]
    public void UpdateConfig(ModConfig config)
    {
        this.Config = config;
    }

    /// <summary>Get whether there are any tillable rules which override the vanilla logic.</summary>
    public bool HasTillableOverrides()
    {
        foreach (TillableRule rule in this.Config.TillableRules)
        {
            if (rule.IsAnyEnabled())
                return true;
        }

        return false;
    }

    /// <summary>Get the plant rules to apply for a location.</summary>
    /// <param name="location">The location containing the plant.</param>
    public PlantRule? GetPlantRule(GameLocation location)
    {
        foreach (PlantRule rule in this.Config.PlantRules)
        {
            if (rule.AppliesTo(location))
                return rule;
        }

        return null;
    }

    /// <summary>Get the tillable rules to apply for a location.</summary>
    /// <param name="location">The location to check.</param>
    public TillableRule? GetTillableRule(GameLocation location)
    {
        foreach (TillableRule rule in this.Config.TillableRules)
        {
            if (rule.AppliesTo(location))
                return rule;
        }

        return null;
    }
}
