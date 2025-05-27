using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Content;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Objects;
using StardewTokenParser = StardewValley.TokenizableStrings.TokenParser;

namespace ContentPatcher.Framework.Migrations;

internal partial class Migration_2_0 : BaseRuntimeMigration
{
    /// <summary>The migration logic to apply pre-1.6 <c>Data/ObjectInformation</c> patches to <c>Data/Objects</c>.</summary>
    private class ObjectInformationMigrator : IEditAssetMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The pre-1.6 asset name.</summary>
        private const string OldAssetName = "Data/ObjectInformation";

        /// <summary>The 1.6 asset name.</summary>
        private const string NewAssetName = "Data/Objects";

        /// <summary>The numeric object IDs added in Stardew Valley 1.6.</summary>
        private readonly HashSet<string> NumericIdsAddedIn16 = ["6", "8", "10", "12", "14", "32", "34", "36", "38", "40", "42", "44", "46", "48", "50", "52", "54", "56", "58", "742"];

        /// <summary>The vanilla data without mod edits applied, used as the base when a pre-1.6 content pack loads the asset.</summary>
        private readonly VanillaAssetFactory<Dictionary<string, ObjectData>> OriginalData = new(DataLoader.Objects);


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool AppliesTo(IAssetName assetName)
        {
            return assetName?.IsEquivalentTo(ObjectInformationMigrator.OldAssetName, useBaseName: true) is true;
        }

        /// <inheritdoc />
        public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            return new AssetName(ObjectInformationMigrator.NewAssetName, null, null);
        }

        /// <inheritdoc />
        public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
        {
            var data = this.OriginalData.GetFreshCopy();
            var dataBackup = this.GetOldFormat(data);

            var legacyLoad = patch.Load<Dictionary<string, string>>(this.GetOldAssetName(assetName));
            this.MergeIntoNewFormat(data, legacyLoad, dataBackup, patch.ContentPack.Manifest.UniqueID);

            asset = (T)(object)data;

            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
        {
            var data = asset.GetData<Dictionary<string, ObjectData>>();
            Dictionary<string, string> tempData = this.GetOldFormat(data);
            Dictionary<string, string> tempDataBackup = new(tempData);
            patch.Edit<Dictionary<string, string>>(new FakeAssetData(asset, this.GetOldAssetName(asset.Name), tempData), onWarning);
            this.MergeIntoNewFormat(data, tempData, tempDataBackup, patch.ContentPack.Manifest.UniqueID);

            error = null;
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the old asset to edit.</summary>
        /// <param name="newName">The new asset name whose locale to use.</param>
        private IAssetName GetOldAssetName(IAssetName newName)
        {
            return new AssetName(ObjectInformationMigrator.OldAssetName, newName.LocaleCode, newName.LanguageCode);
        }

        /// <summary>Get the pre-1.6 equivalent for the new asset data.</summary>
        /// <param name="from">The data to convert.</param>
        private Dictionary<string, string> GetOldFormat(IDictionary<string, ObjectData> from)
        {
            var data = new Dictionary<string, string>();

            string[] fields = new string[9];
            foreach ((string objectId, ObjectData entry) in from)
            {
                ObjectBuffData? buff = entry.Buffs?.FirstOrDefault();

                BuffEffects? buffEffects = buff?.CustomAttributes != null
                    ? new(buff.CustomAttributes)
                    : null;

                fields[0] = entry.Name;
                fields[1] = entry.Price.ToString();
                fields[2] = entry.Edibility.ToString();
                fields[3] = $"{entry.Type} {entry.Category}";
                fields[4] = StardewTokenParser.ParseText(entry.DisplayName);
                fields[5] = StardewTokenParser.ParseText(entry.Description);
                fields[6] = this.GetOldMiscellaneousField(objectId, entry);
                fields[7] = buffEffects?.HasAnyValue() is true ? string.Join(" ", buffEffects.ToLegacyAttributeFormat()) : string.Empty;
                fields[8] = buff?.Duration.ToString() ?? string.Empty;

                data[objectId] = string.Join('/', fields);
            }

            return data;
        }

        /// <summary>Merge pre-1.6 data into the new asset.</summary>
        /// <param name="asset">The asset data to update.</param>
        /// <param name="from">The pre-1.6 data to merge into the asset.</param>
        /// <param name="fromBackup">A copy of <paramref name="from"/> before edits were applied.</param>
        /// <param name="modId">The unique ID for the mod, used in auto-generated entry IDs.</param>
        private void MergeIntoNewFormat(IDictionary<string, ObjectData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup, string modId)
        {
            // remove deleted entries
            foreach (string key in asset.Keys)
            {
                if (!from.ContainsKey(key))
                {
                    if (!int.TryParse(key, out _) || this.NumericIdsAddedIn16.Contains(key))
                        continue; // don't remove 1.6 content for a pre-1.6 content pack (usually a load patch)

                    asset.Remove(key);
                }
            }

            // apply entries
            foreach ((string objectId, string fromEntry) in from)
            {
                // get/add target record
                bool isNew = false;
                if (!asset.TryGetValue(objectId, out ObjectData? entry))
                {
                    isNew = true;
                    entry = new ObjectData();
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

                    // main fields
                    entry.Name = ArgUtility.Get(fields, 0, entry.Name, allowBlank: false);
                    entry.Price = ArgUtility.GetInt(fields, 1, entry.Price);
                    entry.Edibility = ArgUtility.GetInt(fields, 2, entry.Edibility);

                    // type & category
                    string rawTypeAndCategory = ArgUtility.Get(fields, 3);
                    if (!string.IsNullOrWhiteSpace(rawTypeAndCategory) && rawTypeAndCategory != ArgUtility.Get(backupFields, 3))
                    {
                        string[] parts = rawTypeAndCategory.Split(' ', 2);

                        entry.Type = parts[0];
                        entry.Category = ArgUtility.GetInt(parts, 1, entry.Category);
                    }

                    // display text
                    entry.DisplayName = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(fields, 4), ArgUtility.Get(backupFields, 4), entry.DisplayName);
                    entry.Description = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(fields, 5), ArgUtility.Get(backupFields, 5), StardewTokenParser.ParseText(entry.Description));

                    // miscellaneous
                    string rawMiscellaneous = ArgUtility.Get(fields, 6);
                    if (rawMiscellaneous != ArgUtility.Get(backupFields, 6))
                        this.MergeMiscellaneousFieldIntoNewFormat(objectId, entry, rawMiscellaneous, modId);

                    // buff effects
                    string rawBuffEffects = ArgUtility.Get(fields, 7);
                    if (rawBuffEffects != null && rawBuffEffects != ArgUtility.Get(backupFields, 7))
                        this.MergeBuffFieldIntoNewFormat(entry, rawBuffEffects);

                    // buff duration
                    if (entry.Buffs?.Count > 0)
                        entry.Buffs[0].Duration = ArgUtility.GetInt(fields, 8, entry.Buffs[0].Duration);
                }

                // set value
                if (isNew)
                    asset[objectId] = entry;
            }
        }

        /// <summary>Get the pre-1.6 'miscellaneous' field for the new object data.</summary>
        /// <param name="objectId">The object ID.</param>
        /// <param name="data">The object data.</param>
        private string GetOldMiscellaneousField(string objectId, ObjectData data)
        {
            // geode
            if (objectId is "275" or "535" or "536" or "537" or "749")
            {
                if (data.GeodeDrops is null)
                    return string.Empty;

                return string.Join(
                    ' ',
                    from drop in data.GeodeDrops
                    where GameStateQuery.IsImmutablyTrue(drop.Condition)

                    let itemId = RuntimeMigrationHelper.ParseObjectId(drop.ItemId)
                    where itemId != null

                    select itemId
                );
            }

            // artifact
            if (data.Type == "Arch")
            {
                if (data.ArtifactSpotChances is null)
                    return string.Empty;

                return string.Join(' ', data.ArtifactSpotChances.Select(p => $"{p.Key} {p.Value}"));
            }

            // food/drink
            if (data.Buffs?.Count > 0)
                return data.IsDrink ? "drink" : "food";

            // none
            return string.Empty;
        }

        /// <summary>Merge a pre-1.6 'buff' field into the new object data.</summary>
        /// <param name="entry">The object data.</param>
        /// <param name="field">The field value.</param>
        private void MergeBuffFieldIntoNewFormat(ObjectData entry, string field)
        {
            string[] fields = field.Split(' ');

            ObjectBuffData buff = new ObjectBuffData() { BuffId = entry.IsDrink ? "drink" : "food" };
            buff.CustomAttributes ??= new BuffAttributesData();

            BuffAttributesData effects = buff.CustomAttributes;
            effects.FarmingLevel = ArgUtility.GetFloat(fields, 0, effects.FarmingLevel);
            effects.FishingLevel = ArgUtility.GetFloat(fields, 1, effects.FishingLevel);
            effects.MiningLevel = ArgUtility.GetFloat(fields, 2, effects.MiningLevel);
            effects.LuckLevel = ArgUtility.GetFloat(fields, 4, effects.LuckLevel);
            effects.ForagingLevel = ArgUtility.GetFloat(fields, 5, effects.ForagingLevel);
            effects.MaxStamina = ArgUtility.GetFloat(fields, 7, effects.MaxStamina);
            effects.MagneticRadius = ArgUtility.GetFloat(fields, 8, effects.MagneticRadius);
            effects.Speed = ArgUtility.GetFloat(fields, 9, effects.Speed);
            effects.Defense = ArgUtility.GetFloat(fields, 10, effects.Defense);
            effects.Attack = ArgUtility.GetFloat(fields, 11, effects.Attack);

            if (new BuffEffects(buff.CustomAttributes).HasAnyValue())
            {
                entry.Buffs ??= new();
                entry.Buffs.Add(buff);
            }
            else
                entry.Buffs = null;
        }

        /// <summary>Merge a pre-1.6 'miscellaneous' field into the new object data.</summary>
        /// <param name="objectId">The object ID.</param>
        /// <param name="data">The object data.</param>
        /// <param name="miscellaneous">The miscellaneous field value.</param>
        /// <param name="modId">The unique ID for the mod, used in auto-generated entry IDs.</param>
        private void MergeMiscellaneousFieldIntoNewFormat(string objectId, ObjectData data, string miscellaneous, string modId)
        {
            // geode
            if (objectId is "275" or "535" or "536" or "537" or "749")
            {
                HashSet<string> dropIds = new(miscellaneous.Split(' '));

                // step 1: remove existing entries
                if (data.GeodeDrops != null)
                {
                    for (int i = 0; i < data.GeodeDrops.Count; i++)
                    {
                        var entry = data.GeodeDrops[i];

                        // parse entry
                        string? curObjectId = RuntimeMigrationHelper.ParseObjectId(entry.ItemId);
                        if (curObjectId is null || !GameStateQuery.IsImmutablyTrue(entry.Condition))
                            continue;

                        // remove if deleted
                        if (!dropIds.Contains(curObjectId))
                        {
                            data.GeodeDrops.RemoveAt(i);
                            i--;
                            continue;
                        }

                        dropIds.Remove(curObjectId);
                    }
                }

                // step 2: add any remaining as new entries
                if (dropIds.Count > 0)
                {
                    data.GeodeDrops ??= new();

                    foreach (string dropId in dropIds)
                    {
                        string qualifiedItemId = ItemRegistry.type_object + dropId;

                        data.GeodeDrops.Add(
                            new ObjectGeodeDropData
                            {
                                Id = $"{modId}_{qualifiedItemId}",
                                ItemId = qualifiedItemId
                            }
                        );
                    }
                }
            }

            // artifact
            if (data.Type == "Arch")
            {
                // parse artifact field
                Dictionary<string, double> drops = [];
                {
                    string[] fields = miscellaneous.Split(' ');
                    for (int i = 0; i < fields.Length - 1; i += 2)
                    {
                        string dropId = fields[i];
                        if (double.TryParse(fields[i + 1], out double chance))
                            drops[dropId] = chance;
                    }
                }

                // step 1: remove or update existing entries
                if (data.ArtifactSpotChances?.Count > 0)
                {
                    foreach (string dropId in data.ArtifactSpotChances.Keys)
                    {
                        // remove if deleted
                        if (!drops.TryGetValue(dropId, out double chance))
                            data.ArtifactSpotChances.Remove(dropId);

                        // else update
                        else
                        {
                            data.ArtifactSpotChances[dropId] = (float)chance;
                            drops.Remove(dropId);
                        }
                    }
                }

                // step 2: add any remaining as new entries
                if (drops.Count > 0)
                {
                    data.ArtifactSpotChances ??= new();

                    foreach ((string dropId, double chance) in drops)
                    {
                        string qualifiedItemId = ItemRegistry.type_object + dropId;

                        data.ArtifactSpotChances[dropId] = (float)chance;
                    }
                }
            }
        }
    }
}
