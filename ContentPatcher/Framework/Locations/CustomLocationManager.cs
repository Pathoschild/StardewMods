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
            // get locations with duplicated names
            var duplicateLocations =
                (
                    from location in this.CustomLocations
                    from name in new[] { location.Name }.Concat(location.MigrateLegacyNames)
                    select new { name, location }
                )
                .GroupBy(p => p.name)
                .Select(p => new { Name = p.Key, Locations = p.Select(p => p.location).ToArray() })
                .Where(p => p.Locations.Length > 1);

            // warn and disable duplicates
            foreach (var group in duplicateLocations.OrderByIgnoreCase(p => p.Name))
            {
                // log error
                string[] contentPackNames = group.Locations.Select(p => p.ContentPack.Manifest.Name).OrderByIgnoreCase(p => p).Distinct().ToArray();
                string error = contentPackNames.Length > 1
                    ? $"The '{group.Name}' location was added by multiple content packs ('{string.Join("', '", contentPackNames)}')."
                    : $"The '{group.Name}' location was added multiple times in the '{contentPackNames[0]}' content pack.";
                error += " This location won't be added to the game.";
                this.Monitor.Log(error, LogLevel.Error);

                // disable locations
                foreach (var location in group.Locations)
                    location.Disable(error);
            }

            // remove disabled locations
            foreach (var pair in this.CustomLocationsByMapPath.Where(p => !p.Value.IsEnabled).ToArray())
                this.CustomLocationsByMapPath.Remove(pair.Key);
        }

        /// <summary>Add all locations to the game, if applicable.</summary>
        /// <param name="saveLocations">The locations in the save file being loaded.</param>
        /// <param name="gameLocations">The locations that will be populated from the save data.</param>
        public void Apply(IList<GameLocation> saveLocations, IList<GameLocation> gameLocations)
        {
            // get valid locations
            CustomLocationData[] customLocations = this.CustomLocations.Where(p => p.IsEnabled).ToArray();
            if (!customLocations.Any())
                return;

            // migrate legacy locations
            this.MigrateLegacyLocations(saveLocations, gameLocations, customLocations);

            // log location list
            if (this.Monitor.IsVerbose)
            {
                var locationsByPack = customLocations
                    .OrderByIgnoreCase(p => p.Name)
                    .GroupBy(p => p.ContentPack.Manifest.Name)
                    .OrderByIgnoreCase(p => p.Key);

                this.Monitor.VerboseLog($"Adding {customLocations.Length} locations:\n- {string.Join("\n- ", locationsByPack.Select(group => $"{group.Key}: {string.Join(", ", group.Select(p => p.Name))}"))}.");
            }

            // add locations
            foreach (CustomLocationData location in customLocations)
                gameLocations.Add(new GameLocation(mapPath: location.PublicMapPath, name: location.Name));
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
            string[] migrateLegacyNames = config?.MigrateLegacyNames?.Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray() ?? new string[0];
            parsed = new CustomLocationData(name, fromMapFile, migrateLegacyNames, contentPack);

            // validate name
            if (string.IsNullOrWhiteSpace(name))
                return Fail($"the {nameof(config.Name)} field is required.", parsed, out error);
            if (!Regex.IsMatch(name, "^[a-z0-9_]+", RegexOptions.IgnoreCase))
                return Fail($"the {nameof(config.Name)} field can only contain alphanumeric or underscore characters.", parsed, out error);
            if (!name.StartsWith("Custom_"))
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


        /// <summary>Migrate locations which match <see cref="CustomLocationData.MigrateLegacyNames"/> to the new location name.</summary>
        /// <param name="saveLocations">The locations in the save file being loaded.</param>
        /// <param name="gameLocations">The locations that will be populated from the save data.</param>
        /// <param name="customLocations">The custom location data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SMAPI.CommonErrors", "AvoidNetField", Justification = "This is deliberate due to the Name property being readonly.")]
        private void MigrateLegacyLocations(IList<GameLocation> saveLocations, IList<GameLocation> gameLocations, CustomLocationData[] customLocations)
        {
            // get name => save location lookup
            var saveLocationsByName = new Dictionary<string, GameLocation>();
            foreach (GameLocation location in saveLocations)
                saveLocationsByName[location.NameOrUniqueName] = location;

            // get legacy name => custom location lookup
            IDictionary<string, CustomLocationData> migrateLegacyNames =
                (
                    from location in customLocations
                    from legacyName in location.MigrateLegacyNames
                    select new { location, legacyName }
                )
                .ToDictionary(p => p.legacyName, p => p.location, StringComparer.OrdinalIgnoreCase);

            // ignore legacy names which match a vanilla location
            foreach (GameLocation location in gameLocations)
            {
                if (migrateLegacyNames.TryGetValue(location.NameOrUniqueName, out CustomLocationData customLocation))
                {
                    this.Monitor.Log($"Ignored legacy location name '{location.NameOrUniqueName}' added by content pack '{customLocation.ContentPack.Manifest.Name}' because it matches a non-Content Patcher location.", LogLevel.Warn);
                    migrateLegacyNames.Remove(location.NameOrUniqueName);
                }
            }

            // migrate save data
            foreach (var pair in migrateLegacyNames)
            {
                string legacyName = pair.Key;
                CustomLocationData customLocation = pair.Value;

                if (!saveLocationsByName.ContainsKey(customLocation.Name) && saveLocationsByName.TryGetValue(legacyName, out GameLocation location))
                {
                    this.Monitor.Log($"Renamed saved location '{legacyName}' to '{customLocation.Name}' for the '{customLocation.ContentPack.Manifest.Name}' content pack.", LogLevel.Info);
                    location.name.Value = customLocation.Name;
                }
            }
        }
    }
}
