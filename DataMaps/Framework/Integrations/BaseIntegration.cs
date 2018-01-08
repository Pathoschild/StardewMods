using StardewModdingAPI;

namespace Pathoschild.Stardew.DataMaps.Framework.Integrations
{
    /// <summary>The base implementation for a mod integration.</summary>
    internal abstract class BaseIntegration : IModIntegration
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod's unique ID.</summary>
        protected string ModID { get; }

        /// <summary>An API for fetching metadata about loaded mods.</summary>
        protected IModRegistry ModRegistry { get; }

        /// <summary>Encapsulates monitoring and logging.</summary>
        protected IMonitor Monitor { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>A human-readable name for the mod.</summary>
        public string Label { get; }

        /// <summary>Whether the mod is available.</summary>
        public bool IsLoaded { get; protected set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A human-readable name for the mod.</param>
        /// <param name="modID">The mod's unique ID.</param>
        /// <param name="minVersion">The minimum version of the mod that's supported.</param>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        protected BaseIntegration(string label, string modID, string minVersion, IModRegistry modRegistry, IMonitor monitor)
        {
            // init
            this.Label = label;
            this.ModID = modID;
            this.ModRegistry = modRegistry;
            this.Monitor = monitor;

            // validate mod
            IManifest manifest = modRegistry.Get(this.ModID);
            if (manifest == null)
                return;
            if (manifest.Version.IsOlderThan(minVersion))
            {
                monitor.Log($"Detected {label} {manifest.Version}, but need {minVersion} or later. Data from this mod won't be shown.", LogLevel.Warn);
                return;
            }
            this.IsLoaded = true;
        }

        /// <summary>Get an API for the mod, and show a message if it can't be loaded.</summary>
        /// <typeparam name="TInterface">The API type.</typeparam>
        protected TInterface GetValidatedApi<TInterface>() where TInterface : class
        {
            TInterface api = this.ModRegistry.GetApi<TInterface>(this.ModID);
            if (api == null)
            {
                this.Monitor.Log($"Detected {this.Label}, but couldn't fetch its API. Data from this mod may not be shown.", LogLevel.Warn);
                return null;
            }
            return api;
        }
    }
}
