using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.BetterSprinklers
{
    /// <summary>Handles the logic for integrating with the Better Sprinklers Plus mod.</summary>
    internal class BetterSprinklersPlusIntegration : BaseIntegration<IBetterSprinklersPlusApi>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The maximum possible sprinkler radius.</summary>
        public int MaxRadius { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public BetterSprinklersPlusIntegration(IModRegistry modRegistry, IMonitor monitor)
            : base("Better Sprinklers Plus", "com.CodesThings.BetterSprinklersPlus", "2.3.1-unofficial.6-pathoschild", modRegistry, monitor)
        {
            if (base.IsLoaded)
                this.MaxRadius = this.ModApi.GetMaxGridSize();
        }

        /// <summary>Get the configured Sprinkler tiles relative to (0, 0).</summary>
        public IDictionary<int, Vector2[]> GetSprinklerTiles()
        {
            this.AssertLoaded();
            return this.ModApi.GetSprinklerCoverage();
        }
    }
}
