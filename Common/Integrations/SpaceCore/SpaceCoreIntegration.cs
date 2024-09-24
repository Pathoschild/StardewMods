using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.SpaceCore
{
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
    }
}
