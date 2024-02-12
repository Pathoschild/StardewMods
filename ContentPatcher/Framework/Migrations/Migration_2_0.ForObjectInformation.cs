using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Content;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Objects;
using StardewTokenParser = StardewValley.TokenizableStrings.TokenParser;

namespace ContentPatcher.Framework.Migrations
{
    internal partial class Migration_2_0 : BaseRuntimeMigration
    {
        /// <summary>The migration logic to apply pre-1.6 <c>Data/ObjectInformation</c> patches to <c>Data/Objects</c>.</summary>
        public class ObjectInformationMigrator : IEditAssetMigrator
        {
            /*********
            ** Fields
            *********/
            /// <summary>The pre-1.6 asset name.</summary>
            private const string OldAssetName = "Data/ObjectInformation";

            /// <summary>The 1.6 asset name.</summary>
            private const string NewAssetName = "Data/Objects";

            /// <summary>Get the unqualified object ID, if it's a valid object ID.</summary>
            private readonly Func<string, string?> ParseObjectId;


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="parseObjectId">Get the unqualified object ID, if it's a valid object ID.</param>
            public ObjectInformationMigrator(Func<string, string?> parseObjectId)
            {
                this.ParseObjectId = parseObjectId;
            }

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
                Dictionary<string, string> tempData = patch.Load<Dictionary<string, string>>(this.GetOldAssetName(assetName));
                Dictionary<string, ObjectData> newData = new();
                this.MergeIntoNewFormat(newData, tempData, null, patch.ContentPack.Manifest.UniqueID);
                asset = (T)(object)newData;

                error = null;
                return true;
            }

            /// <inheritdoc />
            public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, out string? error)
            {
                var data = asset.GetData<Dictionary<string, ObjectData>>();
                Dictionary<string, string> tempData = this.GetOldFormat(data);
                Dictionary<string, string> tempDataBackup = new(tempData);
                patch.Edit<Dictionary<string, string>>(new FakeAssetData(asset, this.GetOldAssetName(asset.Name), tempData));
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
                    BuffEffects? buffEffects = entry.Buff?.CustomAttributes != null
                        ? new(entry.Buff?.CustomAttributes)
                        : null;

                    fields[0] = entry.Name;
                    fields[1] = entry.Price.ToString();
                    fields[2] = entry.Edibility.ToString();
                    fields[3] = $"{entry.Type} {entry.Category}";
                    fields[4] = StardewTokenParser.ParseText(entry.DisplayName);
                    fields[5] = StardewTokenParser.ParseText(entry.Description);
                    fields[6] = this.GetOldMiscellaneousField(objectId, entry);
                    fields[7] = buffEffects?.HasAnyValue() is true ? string.Join(" ", buffEffects.ToLegacyAttributeFormat()) : string.Empty;
                    fields[8] = entry.Buff?.Duration.ToString() ?? string.Empty;

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
                        asset.Remove(key);
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

                        // display name
                        string displayName = ArgUtility.Get(fields, 4);
                        if (!string.IsNullOrWhiteSpace(displayName) && displayName != ArgUtility.Get(backupFields, 4) && displayName != StardewTokenParser.ParseText(entry.DisplayName))
                            entry.DisplayName = displayName;

                        // description
                        string description = ArgUtility.Get(fields, 5);
                        if (!string.IsNullOrWhiteSpace(description) && description != ArgUtility.Get(backupFields, 5) && description != StardewTokenParser.ParseText(entry.Description))
                            entry.Description = description;

                        // miscellaneous
                        string rawMiscellaneous = ArgUtility.Get(fields, 6);
                        if (rawMiscellaneous != ArgUtility.Get(backupFields, 6))
                            this.MergeMiscellaneousFieldIntoNewFormat(objectId, entry, rawMiscellaneous, modId);

                        // buff effects
                        string rawBuffEffects = ArgUtility.Get(fields, 7);
                        if (rawBuffEffects != null && rawBuffEffects != ArgUtility.Get(backupFields, 7))
                            this.MergeBuffFieldIntoNewFormat(entry, rawBuffEffects);

                        // buff duration
                        if (entry.Buff != null)
                            entry.Buff.Duration = ArgUtility.GetInt(fields, 8, entry.Buff.Duration);
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

                        let itemId = this.ParseObjectId(drop.ItemId)
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
                if (data.Buff != null)
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

                entry.Buff ??= new ObjectBuffData() { BuffId = entry.IsDrink ? "drink" : "food" };
                entry.Buff.CustomAttributes ??= new BuffAttributesData();

                BuffAttributesData effects = entry.Buff.CustomAttributes;
                effects.FarmingLevel = ArgUtility.GetInt(fields, 0, effects.FarmingLevel);
                effects.FishingLevel = ArgUtility.GetInt(fields, 1, effects.FishingLevel);
                effects.MiningLevel = ArgUtility.GetInt(fields, 2, effects.MiningLevel);
                effects.LuckLevel = ArgUtility.GetInt(fields, 4, effects.LuckLevel);
                effects.ForagingLevel = ArgUtility.GetInt(fields, 5, effects.ForagingLevel);
                effects.MaxStamina = ArgUtility.GetInt(fields, 7, effects.MaxStamina);
                effects.MagneticRadius = ArgUtility.GetInt(fields, 8, effects.MagneticRadius);
                effects.Speed = ArgUtility.GetInt(fields, 9, effects.Speed);
                effects.Defense = ArgUtility.GetInt(fields, 10, effects.Defense);
                effects.Attack = ArgUtility.GetInt(fields, 11, effects.Attack);
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
                    HashSet<string> dropIds = new HashSet<string>(miscellaneous.Split(' '));

                    // step 1: remove existing entries
                    if (data.GeodeDrops != null)
                    {
                        for (int i = 0; i < data.GeodeDrops.Count; i++)
                        {
                            var entry = data.GeodeDrops[i];

                            // parse entry
                            string? curObjectId = this.ParseObjectId(entry.ItemId);
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
                    Dictionary<string, double> drops = new();
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
                    if (data.ArtifactSpotChances.Count > 0)
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
}
