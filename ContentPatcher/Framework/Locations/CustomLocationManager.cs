using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;
using xTile;

namespace ContentPatcher.Framework.Locations;

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
    private readonly List<CustomLocationData> CustomLocations = [];

    /// <summary>The enabled locations indexed by their normalized map path.</summary>
    private readonly Dictionary<IAssetName, CustomLocationData> CustomLocationsByMapPath = [];

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
    public bool TryAddCustomLocationConfig(CustomLocationConfig? config, IContentPack contentPack, [NotNullWhen(false)] out string? error)
    {
        // validate config
        if (!this.TryParseCustomLocationConfig(config, contentPack, out CustomLocationData? parsed, out error))
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

    /// <summary>Add TMXL Map Toolkit locations to the save where needed for the game to load their data.</summary>
    /// <param name="saveLocations">The locations in the save file being loaded, or <c>null</c> when creating a new save.</param>
    /// <param name="gameLocations">The locations that will be populated from the save data.</param>
    public void AddTmxlLocations(IList<GameLocation>? saveLocations, IList<GameLocation> gameLocations)
    {
        // skip if no legacy data to migrate
        if (saveLocations?.Count is not > 0)
            return;

        // get location lookups
        Lazy<IDictionary<string, GameLocation>> saveLocationsByName = new(() => this.MapByName(saveLocations));
        Lazy<IDictionary<string, GameLocation>> gameLocationsByName = new(() => this.MapByName(gameLocations));
        var tmxl = new TmxlLocationLoader(this.Monitor);

        // migrate locations if needed
        foreach (CustomLocationData customLocation in this.CustomLocations)
        {
            if (!customLocation.IsEnabled || !customLocation.HasLegacyNames)
                continue;

            // ignore names which match an existing in-game location, since that would cause data loss
            var legacyNames = new List<string>();
            foreach (string legacyName in customLocation.MigrateLegacyNames)
            {
                if (gameLocationsByName.Value.ContainsKey(legacyName))
                    this.Monitor.Log($"'{customLocation.ModName}' defines a legacy location name '{legacyName}' which matches an existing location and will be ignored.", LogLevel.Warn);
                else
                    legacyNames.Add(legacyName);
            }

            // migrate from TMXL Map Toolkit
            if (!saveLocationsByName.Value.ContainsKey(customLocation.Name))
            {
                foreach (string legacyName in legacyNames)
                {
                    if (!tmxl.TryGetLocation(legacyName, out GameLocation? tmxlLocation))
                        continue;

                    this.Monitor.Log($"'{customLocation.ModName}' recovered TMXL Map Toolkit location '{legacyName}' which will be mapped to new location '{customLocation.Name}'.", LogLevel.Info);
                    saveLocations.Add(tmxlLocation);

#pragma warning disable AvoidNetField // deliberate since the Name property isn't settable
                    tmxlLocation.name.Value = customLocation.Name;
#pragma warning restore AvoidNetField

                    saveLocationsByName.Value[customLocation.Name] = tmxlLocation;
                    break;
                }
            }
        }
    }

    /// <summary>Handle the mod being initialized for a screen. The content packs are fully loaded at this point and the initial context was set.</summary>
    public void OnScreenInitialized()
    {
        if (this.CustomLocations.Count > 0)
            this.ContentHelper.InvalidateCache("Data/Locations");
    }

    /// <inheritdoc cref="IContentEvents.AssetRequested" />
    /// <returns>Returns whether the asset was loaded for a custom location.</returns>
    public bool OnAssetRequested(AssetRequestedEventArgs e)
    {
        IAssetName assetName = e.NameWithoutLocale;

        // location data
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
        {
            e.Edit(
                rawData =>
                {
                    IDictionary<string, LocationData> data = rawData.AsDictionary<string, LocationData>().Data;

                    foreach (CustomLocationData entry in this.CustomLocations)
                    {
                        if (!entry.IsEnabled)
                            continue;

                        if (data.ContainsKey(entry.Name))
                        {
                            this.Monitor.Log($"{entry.ModName} failed to add location '{entry.Name}' because it already exists in Data/Locations.", LogLevel.Error);
                            continue;
                        }

                        LocationData model = new()
                        {
                            CreateOnLoad = new()
                            {
                                MapPath = entry.PublicMapPath
                            }
                        };

                        if (entry.HasLegacyNames)
                            model.FormerLocationNames = entry.MigrateLegacyNames.ToList();

                        data[entry.Name] = model;
                    }
                },
                AssetEditPriority.Early // add data before content packs start editing it
            );
        }

        // map asset
        else if (this.CustomLocationsByMapPath.TryGetValue(assetName, out CustomLocationData? location))
        {
            if (!typeof(Map).IsAssignableFrom(e.DataType))
                throw new InvalidOperationException($"Unexpected attempt to load asset '{assetName}' as a {e.DataType} asset instead of {typeof(Map)}.");

            e.LoadFrom(
                load: () => location.ContentPack.ModContent.Load<Map>(location.FromMapFile),
                priority: AssetLoadPriority.Exclusive,
                onBehalfOf: location.ContentPack.Manifest.UniqueID
            );

            return true;
        }

        // invalid type
        return false;
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
    private bool TryParseCustomLocationConfig(CustomLocationConfig? config, IContentPack contentPack, [NotNullWhen(true)] out CustomLocationData? parsed, [NotNullWhen(false)] out string? error)
    {
        static bool Fail(string reason, out string setError)
        {
            setError = reason;
            return false;
        }

        // read values
        string? name = config?.Name?.Trim();
        string? fromMapFile = config?.FromMapFile?.Trim();
        string[] migrateLegacyNames = config?.MigrateLegacyNames.Select(p => p?.Trim()!).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray() ?? [];
        parsed = null;

        // validate name
        if (string.IsNullOrWhiteSpace(name))
            return Fail($"the {nameof(config.Name)} field is required.", out error);
        if (!CustomLocationManager.ValidNamePattern.IsMatch(name))
            return Fail($"the {nameof(config.Name)} field can only contain alphanumeric or underscore characters.", out error);
        if (!name.StartsWith($"{contentPack.Manifest.UniqueID}_") && !name.StartsWith("Custom_"))
            return Fail($"the {nameof(config.Name)} field must be prefixed with the mod ID (like '{contentPack.Manifest.UniqueID}_') or 'Custom_' to avoid conflicting with current or future vanilla locations.", out error);

        // validate map file
        if (string.IsNullOrWhiteSpace(fromMapFile))
            return Fail($"the {nameof(config.FromMapFile)} field is required.", out error);
        if (!contentPack.HasFile(fromMapFile))
            return Fail($"the {nameof(config.FromMapFile)} field specifies a file '{fromMapFile}' which doesn't exist in the content pack.", out error);

        // create instance
        parsed = new CustomLocationData(name, fromMapFile, migrateLegacyNames, contentPack);
        error = null;
        return true;
    }

    /// <summary>Get a location-by-name lookup.</summary>
    /// <param name="locations">The locations for which to build a lookup.</param>
    private IDictionary<string, GameLocation> MapByName(IEnumerable<GameLocation> locations)
    {
        Dictionary<string, GameLocation> lookup = [];
        foreach (GameLocation location in locations)
            lookup[location.NameOrUniqueName] = location;
        return lookup;
    }
}
