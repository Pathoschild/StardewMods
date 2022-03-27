using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;

namespace ContentPatcher.Framework.Locations
{
    /// <summary>Encapsulates loading custom location data and adding it to the game.</summary>
    [SuppressMessage("ReSharper", "CommentTypo", Justification = "'TMXL' is not a typo.")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "'TMXL' is not a typo.")]
    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "'TMXL' is not a typo.")]
    internal class CustomLocationManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>A pattern which matches a valid location name.</summary>
        private static readonly Regex ValidNamePattern = new("^[a-z0-9_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>The registered locations regardless of validity.</summary>
        private readonly List<CustomLocationData> CustomLocations = new();

        /// <summary>The enabled locations indexed by their normalized map path.</summary>
        private readonly Dictionary<IAssetName, CustomLocationData> CustomLocationsByMapPath = new();

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The content API with which to parse asset keys.</summary>
        private readonly IGameContentHelper ContentHelper;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="contentHelper">The content API with which to parse asset keys.</param>
        public CustomLocationManager(IMonitor monitor, IGameContentHelper contentHelper)
        {
            this.Monitor = monitor;
            this.ContentHelper = contentHelper;
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
                this.CustomLocationsByMapPath[this.ContentHelper.ParseAssetName(parsed.PublicMapPath)] = parsed;

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
                    group location by name into locationGroup
                    select new { Name = locationGroup.Key, Locations = locationGroup.ToArray() }
                )
                .Where(group => group.Locations.Length > 1);

            // warn and disable duplicates
            foreach (var group in duplicateLocations.OrderByHuman(p => p.Name))
            {
                // log error
                string[] contentPackNames = group.Locations.Select(p => p.ModName).OrderByHuman().Distinct().ToArray();
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
        /// <param name="saveLocations">The locations in the save file being loaded, or <c>null</c> when creating a new save.</param>
        /// <param name="gameLocations">The locations that will be populated from the save data.</param>
        public void Apply(IList<GameLocation> saveLocations, IList<GameLocation> gameLocations)
        {
            // get valid locations
            CustomLocationData[] customLocations = this.CustomLocations.Where(p => p.IsEnabled).ToArray();
            if (!customLocations.Any())
                return;

            // migrate legacy locations
            List<MigratedLocation> migratedLocations = new List<MigratedLocation>();
            if (saveLocations?.Any() == true)
                this.MigrateLegacyLocations(saveLocations, gameLocations, customLocations, migratedLocations);
            this.UpdateLocationReferences(SaveGame.loaded, migratedLocations);

            // log location list
            if (this.Monitor.IsVerbose)
            {
                var locationsByPack = customLocations
                    .OrderByHuman(p => p.Name)
                    .GroupBy(p => p.ModName)
                    .OrderByHuman(p => p.Key);

                this.Monitor.VerboseLog($"Adding {customLocations.Length} locations:\n- {string.Join("\n- ", locationsByPack.Select(group => $"{group.Key}: {string.Join(", ", group.Select(p => p.Name))}"))}.");
            }

            // add locations
            foreach (CustomLocationData location in customLocations)
            {
                try
                {
                    gameLocations.Add(new GameLocation(mapPath: location.PublicMapPath, name: location.Name));
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{location.ContentPack.Manifest.Name} failed to load location '{location.Name}'. If you save after this point, any previous content in that location will be lost permanently.\nTechnical details: {ex}", LogLevel.Error);
                }
            }
        }

        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="e">The event data.</param>
        public void OnAssetRequested(AssetRequestedEventArgs e)
        {
            IAssetName assetName = e.NameWithoutLocale;

            // not a handled map
            if (!this.CustomLocationsByMapPath.TryGetValue(assetName, out CustomLocationData location))
                return;

            // invalid type
            if (!typeof(Map).IsAssignableFrom(e.DataType))
                throw new InvalidOperationException($"Unexpected attempt to load asset '{assetName}' as a {e.DataType} asset instead of {typeof(Map)}.");

            // load asset
            e.LoadFrom(
                load: () => location.ContentPack.ModContent.Load<Map>(location.FromMapFile),
                priority: AssetLoadPriority.Exclusive,
                onBehalfOf: location.ContentPack.Manifest.UniqueID
            );
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
            string[] migrateLegacyNames = config?.MigrateLegacyNames?.Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray() ?? Array.Empty<string>();
            parsed = new CustomLocationData(name, fromMapFile, migrateLegacyNames, contentPack);

            // validate name
            if (string.IsNullOrWhiteSpace(name))
                return Fail($"the {nameof(config.Name)} field is required.", parsed, out error);
            if (!CustomLocationManager.ValidNamePattern.IsMatch(name))
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

        /// <summary>Update references to renamed locations, if needed.</summary>
        /// <param name="saveData">The save data to migrate.</param>
        /// <param name="migratedLocations">The location migrations, if any.</param>
        private void UpdateLocationReferences(SaveGame saveData, IList<MigratedLocation> migratedLocations)
        {
            if (!migratedLocations.Any())
                return;

            var oldNameMap = migratedLocations.ToDictionary(p => p.OldName);
            foreach (GameLocation location in saveData.locations)
            {
                // update NPC home maps
                foreach (NPC npc in location.characters)
                {
                    string curName = npc.DefaultMap;
                    if (curName != null && oldNameMap.TryGetValue(curName, out MigratedLocation migration))
                    {
                        string newName = migration.SaveLocation.Name;
                        this.Monitor.Log($"'{migration.CustomLocation.ModName}' changed default map for NPC '{npc.Name}' from '{curName}' to '{newName}'.", LogLevel.Info);
                        npc.DefaultMap = newName;
                    }
                }
            }
        }

        /// <summary>Migrate locations which match <see cref="CustomLocationData.MigrateLegacyNames"/> to the new location name.</summary>
        /// <param name="saveLocations">The locations in the save file being loaded.</param>
        /// <param name="gameLocations">The locations that will be populated from the save data.</param>
        /// <param name="customLocations">The custom location data.</param>
        /// <param name="migratedLocations">A list to update with location migrations, if any.</param>
        private void MigrateLegacyLocations(IList<GameLocation> saveLocations, IList<GameLocation> gameLocations, CustomLocationData[] customLocations, IList<MigratedLocation> migratedLocations)
        {
            // get location lookups
            Lazy<IDictionary<string, GameLocation>> saveLocationsByName = new(() => this.MapByName(saveLocations));
            Lazy<IDictionary<string, GameLocation>> gameLocationsByName = new(() => this.MapByName(gameLocations));
            var tmxl = new TmxlLocationLoader(this.Monitor);

            // migrate locations if needed
            foreach (CustomLocationData customLocation in customLocations.Where(p => p.HasLegacyNames))
            {
                this.MigrateLegacyLocation(
                    customLocation: customLocation,
                    saveLocations: saveLocations,
                    saveLocationsByName: saveLocationsByName,
                    gameLocationsByName: gameLocationsByName,
                    tmxl: tmxl,
                    migratedLocations: migratedLocations
                );
            }
        }

        /// <summary>Migrate legacy locations in the save file for the given custom location, if needed.</summary>
        /// <param name="customLocation">The custom location for which to migrate save data.</param>
        /// <param name="saveLocations">The locations in the save file being loaded.</param>
        /// <param name="saveLocationsByName">The <paramref name="saveLocations"/> indexed by name.</param>
        /// <param name="gameLocationsByName">The locations that will be populated from the save data, indexed by name.</param>
        /// <param name="tmxl">The locations in TMXL Map Toolkit's serialized data.</param>
        /// <param name="migratedLocations">A list to update with location migrations, if any.</param>
        /// <returns>Returns whether any save data was migrated.</returns>
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField", Justification = "This is deliberate due to the Name property being readonly.")]
        private void MigrateLegacyLocation(CustomLocationData customLocation, IList<GameLocation> saveLocations, Lazy<IDictionary<string, GameLocation>> saveLocationsByName, Lazy<IDictionary<string, GameLocation>> gameLocationsByName, TmxlLocationLoader tmxl, IList<MigratedLocation> migratedLocations)
        {
            if (!customLocation.HasLegacyNames)
                return;

            // ignore names which match an existing in-game location, since that would cause data loss
            var legacyNames = new List<string>();
            foreach (string legacyName in customLocation.MigrateLegacyNames)
            {
                if (gameLocationsByName.Value.ContainsKey(legacyName))
                    this.Monitor.Log($"'{customLocation.ModName}' defines a legacy location name '{legacyName}' which matches a non-Content Patcher location and will be ignored.", LogLevel.Warn);
                else
                    legacyNames.Add(legacyName);
            }

            // already in save file
            if (saveLocationsByName.Value.ContainsKey(customLocation.Name))
                return;

            // migrate from old name
            foreach (string legacyName in legacyNames)
            {
                if (!saveLocationsByName.Value.TryGetValue(legacyName, out GameLocation saveLocation))
                    continue;

                this.Monitor.Log($"'{customLocation.ModName}' renamed saved location '{legacyName}' to '{customLocation.Name}'.", LogLevel.Info);
                saveLocation.name.Value = customLocation.Name;
                saveLocationsByName.Value.Remove(legacyName);
                saveLocationsByName.Value[saveLocation.Name] = saveLocation;
                migratedLocations.Add(
                    new MigratedLocation(legacyName, saveLocation, customLocation)
                );
                return;
            }

            // migrate from TMXL Map Toolkit
            foreach (string legacyName in legacyNames)
            {
                if (!tmxl.TryGetLocation(legacyName, out GameLocation tmxlLocation))
                    continue;

                this.Monitor.Log($"'{customLocation.ModName}' migrated saved TMXL Map Toolkit location '{legacyName}' to Content Patcher location '{customLocation.Name}'.", LogLevel.Info);
                saveLocations.Add(tmxlLocation);
                tmxlLocation.name.Value = customLocation.Name;
                saveLocationsByName.Value[customLocation.Name] = tmxlLocation;
                migratedLocations.Add(
                    new MigratedLocation(legacyName, tmxlLocation, customLocation)
                );
                return;
            }
        }

        /// <summary>Get a location-by-name lookup.</summary>
        /// <param name="locations">The locations for which to build a lookup.</param>
        private IDictionary<string, GameLocation> MapByName(IEnumerable<GameLocation> locations)
        {
            var lookup = new Dictionary<string, GameLocation>();
            foreach (GameLocation location in locations)
                lookup[location.NameOrUniqueName] = location;
            return lookup;
        }
    }
}
