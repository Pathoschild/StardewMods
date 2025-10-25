using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.Automate;

/// <summary>Handles the logic for integrating with the Automate mod.</summary>
internal class AutomateIntegration : BaseIntegration<IAutomateApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public AutomateIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("Automate", "Pathoschild.Automate", "1.11.0", modRegistry, monitor) { }

    /// <summary>Get the status of machines in a tile area. This is a specialized API for Data Layers and similar mods.</summary>
    /// <param name="location">The location for which to display data.</param>
    /// <param name="tileArea">The tile area for which to display data.</param>
    public IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea)
    {
        return
            this.SafelyCallApi(
                api => api.GetMachineStates(location, tileArea),
                "Failed fetching machine states from {0}."
            )
            ?? new Dictionary<Vector2, int>();
    }
}
