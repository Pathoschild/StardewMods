using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.SpaceCore;

/// <summary>Handles the logic for integrating with the SpaceCore mod.</summary>
internal class SpaceCoreIntegration : BaseIntegration<ISpaceCoreApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public SpaceCoreIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("SpaceCore", "spacechase0.SpaceCore", "1.25.0", modRegistry, monitor) { }

    /// <inheritdoc cref="ISpaceCoreApi.GetCustomSkills" />
    public string[] GetCustomSkills()
    {
        return
            this.SafelyCallApi(
                api => api.GetCustomSkills(),
                "getting custom skills"
            )
            ?? [];
    }

    /// <inheritdoc cref="ISpaceCoreApi.GetExperienceForCustomSkill" />
    public int GetExperienceForCustomSkill(Farmer farmer, string skill)
    {
        return this.SafelyCallApi(
            api => api.GetExperienceForCustomSkill(farmer, skill),
            $"getting skill XP for skill '{skill}'"
        );
    }

    /// <inheritdoc cref="ISpaceCoreApi.GetDisplayNameOfCustomSkill" />
    public string GetDisplayNameOfCustomSkill(string skill)
    {
        return
            this.SafelyCallApi(
                api => api.GetDisplayNameOfCustomSkill(skill),
                $"getting display name for skill '{skill}'"
            )
            ?? skill;
    }
}
