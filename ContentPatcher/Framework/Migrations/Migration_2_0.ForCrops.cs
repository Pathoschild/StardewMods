using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;

namespace ContentPatcher.Framework.Migrations;

internal partial class Migration_2_0 : BaseRuntimeMigration
{
    /// <summary>The migration logic to apply pre-1.6 <c>Data/Crops</c> patches to the new format.</summary>
    private class CropsMigrator : IEditAssetMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The asset name.</summary>
        private const string AssetName = "Data/Crops";

        /// <summary>The vanilla data without mod edits applied, used as the base when a pre-1.6 content pack loads the asset.</summary>
        private readonly VanillaAssetFactory<Dictionary<string, CropData>> OriginalData = new(DataLoader.Crops);


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool AppliesTo(IAssetName assetName)
        {
            return assetName?.IsEquivalentTo(CropsMigrator.AssetName, useBaseName: true) is true;
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
            var dataBackup = this.GetOldFormat(data);

            var legacyLoad = patch.Load<Dictionary<string, string>>(assetName);
            this.MergeIntoNewFormat(data, legacyLoad, dataBackup);

            asset = (T)(object)data;

            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
        {
            var data = asset.GetData<Dictionary<string, CropData>>();
            Dictionary<string, string> tempData = this.GetOldFormat(data);
            Dictionary<string, string> tempDataBackup = new(tempData);
            patch.Edit<Dictionary<string, string>>(new FakeAssetData(asset, asset.Name, tempData), onWarning);
            this.MergeIntoNewFormat(data, tempData, tempDataBackup);

            error = null;
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the pre-1.6 equivalent for the new asset data.</summary>
        /// <param name="from">The data to convert.</param>
        private Dictionary<string, string> GetOldFormat(IDictionary<string, CropData> from)
        {
            var data = new Dictionary<string, string>();

            string[] fields = new string[9];
            foreach ((string objectId, CropData entry) in from)
            {
                fields[0] = string.Join(' ', entry.DaysInPhase);
                fields[1] = this.GetOldSeasonsField(entry);
                fields[2] = entry.SpriteIndex.ToString();
                fields[3] = entry.HarvestItemId;
                fields[4] = entry.RegrowDays.ToString();
                fields[5] = ((int)entry.HarvestMethod).ToString();
                fields[6] = this.GetOldExtraHarvestField(entry);
                fields[7] = entry.IsRaised.ToString().ToLowerInvariant();
                fields[8] = this.GetOldTintColorsField(entry);

                data[objectId] = string.Join('/', fields);
            }

            return data;
        }

        /// <summary>Merge pre-1.6 data into the new asset.</summary>
        /// <param name="asset">The asset data to update.</param>
        /// <param name="from">The pre-1.6 data to merge into the asset.</param>
        /// <param name="fromBackup">A copy of <paramref name="from"/> before edits were applied.</param>
        private void MergeIntoNewFormat(IDictionary<string, CropData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
        {
            // remove deleted entries
            foreach (string key in asset.Keys)
            {
                if (!from.ContainsKey(key))
                    asset.Remove(key);
            }

            // apply entries
            foreach ((string objectId, string fromEntry) in from)
            {
                // get/add target record
                bool isNew = false;
                if (!asset.TryGetValue(objectId, out CropData? entry))
                {
                    isNew = true;
                    entry = new CropData();
                }

                // get backup
                string[]? backupFields = null;
                if (fromBackup is not null)
                {
                    if (fromBackup.TryGetValue(objectId, out string? prevRow) && prevRow == fromEntry)
                        continue; // no changes
                    backupFields = prevRow?.Split('/');
                }

                // merge fields into new asset
                {
                    string[] fields = fromEntry.Split('/');

                    string rawDaysInPhase = ArgUtility.Get(fields, 0);
                    if (rawDaysInPhase != ArgUtility.Get(backupFields, 0))
                        this.MergDaysInPhaseIntoNewFormat(entry, rawDaysInPhase);

                    string rawSeasons = ArgUtility.Get(fields, 1);
                    if (rawSeasons != ArgUtility.Get(backupFields, 1))
                        this.MergeSeasonsIntoNewFormat(entry, rawSeasons);

                    entry.SpriteIndex = ArgUtility.GetInt(fields, 2, entry.SpriteIndex);
                    entry.HarvestItemId = ArgUtility.Get(fields, 3, entry.HarvestItemId, allowBlank: false);
                    entry.RegrowDays = ArgUtility.GetInt(fields, 4, entry.RegrowDays);
                    entry.HarvestMethod = ArgUtility.GetEnum(fields, 5, entry.HarvestMethod);

                    string rawExtraHarvestField = ArgUtility.Get(fields, 6);
                    if (rawExtraHarvestField != null && rawExtraHarvestField != ArgUtility.Get(backupFields, 6))
                        this.MergeExtraHarvestIntoNewFormat(entry, rawExtraHarvestField);

                    entry.IsRaised = ArgUtility.GetBool(fields, 7, entry.IsRaised);

                    string rawColors = ArgUtility.Get(fields, 8);
                    if (rawColors != null && rawColors != ArgUtility.Get(backupFields, 8))
                        this.MergeOldTintColorsFieldIntoNewFormat(entry, rawColors);
                }

                // set value
                if (isNew)
                    asset[objectId] = entry;
            }
        }

        /// <summary>Get the pre-1.6 'seasons' field from the new asset.</summary>
        /// <param name="entry">The crop data to convert.</param>
        private string GetOldSeasonsField(CropData entry)
        {
            return entry.Seasons is not null
                ? string.Join(' ', entry.Seasons.Select(p => p.ToString().ToLower()))
                : string.Empty;
        }

        /// <summary>Merge a pre-1.6 'extra harvest' field into the new asset.</summary>
        /// <param name="entry">The data entry to update.</param>
        /// <param name="field">The pre-1.6 field value to merge into the asset.</param>
        private void MergeSeasonsIntoNewFormat(CropData entry, string field)
        {
            entry.Seasons?.Clear();
            entry.Seasons ??= new();

            foreach (string rawSeason in field.Split(' '))
            {
                if (Utility.TryParseEnum(rawSeason, out Season season))
                    entry.Seasons.Add(season);
            }
        }

        /// <summary>Merge a pre-1.6 'days in phase' field into the new asset.</summary>
        /// <param name="entry">The data entry to update.</param>
        /// <param name="field">The pre-1.6 field value to merge into the asset.</param>
        private void MergDaysInPhaseIntoNewFormat(CropData entry, string field)
        {
            entry.DaysInPhase?.Clear();
            entry.DaysInPhase ??= new();

            foreach (string rawDay in field.Split(' '))
            {
                int.TryParse(rawDay, out int day);
                entry.DaysInPhase.Add(day);
            }
        }

        /// <summary>Get the pre-1.6 'extra harvest' field from the new asset.</summary>
        /// <summary>Get the extra harvest info converted to the 1.5.6 format.</summary>
        /// <param name="entry">The crop data to convert.</param>
        private string GetOldExtraHarvestField(CropData entry)
        {
            return entry.HarvestMaxStack > entry.HarvestMinStack || entry.HarvestMaxIncreasePerFarmingLevel > 0 || entry.ExtraHarvestChance > 0
                ? $"true {entry.HarvestMinStack} {entry.HarvestMaxStack} {entry.HarvestMaxIncreasePerFarmingLevel.ToString().TrimStart('0')} {entry.ExtraHarvestChance.ToString().TrimStart('0')}"
                : "false";
        }

        /// <summary>Merge a pre-1.6 'extra harvest' field into the new asset.</summary>
        /// <param name="entry">The data entry to update.</param>
        /// <param name="field">The pre-1.6 field value to merge into the asset.</param>
        private void MergeExtraHarvestIntoNewFormat(CropData entry, string field)
        {
            string[] fields = field.Split(' ');
            if (!ArgUtility.TryGetBool(fields, 0, out bool applies, out _))
                return;

            if (applies)
            {
                if (
                    ArgUtility.TryGetInt(fields, 1, out int minStack, out _)
                    && ArgUtility.TryGetInt(fields, 2, out int maxStack, out _)
                    && ArgUtility.TryGetInt(fields, 3, out int maxIncreasePerFarmingLevel, out _)
                    && ArgUtility.TryGetFloat(fields, 4, out float extraChance, out _)
                )
                {
                    entry.HarvestMinStack = minStack;
                    entry.HarvestMaxStack = maxStack;
                    entry.HarvestMaxIncreasePerFarmingLevel = maxIncreasePerFarmingLevel;
                    entry.ExtraHarvestChance = extraChance;
                }
            }
            if (!applies)
            {
                entry.HarvestMaxStack = 1;
                entry.HarvestMaxIncreasePerFarmingLevel = 0;
                entry.ExtraHarvestChance = 0;
            }
        }

        /// <summary>Get the pre-1.6 'tint colors' field from the new asset.</summary>
        /// <param name="entry">The crop data to convert.</param>
        private string GetOldTintColorsField(CropData entry)
        {
            if (entry.TintColors?.Count is not > 0)
                return "false";

            StringBuilder result = new("true");

            foreach (string rawColor in entry.TintColors)
            {
                Color? color = Utility.StringToColor(rawColor);
                if (color is null)
                    continue;

                result
                    .Append(' ')
                    .Append(color.Value.R)
                    .Append(' ')
                    .Append(color.Value.G)
                    .Append(' ')
                    .Append(color.Value.B);
            }

            return result.ToString();
        }

        /// <summary>Merge a pre-1.6 'tint colors' field into the new asset.</summary>
        /// <param name="entry">The data entry to update.</param>
        /// <param name="field">The pre-1.6 field value to merge into the asset.</param>
        private void MergeOldTintColorsFieldIntoNewFormat(CropData entry, string field)
        {
            string[] fields = field.Split(' ');
            if (!ArgUtility.TryGetBool(fields, 0, out bool applies, out _))
                return;

            if (applies)
            {
                // extract colors
                HashSet<string> newColors = [];
                for (int i = 1; i < fields.Length - 3; i += 3)
                    newColors.Add($"{fields[i]} {fields[i + 1]} {fields[i + 2]}");

                // remove or update existing entries
                if (entry.TintColors?.Count > 0)
                {
                    for (int i = 0; i < entry.TintColors.Count; i++)
                    {
                        Color? color = Utility.StringToColor(entry.TintColors[i]);
                        string? oldColorCode = color.HasValue
                            ? $"{color.Value.R} {color.Value.G} {color.Value.B}"
                            : null;

                        // remove if not in new colors
                        if (oldColorCode is null || !newColors.Remove(oldColorCode))
                        {
                            entry.TintColors.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                }

                // add any remaining colors
                if (newColors.Count > 0)
                {
                    entry.TintColors ??= new();
                    entry.TintColors.AddRange(newColors);
                }
            }
            else
            {
                entry.TintColors?.Clear();
            }
        }
    }
}
