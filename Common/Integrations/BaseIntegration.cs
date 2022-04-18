#nullable disable

using System;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations
{
    /// <summary>The base implementation for a mod integration.</summary>
    internal abstract class BaseIntegration : IModIntegration
    {
        /*********
        ** Fields
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
        public virtual bool IsLoaded { get; }


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
            IManifest manifest = modRegistry.Get(this.ModID)?.Manifest;
            if (manifest == null)
                return;
            if (manifest.Version.IsOlderThan(minVersion))
            {
                monitor.Log($"Detected {label} {manifest.Version}, but need {minVersion} or later. Disabled integration with this mod.", LogLevel.Warn);
                return;
            }
            this.IsLoaded = true;
        }

        /// <summary>Get an API for the mod, and show a message if it can't be loaded.</summary>
        /// <typeparam name="TApi">The API type.</typeparam>
        protected TApi GetValidatedApi<TApi>()
            where TApi : class
        {
            TApi api = this.ModRegistry.GetApi<TApi>(this.ModID);
            if (api == null)
            {
                this.Monitor.Log($"Detected {this.Label}, but couldn't fetch its API. Disabled integration with this mod.", LogLevel.Warn);
                return null;
            }
            return api;
        }

        /// <summary>Assert that the integration is loaded.</summary>
        /// <exception cref="InvalidOperationException">The integration isn't loaded.</exception>
        protected void AssertLoaded()
        {
            if (!this.IsLoaded)
                throw new InvalidOperationException($"The {this.Label} integration isn't loaded.");
        }
    }

    /// <summary>The base implementation for a mod integration.</summary>
    /// <typeparam name="TApi">The API type.</typeparam>
    internal abstract class BaseIntegration<TApi> : BaseIntegration
        where TApi : class
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod's public API.</summary>
        public TApi ModApi { get; }

        /// <inheritdoc />
        public override bool IsLoaded => this.ModApi != null;


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
            : base(label, modID, minVersion, modRegistry, monitor)
        {
            if (base.IsLoaded)
                this.ModApi = this.GetValidatedApi<TApi>();
        }
    }
}
