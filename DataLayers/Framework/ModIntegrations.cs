using Pathoschild.Stardew.Common.Integrations.Automate;
using Pathoschild.Stardew.Common.Integrations.BetterJunimos;
using Pathoschild.Stardew.Common.Integrations.BetterSprinklers;
using Pathoschild.Stardew.Common.Integrations.Cobalt;
using Pathoschild.Stardew.Common.Integrations.LineSprinklers;
using Pathoschild.Stardew.Common.Integrations.PelicanFiber;
using Pathoschild.Stardew.Common.Integrations.PrismaticTools;
using Pathoschild.Stardew.Common.Integrations.SimpleSprinkler;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataLayers.Framework
{
    /// <summary>A container for the supported mod integrations.</summary>
    internal class ModIntegrations
    {
        /*********
        ** Fields
        *********/
        /// <summary>An API for fetching metadata about loaded mods.</summary>
        private readonly IModRegistry ModRegistry;


        /*********
        ** Accessors
        *********/
        /// <summary>Handles access to the Automate mod.</summary>
        public AutomateIntegration Automate { get; }

        /// <summary>Handles access to the Better Junimos mod.</summary>
        public BetterJunimosIntegration BetterJunimos { get; }

        /// <summary>Handles access to the Better Sprinklers mod.</summary>
        public BetterSprinklersIntegration BetterSprinklers { get; }

        /// <summary>Handles access to the Cobalt mod.</summary>
        public CobaltIntegration Cobalt { get; }

        /// <summary>Handles access to the Line Sprinklers mod.</summary>
        public LineSprinklersIntegration LineSprinklers { get; }

        /// <summary>Handles access to the Pelican Fiber mod.</summary>
        public PelicanFiberIntegration PelicanFiber { get; }

        /// <summary>Handles access to the Prismatic Tools mod.</summary>
        public PrismaticToolsIntegration PrismaticTools { get; }

        /// <summary>Handles access to the Simple Sprinkler mod.</summary>
        public SimpleSprinklerIntegration SimpleSprinkler;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="reflection">An API for accessing private code.</param>
        public ModIntegrations(IMonitor monitor, IModRegistry modRegistry, IReflectionHelper reflection)
        {
            this.ModRegistry = modRegistry;

            this.Automate = new AutomateIntegration(modRegistry, monitor);
            this.BetterJunimos = new BetterJunimosIntegration(modRegistry, monitor);
            this.BetterSprinklers = new BetterSprinklersIntegration(modRegistry, monitor);
            this.Cobalt = new CobaltIntegration(modRegistry, monitor);
            this.LineSprinklers = new LineSprinklersIntegration(modRegistry, monitor);
            this.PelicanFiber = new PelicanFiberIntegration(modRegistry, reflection, monitor);
            this.PrismaticTools = new PrismaticToolsIntegration(modRegistry, monitor);
            this.SimpleSprinkler = new SimpleSprinklerIntegration(modRegistry, monitor);
        }

        /// <summary>Get whether a mod has been loaded.</summary>
        /// <param name="uniqueID">The mod's unique ID.</param>
        public bool HasMod(string uniqueID)
        {
            return this.ModRegistry.IsLoaded(uniqueID);
        }
    }
}
