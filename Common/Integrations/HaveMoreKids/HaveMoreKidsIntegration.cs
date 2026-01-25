using StardewModdingAPI;
using StardewValley.Characters;

namespace Pathoschild.Stardew.Common.Integrations.HaveMoreKids;

/// <summary>Handles the logic for integrating with the Extra Machine Config mod.</summary>
internal class HaveMoreKidsIntegration : BaseIntegration<IHaveMoreKidsAPI>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public HaveMoreKidsIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("HaveMoreKids", "mushymato.HaveMoreKids", "1.1.0", modRegistry, monitor) { }

    /// <inheritdoc cref="IHaveMoreKidsAPI.GetDaysToNextChildGrowth" />
    public int? GetDaysToNextChildGrowth(Child kid)
    {
        return this.SafelyCallApi(
            api => api.GetDaysToNextChildGrowth(kid),
            $"Failed getting days to next growth for '{kid.Name}' from '{{0}}'."
        );
    }

    /// <inheritdoc cref="IHaveMoreKidsAPI.GetChildBirthdayString" />
    public string? GetChildBirthdayString(Child kid)
    {
        return this.SafelyCallApi(
            api => api.GetChildBirthdayString(kid),
            $"Failed getting birthday string for '{kid.Name}' from '{{0}}'."
        );
    }
}
