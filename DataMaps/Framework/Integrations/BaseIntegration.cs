using StardewModdingAPI;

namespace Pathoschild.Stardew.DataMaps.Framework.Integrations
{
    /// <summary>The base implementation for a mod integration.</summary>
    internal abstract class BaseIntegration : IModIntegration
    {
        /*********
        ** Properties
        *********/
        /// <summary>A human-readable name for the mod.</summary>
        protected string Label { get; }

        /// <summary>Encapsulates monitoring and logging.</summary>
        protected IMonitor Monitor { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>The mod's unique ID.</summary>
        public string ModID { get; }

        /// <summary>The minimum version of the mod that's supported.</summary>
        public ISemanticVersion MinimumVersion { get; }

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
            this.MinimumVersion = new SemanticVersion(minVersion);
            this.Monitor = monitor;

            // validate mod
            IManifest manifest = modRegistry.Get(this.ModID);
            if (manifest == null)
                return;
            if (manifest.Version.IsOlderThan(this.MinimumVersion))
            {
                monitor.Log($"Detected {label} {manifest.Version}, but need {this.MinimumVersion} or later. Data from this mod won't be shown.", LogLevel.Warn);
                return;
            }
            this.IsLoaded = true;
        }
    }
}
