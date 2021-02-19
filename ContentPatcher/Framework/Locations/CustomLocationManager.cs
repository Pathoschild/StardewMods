using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace ContentPatcher.Framework.Locations
{
    /// <summary>Encapsulates loading custom location data and adding it to the game.</summary>
    internal class CustomLocationManager : IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>The registered locations regardless of validity.</summary>
        private readonly List<CustomLocationData> CustomLocations = new();

        /// <summary>The enabled locations indexed by their normalized map path.</summary>
        private readonly InvariantDictionary<CustomLocationData> CustomLocationsByMapPath = new();

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public CustomLocationManager(IMonitor monitor)
        {
            this.Monitor = monitor;
        }

        /// <summary>Parse a raw custom location model, and validate that it's valid.</summary>
        /// <param name="config">The raw location config model.</param>
        /// <param name="contentPack">The content pack loading the file.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        public bool TryAddCustomLocationConfig(CustomLocationConfig config, IContentPack contentPack, out string error)
        {
            // validate config
            if (!this.TryParseCustomLocationConfig(config, contentPack, out CustomLocationData parsed, out error))
                return false;

            this.CustomLocations.Add(parsed);
            if (parsed.IsEnabled)
                this.CustomLocationsByMapPath[parsed.PublicMapPath] = parsed;

            return true;
        }

        /// <summary>Enforce that custom location maps have a unique name.</summary>
        public void EnforceUniqueNames()
        {
            var duplicateLocations = this.CustomLocations
                .GroupByIgnoreCase(p => p.Name)
                .OrderByIgnoreCase(p => p.Key)
                .Where(p => p.Count() > 1);

            foreach (var group in duplicateLocations)
            {
                // log error
                string[] contentPackNames = group.Select(p => p.ContentPack.Manifest.Name).OrderByIgnoreCase(p => p).Distinct().ToArray();
                string error = contentPackNames.Length > 1
                    ? $"The '{group.Key}' location was added by multiple content packs ('{string.Join("', '", contentPackNames)}')."
                    : $"The '{group.Key}' location was added multiple times in the '{contentPackNames[0]}' content pack.";
                error += " This location won't be added to the game.";
                this.Monitor.Log(error, LogLevel.Error);

                // disable locations
                foreach (var location in group)
                    location.Disable(error);
                this.CustomLocationsByMapPath.Remove(group.Key);
            }
        }

        /// <summary>Add all locations to the game, if applicable.</summary>
        public void Apply()
        {
            // get valid locations
            var locations = this.CustomLocations.Where(p => p.IsEnabled).ToArray();
            if (!locations.Any())
                return;


            // log location list
            if (this.Monitor.IsVerbose)
            {
                var locationsByPack = locations
                    .OrderByIgnoreCase(p => p.Name)
                    .GroupByIgnoreCase(p => p.ContentPack.Manifest.Name)
                    .OrderByIgnoreCase(p => p.Key);

                this.Monitor.VerboseLog($"Adding {locations.Length} locations:\n- {string.Join("\n- ", locationsByPack.Select(group => $"{group.Key}: {string.Join(", ", group.Select(p => p.Name))}"))}.");
            }

            // add locations
            foreach (CustomLocationData location in locations)
                Game1.locations.Add(new GameLocation(mapPath: location.PublicMapPath, name: location.Name));
        }

        /// <inheritdoc />
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return this.CustomLocationsByMapPath.ContainsKey(asset.AssetName);
        }

        /// <inheritdoc />
        public T Load<T>(IAssetInfo asset)
        {
            // not a handled map
            if (!this.CustomLocationsByMapPath.TryGetValue(asset.AssetName, out CustomLocationData location))
                throw new InvalidOperationException($"Unexpected asset name '{asset.AssetName}'.");

            // invalid type
            if (!typeof(Map).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException($"Unexpected attempt to load asset '{asset.AssetName}' as a {typeof(T)} asset instead of {typeof(Map)}.");

            // load asset
            return location.ContentPack.LoadAsset<T>(location.FromMapFile);
        }

        /// <summary>Get the defined custom locations.</summary>
        public IEnumerable<CustomLocationData> GetCustomLocationData()
        {
            return this.CustomLocations;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse a raw custom location model, and validate that it's valid.</summary>
        /// <param name="config">The raw location config model.</param>
        /// <param name="contentPack">The content pack loading the file.</param>
        /// <param name="parsed">The parsed value.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        private bool TryParseCustomLocationConfig(CustomLocationConfig config, IContentPack contentPack, out CustomLocationData parsed, out string error)
        {
            static bool Fail(string reason, CustomLocationData parsed, out string setError)
            {
                setError = reason;
                parsed.Disable(reason);
                return false;
            }

            // read values
            string name = config?.Name?.Trim();
            string fromMapFile = config?.FromMapFile?.Trim();
            parsed = new CustomLocationData(name, fromMapFile, contentPack);

            // validate name
            if (string.IsNullOrWhiteSpace(name))
                return Fail($"the {nameof(config.Name)} field is required.", parsed, out error);
            if (!Regex.IsMatch(name, "^[a-z0-9_]+", RegexOptions.IgnoreCase))
                return Fail($"the {nameof(config.Name)} field can only contain alphanumeric or underscore characters.", parsed, out error);
            if (!name.StartsWith("Custom_", StringComparison.OrdinalIgnoreCase))
                return Fail($"the {nameof(config.Name)} field must be prefixed with 'Custom_' to avoid conflicting with current or future vanilla locations.", parsed, out error);

            // validate map file
            if (string.IsNullOrWhiteSpace(fromMapFile))
                return Fail($"the {nameof(config.FromMapFile)} field is required.", parsed, out error);
            if (!contentPack.HasFile(fromMapFile))
                return Fail($"the {nameof(config.FromMapFile)} field specifies a file '{fromMapFile}' which doesn't exist in the content pack.", parsed, out error);

            // create instance
            error = null;
            return true;
        }
    }
}
