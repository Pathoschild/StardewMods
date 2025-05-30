using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Locations;

namespace ContentPatcher.Framework.Migrations;

//
// Data/Locations is far more dynamic in 1.6, and there's no conceivable way to convert the new spawn options to
// the 1.5.6 format without losing nearly everything that defines them.
//
// Fortunately we don't actually need to, since we're only interested in what the edit patch changes. So this
// migration applies a few heuristics:
//   - Skip any rows/fields in the old format that didn't change.
//   - When a patch removes a spawn, all new spawn which would be converted to that format are removed.
//   - When a patch changes a spawn's chance or seasons, the existing entry is updated if possible. That's tricky
//     since there isn't always a 1:1 match between 1.5.6 (one list per season) and 1.6 (single list of conditional
//     spawns), but it's important for mod compatibility since other mods may target the same entries by ID in 1.6.
//   - Otherwise it's added to the asset with equivalent options.
//

internal partial class Migration_2_0 : BaseRuntimeMigration
{
    /// <summary>The migration logic to apply pre-1.6 <c>Data/Locations</c> patches to the new format.</summary>
    private class LocationsMigrator : IEditAssetMigrator
    {
        /*********
        ** Fields
        *********/
        /****
        ** Constants
        ****/
        /// <summary>The asset name.</summary>
        private const string AssetName = "Data/Locations";

        /// <summary>The valid season values.</summary>
        private readonly Season[] ValidSeasons = [Season.Spring, Season.Summer, Season.Fall, Season.Winter];

        /// <summary>The backing cache for <see cref="ParseEffectiveSeasons"/>.</summary>
        private readonly Dictionary<string, IReadOnlySet<Season>?> ParseSeasonsCache = [];

        /// <summary>The vanilla data without mod edits applied, used as the base when a pre-1.6 content pack loads the asset.</summary>
        private readonly VanillaAssetFactory<Dictionary<string, LocationData>> OriginalData = new(DataLoader.Locations);


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool AppliesTo(IAssetName assetName)
        {
            return assetName?.IsEquivalentTo(LocationsMigrator.AssetName, useBaseName: true) is true;
        }

        /// <inheritdoc />
        public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            return null; // same asset name
        }

        /// <inheritdoc />
        public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
        {
            var data = this.OriginalData.GetFreshCopy();
            var dataBackup = this.GetOldFormat(data, out HashSet<string> skippedDueToNoData);

            var legacyLoad = patch.Load<Dictionary<string, string>>(assetName);
            this.MergeIntoNewFormat(data, legacyLoad, dataBackup, skippedDueToNoData, patch.ContentPack.Manifest.UniqueID);

            asset = (T)(object)data;

            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
        {
            var assetData = asset.GetData<Dictionary<string, LocationData>>();
            Dictionary<string, string> tempData = this.GetOldFormat(assetData, out HashSet<string> skippedDueToNoData);
            Dictionary<string, string> tempDataBackup = new(tempData);

            patch.Edit<Dictionary<string, string>>(new FakeAssetData(asset, asset.Name, tempData), onWarning);
            this.MergeIntoNewFormat(assetData, tempData, tempDataBackup, skippedDueToNoData, patch.ContentPack.Manifest.UniqueID);

            error = null;
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the pre-1.6 equivalent for the new asset data.</summary>
        /// <param name="from">The data to convert.</param>
        /// <param name="skippedDueToNoData">The location names which were skipped because they had no spawn data. These shouldn't be deleted if they're missing.</param>
        private Dictionary<string, string> GetOldFormat(IDictionary<string, LocationData> from, out HashSet<string> skippedDueToNoData)
        {
            Dictionary<string, string> data = [];
            skippedDueToNoData = new();

            string[] fields = new string[9];
            foreach (var pair in from)
            {
                string locationName = pair.Key;
                LocationData entry = pair.Value;

                // map location names
                if (locationName == "Farm_Standard")
                    locationName = "Farm";
                else if (this.IsReservedLocationName(locationName))
                    continue;

                // load data
                this.GetForageChancesFromAsset(entry.Forage, out Dictionary<string, double> springForage, out Dictionary<string, double> summerForage, out Dictionary<string, double> fallForage, out Dictionary<string, double> winterForage);
                this.GetFishFromNewList(entry.Fish, out HashSet<FishSpawnValue> springFish, out HashSet<FishSpawnValue> summerFish, out HashSet<FishSpawnValue> fallFish, out HashSet<FishSpawnValue> winterFish);
                this.GetArtifactChancesFromAsset(entry.ArtifactSpots, out Dictionary<string, double> artifacts);

                // skip if no data
                if (
                    springForage.Count == 0 && summerForage.Count == 0 && fallForage.Count == 0 && winterForage.Count == 0
                    && springFish.Count == 0 && summerFish.Count == 0 && fallFish.Count == 0 && winterFish.Count == 0
                    && artifacts.Count == 0
                )
                {
                    skippedDueToNoData.Add(locationName);
                    continue;
                }

                // apply
                fields[0] = this.ConvertToOldArtifactOrForageString(springForage);
                fields[1] = this.ConvertToOldArtifactOrForageString(summerForage);
                fields[2] = this.ConvertToOldArtifactOrForageString(fallForage);
                fields[3] = this.ConvertToOldArtifactOrForageString(winterForage);
                fields[4] = this.ConvertToOldFishString(locationName, springFish);
                fields[5] = this.ConvertToOldFishString(locationName, summerFish);
                fields[6] = this.ConvertToOldFishString(locationName, fallFish);
                fields[7] = this.ConvertToOldFishString(locationName, winterFish);
                fields[8] = this.ConvertToOldArtifactOrForageString(artifacts);

                data[locationName] = string.Join('/', fields);
            }

            return data;
        }

        /// <summary>Merge pre-1.6 data into the new asset.</summary>
        /// <param name="asset">The asset data to update.</param>
        /// <param name="temp">The pre-1.6 data to merge into the asset.</param>
        /// <param name="tempBackup">A copy of <paramref name="temp"/> before edits were applied.</param>
        /// <param name="skippedDueToNoData">The location names which were skipped because they had no spawn data. These shouldn't be deleted if they're missing.</param>
        /// <param name="modId">The unique ID for the mod, used in auto-generated entry IDs.</param>
        private void MergeIntoNewFormat(IDictionary<string, LocationData> asset, Dictionary<string, string> temp, Dictionary<string, string>? tempBackup, IReadOnlySet<string>? skippedDueToNoData, string modId)
        {
            // remove deleted entries
            foreach (string key in asset.Keys)
            {
                if (this.IsReservedLocationName(key))
                    continue;

                if (!temp.ContainsKey(key) && skippedDueToNoData?.Contains(key) is not true)
                    asset.Remove(key);
            }

            // apply entries
            foreach ((string oldKey, string fromEntry) in temp)
            {
                // map location names
                string locationName = oldKey;
                if (locationName is "Farm")
                    locationName = "Farm_Standard";
                else if (this.IsReservedLocationName(locationName))
                    continue;

                // get/add target record
                bool isNew = false;
                if (!asset.TryGetValue(locationName, out LocationData? targetLocationEntry))
                {
                    isNew = true;
                    targetLocationEntry = new LocationData
                    {
                        CreateOnLoad = new()
                        {
                            MapPath = $"Maps\\{locationName}"
                        }
                    };
                }

                // get backup
                if (tempBackup is not null)
                {
                    if (tempBackup.TryGetValue(oldKey, out string? prevRow) && prevRow == fromEntry)
                        continue; // no changes
                }

                // merge fields into new asset
                string[] fromFields = fromEntry.Split('/');
                if (locationName is not "Farm_Standard") // farm forage/fish were hardcoded in 1.5.6
                {
                    this.MergeForageIntoNewFormat(locationName, fromFields, targetLocationEntry, modId);
                    this.MergeFishIntoNewFormat(locationName, fromFields, targetLocationEntry, modId);
                }
                this.MergeArtifactsIntoNewFormat(fromFields, targetLocationEntry, modId);

                // set value
                if (isNew)
                    asset[locationName] = targetLocationEntry;
            }
        }

        /// <summary>Whether a location name is reserved for 1.6 content and can't be edited by a 1.5.6 patch.</summary>
        /// <param name="locationName">The location name to check.</param>
        private bool IsReservedLocationName(string locationName)
        {
            return locationName is "Default" || locationName?.StartsWith("Farm_") is true;
        }

        /// <summary>Merge pre-1.6 forage data into the new asset.</summary>
        /// <param name="locationName">The internal location name being edited.</param>
        /// <param name="fromFields">The pre-1.6 data to merge into the asset.</param>
        /// <param name="targetLocationEntry">The location entry being added or edited.</param>
        /// <param name="modId">The unique ID for the mod, used in auto-generated entry IDs.</param>
        private void MergeForageIntoNewFormat(string locationName, string[] fromFields, LocationData targetLocationEntry, string modId)
        {
            // parseparse forage fields
            Dictionary<string, double> fromSpringForage = this.GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 0));
            Dictionary<string, double> fromSummerForage = this.GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 1));
            Dictionary<string, double> fromFallForage = this.GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 2));
            Dictionary<string, double> fromWinterForage = this.GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 3));
            Dictionary<string, double> GetListForSeason(Season season)
            {
                return season switch
                {
                    Season.Spring => fromSpringForage,
                    Season.Summer => fromSummerForage,
                    Season.Fall => fromFallForage,
                    _ => fromWinterForage
                };
            }
            void RemoveFromSeasons(string objectId, IEnumerable<Season> seasons)
            {
                foreach (Season season in seasons)
                    GetListForSeason(season).Remove(objectId);
            }

            // step 1: remove or update existing entries
            if (targetLocationEntry.Forage?.Count > 0)
            {
                for (int i = 0; i < targetLocationEntry.Forage.Count; i++)
                {
                    var entry = targetLocationEntry.Forage[i];

                    // parse entry
                    string? objectId = RuntimeMigrationHelper.ParseObjectId(entry.ItemId);
                    IReadOnlySet<Season>? actualSeasons = this.ParseEffectiveSeasons(entry.Season, entry.Condition);
                    if (objectId is null || actualSeasons is null)
                        continue;

                    // get updated chance for each season the entry targets
                    Dictionary<Season, double> seasonChances = [];
                    double? oneChance = null;
                    bool allSameChance = true;
                    foreach (Season season in actualSeasons)
                    {
                        double chance = GetListForSeason(season).GetValueOrDefault(objectId);
                        if (chance > 0)
                        {
                            oneChance ??= chance;
                            allSameChance = allSameChance && oneChance == chance;
                            seasonChances[season] = chance;
                        }
                    }

                    // remove seasons that no longer apply
                    if (!actualSeasons.SetEquals(seasonChances.Keys))
                    {
                        HashSet<Season> seasons = new(actualSeasons);
                        seasons.IntersectWith(seasonChances.Keys);

                        RemoveFromSeasons(objectId, seasons);

                        this.SetSeasons(locationName, targetLocationEntry.Forage, entry, seasons, out bool wasRemoved);
                        if (wasRemoved)
                        {
                            i--;
                            continue;
                        }

                        actualSeasons = seasons;
                    }

                    // update chance if all seasons have the same chance
                    if (allSameChance)
                    {
                        entry.Chance = oneChance!.Value;
                        RemoveFromSeasons(objectId, actualSeasons);
                    }
                }
            }

            // step 2: add any remaining as new entries
            void SaveForSeason(Season season, Dictionary<string, double> fromForage)
            {
                if (fromForage.Count > 0)
                {
                    targetLocationEntry.Forage ??= new();

                    foreach ((string objectId, double chance) in fromForage)
                    {
                        string qualifiedItemId = ItemRegistry.type_object + objectId;

                        targetLocationEntry.Forage.Add(
                            new SpawnForageData
                            {
                                Id = $"{modId}_{season}_{qualifiedItemId}",
                                ItemId = qualifiedItemId,
                                Chance = chance,
                                Season = season
                            }
                        );
                    }
                }
            }

            SaveForSeason(Season.Spring, fromSpringForage);
            SaveForSeason(Season.Summer, fromSummerForage);
            SaveForSeason(Season.Fall, fromFallForage);
            SaveForSeason(Season.Winter, fromWinterForage);
        }

        /// <summary>Merge pre-1.6 fish data into the new asset.</summary>
        /// <param name="locationName">The internal location name being edited.</param>
        /// <param name="fromFields">The pre-1.6 data to merge into the asset.</param>
        /// <param name="targetLocationEntry">The location entry being added or edited.</param>
        /// <param name="modId">The unique ID for the mod, used in auto-generated entry IDs.</param>
        private void MergeFishIntoNewFormat(string locationName, string[] fromFields, LocationData targetLocationEntry, string modId)
        {
            // parse forage fields
            HashSet<FishSpawnValue> fromSpringFish = this.GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 4));
            HashSet<FishSpawnValue> fromSummerFish = this.GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 5));
            HashSet<FishSpawnValue> fromFallFish = this.GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 6));
            HashSet<FishSpawnValue> fromWinterFish = this.GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 7));
            HashSet<FishSpawnValue> GetListForSeason(Season season)
            {
                return season switch
                {
                    Season.Spring => fromSpringFish,
                    Season.Summer => fromSummerFish,
                    Season.Fall => fromFallFish,
                    _ => fromWinterFish
                };
            }
            void RemoveFromSeasons(FishSpawnValue key, IEnumerable<Season> seasons)
            {
                foreach (Season season in seasons)
                    GetListForSeason(season).Remove(key);
            }

            // step 1: remove or update existing entries
            if (targetLocationEntry.Fish?.Count > 0)
            {
                for (int i = 0; i < targetLocationEntry.Fish.Count; i++)
                {
                    var entry = targetLocationEntry.Fish[i];

                    // parse entry
                    string? objectId = RuntimeMigrationHelper.ParseObjectId(entry.ItemId);
                    IReadOnlySet<Season>? actualSeasons = this.ParseEffectiveSeasons(entry.Season, entry.Condition);
                    if (objectId is null || actualSeasons is null)
                        continue;
                    FishSpawnValue key = new FishSpawnValue(objectId, entry.FishAreaId);

                    // get updated chance for each season the entry targets
                    HashSet<Season> newSeasons = [];
                    foreach (Season season in actualSeasons)
                    {
                        if (GetListForSeason(season).Contains(key))
                            newSeasons.Add(season);
                    }

                    // remove seasons that no longer apply
                    if (!actualSeasons.SetEquals(newSeasons))
                    {
                        this.SetSeasons(locationName, targetLocationEntry.Fish, entry, newSeasons, out bool wasRemoved);
                        if (wasRemoved)
                        {
                            i--;
                            continue;
                        }

                        actualSeasons = newSeasons;
                    }

                    // track that it's already in the list
                    RemoveFromSeasons(key, actualSeasons);
                }
            }

            // step 2: add any remaining as new entries
            void SaveForSeason(Season season, HashSet<FishSpawnValue> fromFish)
            {
                if (fromFish.Count > 0)
                {
                    targetLocationEntry.Fish ??= new();

                    foreach ((string objectId, string? fishAreaId) in fromFish)
                    {
                        string qualifiedItemId = ItemRegistry.type_object + objectId;

                        targetLocationEntry.Fish.Add(
                            new SpawnFishData
                            {
                                Id = $"{modId}_{season}_{qualifiedItemId}{(fishAreaId != null ? "_" + fishAreaId : "")}",
                                ItemId = qualifiedItemId,
                                FishAreaId = fishAreaId,
                                Season = season
                            }
                        );
                    }
                }
            }

            SaveForSeason(Season.Spring, fromSpringFish);
            SaveForSeason(Season.Summer, fromSummerFish);
            SaveForSeason(Season.Fall, fromFallFish);
            SaveForSeason(Season.Winter, fromWinterFish);
        }

        /// <summary>Merge pre-1.6 artifact data into the new asset.</summary>
        /// <param name="fromFields">The pre-1.6 data to merge into the asset.</param>
        /// <param name="targetLocationEntry">The location entry being added or edited.</param>
        /// <param name="modId">The unique ID for the mod, used in auto-generated entry IDs.</param>
        private void MergeArtifactsIntoNewFormat(string[] fromFields, LocationData targetLocationEntry, string modId)
        {
            // parse artifact field
            Dictionary<string, double> fromArtifacts = this.GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 8));

            // step 1: remove or update existing entries
            if (targetLocationEntry.ArtifactSpots?.Count > 0)
            {
                for (int i = 0; i < targetLocationEntry.ArtifactSpots.Count; i++)
                {
                    var entry = targetLocationEntry.ArtifactSpots[i];

                    // parse entry
                    string? objectId = RuntimeMigrationHelper.ParseObjectId(entry.ItemId);
                    IReadOnlySet<Season>? actualSeasons = this.ParseEffectiveSeasons(null, entry.Condition);
                    if (objectId is null || actualSeasons is null)
                        continue;

                    // skip: can't convert seasonal artifact spots between 1.5.6 and 1.6
                    if (actualSeasons.Count != 4)
                        continue;

                    // remove if deleted
                    if (!fromArtifacts.TryGetValue(objectId, out double chance))
                    {
                        targetLocationEntry.ArtifactSpots.RemoveAt(i);
                        i--;
                        continue;
                    }

                    // update chance
                    entry.Chance = chance;
                    fromArtifacts.Remove(objectId);
                }
            }

            // step 2: add any remaining as new entries
            if (fromArtifacts.Count > 0)
            {
                targetLocationEntry.ArtifactSpots ??= new();

                foreach ((string objectId, double chance) in fromArtifacts)
                {
                    string qualifiedItemId = ItemRegistry.type_object + objectId;

                    targetLocationEntry.ArtifactSpots.Add(
                        new ArtifactSpotDropData
                        {
                            Id = $"{modId}_{qualifiedItemId}",
                            ItemId = qualifiedItemId,
                            Chance = chance
                        }
                    );
                }
            }
        }

        /// <summary>Set the seasons for a forage entry, or remove it if it no longer spawns in any season.</summary>
        /// <param name="locationName">The internal location name.</param>
        /// <param name="list">The forage list in the target asset.</param>
        /// <param name="entry">The entry to edit in the target asset.</param>
        /// <param name="seasons">The seasons for which it should spawn.</param>
        /// <param name="wasRemoved">Whether the forage entry was removed from the <paramref name="list"/>.</param>
        private void SetSeasons(string locationName, List<SpawnForageData> list, SpawnForageData entry, HashSet<Season> seasons, out bool wasRemoved)
        {
            wasRemoved = false;

            switch (seasons.Count)
            {
                case 0:
                    list.Remove(entry);
                    wasRemoved = true;
                    break;

                case 1:
                    entry.Season = seasons.First();
                    entry.Condition = null;
                    break;

                default:
                    entry.Season = null;
                    entry.Condition = entry.Condition?.Trim().StartsWith(nameof(GameStateQuery.DefaultResolvers.SEASON), StringComparison.OrdinalIgnoreCase) is true
                        ? $"{nameof(GameStateQuery.DefaultResolvers.SEASON)} {string.Join(' ', seasons)}"
                        : $"{nameof(GameStateQuery.DefaultResolvers.LOCATION_SEASON)} {locationName} {string.Join(' ', seasons)}";
                    break;
            }
        }

        /// <summary>Set the seasons for a fish entry, or remove it if it no longer spawns in any season.</summary>
        /// <param name="locationName">The internal location name.</param>
        /// <param name="list">The fish list in the target asset.</param>
        /// <param name="entry">The entry to edit in the target asset.</param>
        /// <param name="seasons">The seasons for which it should spawn.</param>
        /// <param name="wasRemoved">Whether the fish entry was removed from the <paramref name="list"/>.</param>
        private void SetSeasons(string locationName, List<SpawnFishData> list, SpawnFishData entry, HashSet<Season> seasons, out bool wasRemoved)
        {
            wasRemoved = false;

            switch (seasons.Count)
            {
                case 0:
                    list.Remove(entry);
                    wasRemoved = true;
                    break;

                case 1:
                    entry.Season = seasons.First();
                    entry.Condition = null;
                    break;

                default:
                    entry.Season = null;
                    entry.Condition = entry.Condition?.Trim().StartsWith(nameof(GameStateQuery.DefaultResolvers.SEASON), StringComparison.OrdinalIgnoreCase) is true
                        ? $"{nameof(GameStateQuery.DefaultResolvers.SEASON)} {string.Join(' ', seasons)}"
                        : $"{nameof(GameStateQuery.DefaultResolvers.LOCATION_SEASON)} {locationName} {string.Join(' ', seasons)}";
                    break;
            }
        }

        /// <summary>Get the item IDs that spawn based on a space-delimited list of 'ID chance' pairs.</summary>
        /// <param name="rawData">The raw forage list string.</param>
        private Dictionary<string, double> GetChancesFromOldArtifactOrForageList(string rawData)
        {
            Dictionary<string, double> forage = [];

            string[] fields = rawData.Split(' ');
            for (int i = 0; i < fields.Length - 1; i += 2)
            {
                string objectId = fields[i];
                if (double.TryParse(fields[i + 1], out double chance))
                    forage[objectId] = chance;
            }

            return forage;
        }

        /// <summary>Get the fish IDs that spawn based on a space-delimited list of 'ID zone chance' tuples.</summary>
        /// <param name="locationName">The internal location name.</param>
        /// <param name="rawData">The raw forage list string.</param>
        private HashSet<FishSpawnValue> GetValuesFromOldFishList(string locationName, string rawData)
        {
            HashSet<FishSpawnValue> fish = [];

            string[] fields = rawData.Split(' ');
            for (int i = 0; i < fields.Length - 1; i += 2)
            {
                string objectId = fields[i];
                string? fishArea = this.UpgradeFishAreaId(locationName, fields[i + 1]);

                fish.Add(new FishSpawnValue(objectId, fishArea));
            }

            return fish;
        }

        /// <summary>Get the forage that spawn based on the new data.</summary>
        /// <param name="data">The new location forage data.</param>
        /// <param name="springForage">The equivalent pre-1.6 data for spring.</param>
        /// <param name="summerForage">The equivalent pre-1.6 data for summer.</param>
        /// <param name="fallForage">The equivalent pre-1.6 data for fall.</param>
        /// <param name="winterForage">The equivalent pre-1.6 data for winter.</param>
        private void GetForageChancesFromAsset(List<SpawnForageData> data, out Dictionary<string, double> springForage, out Dictionary<string, double> summerForage, out Dictionary<string, double> fallForage, out Dictionary<string, double> winterForage)
        {
            // init lookups
            springForage = new();
            summerForage = new();
            fallForage = new();
            winterForage = new();

            // read data
            foreach (SpawnForageData entry in data)
            {
                // parse raw data
                string? objectId = RuntimeMigrationHelper.ParseObjectId(entry.ItemId);
                IReadOnlySet<Season>? seasons = this.ParseEffectiveSeasons(entry.Season, entry.Condition);
                if (objectId is null || seasons is null)
                    continue;

                // apply
                foreach (Season season in this.ValidSeasons)
                {
                    if (!seasons.Contains(season))
                        continue;

                    var dict = season switch
                    {
                        Season.Spring => springForage,
                        Season.Summer => summerForage,
                        Season.Fall => fallForage,
                        _ => winterForage
                    };

                    dict[objectId] = entry.Chance;
                }
            }
        }

        /// <summary>Get the artifacts that spawn based on the new data.</summary>
        /// <param name="data">The new location artifact data.</param>
        /// <param name="artifacts">The equivalent pre-1.6 data.</param>
        private void GetArtifactChancesFromAsset(List<ArtifactSpotDropData> data, out Dictionary<string, double> artifacts)
        {
            // init lookup
            artifacts = new();

            // read data
            foreach (ArtifactSpotDropData entry in data)
            {
                // parse raw data
                string? objectId = RuntimeMigrationHelper.ParseObjectId(entry.ItemId);
                IReadOnlySet<Season>? seasons = this.ParseEffectiveSeasons(null, entry.Condition);
                if (objectId is null || seasons is null)
                    continue;

                // skip: can't convert seasonal artifact spots to 1.5.6
                if (seasons.Count != 4)
                    continue;

                // apply
                artifacts[objectId] = entry.Chance;
            }
        }

        /// <summary>Get the fish that spawn based on the new data.</summary>
        /// <param name="data">The new location fish data.</param>
        /// <param name="springFish">The equivalent pre-1.6 data for spring.</param>
        /// <param name="summerFish">The equivalent pre-1.6 data for summer.</param>
        /// <param name="fallFish">The equivalent pre-1.6 data for fall.</param>
        /// <param name="winterFish">The equivalent pre-1.6 data for winter.</param>
        private void GetFishFromNewList(List<SpawnFishData> data, out HashSet<FishSpawnValue> springFish, out HashSet<FishSpawnValue> summerFish, out HashSet<FishSpawnValue> fallFish, out HashSet<FishSpawnValue> winterFish)
        {
            // init lookups
            springFish = new();
            summerFish = new();
            fallFish = new();
            winterFish = new();

            // read data
            foreach (SpawnFishData entry in data)
            {
                if (entry.Chance < 1)
                    continue; // can't convert fish chances to 1.5.6

                // parse raw data
                string? objectId = RuntimeMigrationHelper.ParseObjectId(entry.ItemId);
                IReadOnlySet<Season>? seasons = this.ParseEffectiveSeasons(null, entry.Condition);
                if (objectId is null || seasons is null)
                    continue;

                // apply
                foreach (Season season in this.ValidSeasons)
                {
                    if (!seasons.Contains(season))
                        continue;

                    var dict = season switch
                    {
                        Season.Spring => springFish,
                        Season.Summer => summerFish,
                        Season.Fall => fallFish,
                        _ => winterFish
                    };

                    dict.Add(new FishSpawnValue(objectId, entry.FishAreaId));
                }
            }
        }

        /// <summary>Convert a dictionary of artifact or forage spawn values to the old format.</summary>
        /// <param name="forage">The forage data to convert.</param>
        private string ConvertToOldArtifactOrForageString(Dictionary<string, double> forage)
        {
            if (forage.Count == 0)
                return "-1";

            StringBuilder result = new();
            foreach ((string objectId, double chance) in forage)
            {
                result
                    .Append(objectId)
                    .Append(' ')
                    .Append(chance)
                    .Append(' ');
            }

            return result.ToString(0, result.Length - 1);
        }

        /// <summary>Convert a dictionary of fish spawn values to the old format.</summary>
        /// <param name="locationName">The internal location name.</param>
        /// <param name="fish">The fish data to convert.</param>
        private string ConvertToOldFishString(string locationName, HashSet<FishSpawnValue> fish)
        {
            if (fish.Count == 0)
                return "-1";

            StringBuilder result = new();
            foreach ((string objectId, string? fishZoneId) in fish)
            {
                result
                    .Append(objectId)
                    .Append(' ')
                    .Append(this.DowngradeFishAreaId(locationName, fishZoneId))
                    .Append(' ');
            }

            return result.ToString(0, result.Length - 1);
        }

        private string DowngradeFishAreaId(string locationName, string? id)
        {
            if (id is null)
                return "-1";

            switch (locationName)
            {
                case "Forest":
                    return id switch
                    {
                        "Lake" => "1",
                        "River" => "0",
                        _ => id
                    };

                case "IslandWest":
                    return id switch
                    {
                        "Freshwater" => "2",
                        "Ocean" => "1",
                        _ => id
                    };

                default:
                    return id;
            }
        }

        private string? UpgradeFishAreaId(string locationName, string id)
        {
            if (id is "-1")
                return null;

            switch (locationName)
            {
                case "Forest":
                    return id switch
                    {
                        "0" => "River",
                        "1" => "Lake",
                        _ => id
                    };

                case "IslandWest":
                    return id switch
                    {
                        "1" => "Ocean",
                        "2" => "Freshwater",
                        _ => id
                    };

                default:
                    return id;
            }
        }

        /// <summary>Parse the seasons for which a spawn entry applies, if it's valid and not too dynamic to convert to 1.5.6.</summary>
        /// <param name="season">The specific season for which the entry applies.</param>
        /// <param name="condition">The game state query which indicates whether the entry applies.</param>
        /// <returns>Returns the parsed seasons, or <c>null</c> if invalid or not convertible to 1.5.6.</returns>
        private IReadOnlySet<Season>? ParseEffectiveSeasons(Season? season, string condition)
        {
            string cacheKey = $"{season}|{condition}";

            // skip cached
            {
                if (this.ParseSeasonsCache.TryGetValue(cacheKey, out IReadOnlySet<Season>? cached))
                    return cached;
            }

            // parse raw seasons
            if (!this.TryParseRawSeasonsFromNewData(season, condition, out HashSet<Season> seasons, out HashSet<Season> notSeasons))
            {
                this.ParseSeasonsCache[cacheKey] = null;
                return null;
            }

            // cancel out seasons that are both included and excluded
            if (notSeasons.Count > 0 && notSeasons.Count > 0)
            {
                foreach (Season curSeason in this.ValidSeasons)
                {
                    bool apply = seasons.Contains(curSeason);
                    bool exclude = notSeasons.Contains(curSeason);

                    if (apply && exclude)
                    {
                        seasons.Remove(curSeason);
                        notSeasons.Remove(curSeason);
                    }
                }

                // if all seasons were cancelled out, the entry can never spawn
                if (seasons.Count == 0 && notSeasons.Count == 0)
                {
                    this.ParseSeasonsCache[cacheKey] = null;
                    return null;
                }
            }

            // If both seasons and not-seasons still have values, they're non-overlapping. In that case not-seasons
            // is redundant since it would only exclude seasons that are already not excluded via seasons.
            if (seasons.Count > 0 && notSeasons.Count > 0)
                notSeasons.Clear();

            // invert not seasons
            if (notSeasons.Count > 0)
            {
                foreach (Season curSeason in this.ValidSeasons)
                {
                    if (!notSeasons.Contains(curSeason))
                        seasons.Add(curSeason);
                }
            }

            // get result
            if (seasons.Count > 0)
            {
                this.ParseSeasonsCache[cacheKey] = seasons;
                return seasons;
            }
            else
            {
                this.ParseSeasonsCache[cacheKey] = null;
                return null;
            }
        }

        /// <summary>Extract the seasons for which a spawn entry applies, if it's valid and not too dynamic to convert to 1.5.6.</summary>
        /// <param name="season">The specific season for which the entry applies.</param>
        /// <param name="condition">The game state query which indicates whether the entry applies.</param>
        /// <param name="parsedSeasons">The set to which to add the seasons it spawns in.</param>
        /// <param name="parsedNotSeasons">The set to which to add the seasons it cannot spawn in.</param>
        private bool TryParseRawSeasonsFromNewData(Season? season, string condition, out HashSet<Season> parsedSeasons, out HashSet<Season> parsedNotSeasons)
        {
            // init sets
            parsedSeasons = new();
            parsedNotSeasons = new();

            // single season
            bool isSingleSeason = false;
            if (season.HasValue)
            {
                parsedSeasons.Add(season.Value);
                isSingleSeason = true;
            }

            // shortcut if no conditions
            if (GameStateQuery.IsImmutablyTrue(condition))
            {
                if (!isSingleSeason)
                {
                    parsedSeasons.Add(Season.Spring);
                    parsedSeasons.Add(Season.Summer);
                    parsedSeasons.Add(Season.Fall);
                    parsedSeasons.Add(Season.Winter);
                }

                return parsedSeasons.Count > 0;
            }

            // extract from game state query
            foreach (GameStateQuery.ParsedGameStateQuery query in GameStateQuery.Parse(condition))
            {
                // skip broken
                if (query.Error is not null)
                    return false;

                // get index where list of seasons starts
                string[] args = query.Query;
                string name = ArgUtility.Get(args, 0);
                int seasonIndex;
                if (string.Equals(name, nameof(GameStateQuery.DefaultResolvers.LOCATION_SEASON)))
                    seasonIndex = 2;
                else if (string.Equals(name, nameof(GameStateQuery.DefaultResolvers.SEASON)))
                    seasonIndex = 1;
                else
                    return false; // non-season query, too dynamic to convert

                // apply
                bool negate = query.Negated;
                for (int i = seasonIndex; i < args.Length; i++)
                {
                    if (!Utility.TryParseEnum(args[i], out Season curSeason))
                        return false;

                    if (negate)
                        parsedNotSeasons.Add(curSeason);
                    else
                        parsedSeasons.Add(curSeason);
                }
            }

            return parsedSeasons.Count > 0 || parsedNotSeasons.Count > 0;
        }
    }

    /// <summary>The unique key pair for old fish spawn data.</summary>
    /// <param name="ObjectId">The unqualified object ID to spawn.</param>
    /// <param name="FishZoneId">The fish zone ID it spawns in.</param>
    public record FishSpawnValue(string ObjectId, string? FishZoneId);
}
