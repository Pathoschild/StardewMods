using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.DataMaps.Framework.Integrations.BetterSprinklers
{
    /// <summary>Handles the logic for integrating with the Better Sprinklers mod.</summary>
    internal class BetterSprinklersIntegration : BaseIntegration
    {
        /*********
        ** Properties
        *********/
        /// <summary>An API for accessing private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Gets the Better Sprinklers mod's config model.</summary>
        private readonly Func<object> GetModConfig;


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
        /// <param name="reflection">An API for accessing private code.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public BetterSprinklersIntegration(IModRegistry modRegistry, IReflectionHelper reflection, IMonitor monitor)
            : base("Better Sprinklers", "Speeder.BetterSprinklers", "2.3.1-pathoschild-update.3", modRegistry, monitor)
        {
            this.Reflection = reflection;
            if (!this.IsLoaded)
                return;

            try
            {
                object modEntry = this.GetModEntry(modRegistry);
                this.GetModConfig = this.GetModConfigDelegate(modEntry, reflection);
                this.MaxRadius = reflection.GetField<int>(modEntry, "MaxGridSize").GetValue() / 2;
            }
            catch (Exception ex)
            {
                monitor.Log("Detected Better Sprinklers, but couldn't integrate with it. Make sure you have the latest version of both mods.", LogLevel.Warn);
                monitor.Log(ex.ToString(), LogLevel.Trace);
                this.IsLoaded = false;
            }
        }

        /// <summary>Get the configured Sprinkler tiles relative to (0, 0).</summary>
        public IDictionary<int, Vector2[]> GetSprinklerTiles()
        {
            IDictionary<int, Vector2[]> radii = new Dictionary<int, Vector2[]>();
            foreach (KeyValuePair<int, int[,]> entry in this.GetSprinklerShapes(this.GetModConfig, this.Reflection))
                radii[entry.Key] = this.GetTiles(entry.Value).ToArray();
            return radii;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the mod's entry class.</summary>
        /// <param name="modRegistryApi">An API for fetching metadata about loaded mods.</param>
        private object GetModEntry(IModRegistry modRegistryApi)
        {
            // get the mod registry instance
            object modRegistry = modRegistryApi.GetType().GetField("Registry", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(modRegistryApi);
            if (modRegistry == null)
                throw new InvalidOperationException($"Can't access the private {modRegistryApi.GetType().FullName}::Registry field.");

            // get the mod list
            IList modList = modRegistry.GetType().GetField("Mods", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(modRegistry) as IList;
            if (modList == null)
                throw new InvalidOperationException($"Can't access the private {modRegistry.GetType().FullName}::Mods field.");

            // get the mod entry class
            object modEntry = null;
            foreach (object entry in modList)
            {
                // get manifest
                IManifest manifest = entry.GetType().GetProperty("Manifest")?.GetValue(entry) as IManifest;
                if (manifest == null)
                    throw new InvalidOperationException($"Can't access the {entry.GetType().FullName}::Manifest property.");

                // ignore wrong entry
                if (manifest.UniqueID != this.ModID)
                    continue;

                // get mod entry class
                modEntry = entry.GetType().GetProperty("Mod")?.GetValue(entry);
                if (modEntry == null)
                    throw new InvalidOperationException($"Can't access the {entry.GetType().FullName}::Mod property.");
            }
            if (modEntry == null)
                throw new InvalidOperationException($"Can't find the mod entry in {modRegistry.GetType().FullName}::Mods.");

            return modEntry;
        }

        /// <summary>Get the mod's config model.</summary>
        /// <param name="modEntry">The mod's entry class.</param>
        private Func<object> GetModConfigDelegate(object modEntry, IReflectionHelper reflection)
        {
            IReflectedField<object> configField = reflection.GetField<object>(modEntry, "Config");
            return () => configField.GetValue();
        }

        /// <summary>Get the current Better Sprinklers configuration.</summary>
        /// <param name="getConfigModel">Gets the mod's config model.</param>
        /// <param name="reflection">An API for accessing private code.</param>
        private IDictionary<int, int[,]> GetSprinklerShapes(Func<object> getConfigModel, IReflectionHelper reflection)
        {
            // get config model
            object configModel = getConfigModel();
            if (configModel == null)
                throw new InvalidOperationException($"The Better Sprinklers config model is unexpectedly null.");

            // get sprinkler shapes
            IDictionary<int, int[,]> sprinklerShapes = reflection.GetProperty<Dictionary<int, int[,]>>(configModel, "SprinklerShapes").GetValue();
            if (sprinklerShapes == null)
                throw new InvalidOperationException("The Better Sprinklers config model is unexpectedly null.");

            return sprinklerShapes;
        }

        /// <summary>Get relative tile coordinates from a Better Sprinklers shape grid.</summary>
        /// <param name="grid">The shape grid.</param>
        private IEnumerable<Vector2> GetTiles(int[,] grid)
        {
            int radiusX = grid.GetLength(0) / 2;
            int radiusY = grid.GetLength(1) / 2;

            for (int gridX = 0, x = -radiusX; x <= radiusX; gridX++, x++)
            {
                for (int gridY = 0, y = -radiusY; y <= radiusY; gridY++, y++)
                {
                    if (grid[gridX, gridY] > 0)
                        yield return new Vector2(x, y);
                }
            }
        }
    }
}
