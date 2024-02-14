using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Content;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using SObject = StardewValley.Object;
using StardewTokenParser = StardewValley.TokenizableStrings.TokenParser;

namespace ContentPatcher.Framework.Migrations
{
    internal partial class Migration_2_0 : BaseRuntimeMigration
    {
        /// <summary>The migration logic to apply pre-1.6 <c>Data/BigCraftableInformation</c> patches to <c>Data/BigCraftables</c>.</summary>
        private class BigCraftableInformationMigrator : IEditAssetMigrator
        {
            /*********
            ** Fields
            *********/
            /// <summary>The pre-1.6 asset name.</summary>
            private const string OldAssetName = "Data/BigCraftablesInformation";

            /// <summary>The 1.6 asset name.</summary>
            private const string NewAssetName = "Data/BigCraftables";


            /*********
            ** Public methods
            *********/
            /// <inheritdoc />
            public bool AppliesTo(IAssetName assetName)
            {
                return assetName?.IsEquivalentTo(BigCraftableInformationMigrator.OldAssetName, useBaseName: true) is true;
            }

            /// <inheritdoc />
            public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
            {
                return new AssetName(BigCraftableInformationMigrator.NewAssetName, null, null);
            }

            /// <inheritdoc />
            public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
            {
                Dictionary<string, string> tempData = patch.Load<Dictionary<string, string>>(this.GetOldAssetName(assetName));
                Dictionary<string, BigCraftableData> newData = new();
                this.MergeIntoNewFormat(newData, tempData, null);
                asset = (T)(object)newData;

                error = null;
                return true;
            }

            /// <inheritdoc />
            public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, out string? error)
            {
                var data = asset.GetData<Dictionary<string, BigCraftableData>>();
                Dictionary<string, string> tempData = this.GetOldFormat(data);
                Dictionary<string, string> tempDataBackup = new(tempData);
                patch.Edit<Dictionary<string, string>>(new FakeAssetData(asset, this.GetOldAssetName(asset.Name), tempData));
                this.MergeIntoNewFormat(data, tempData, tempDataBackup);

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
                return new AssetName(BigCraftableInformationMigrator.OldAssetName, newName.LocaleCode, newName.LanguageCode);
            }

            /// <summary>Get the pre-1.6 equivalent for the new asset data.</summary>
            /// <param name="from">The data to convert.</param>
            private Dictionary<string, string> GetOldFormat(IDictionary<string, BigCraftableData> from)
            {
                var data = new Dictionary<string, string>();

                string[] fields = new string[10];
                foreach ((string objectId, BigCraftableData entry) in from)
                {
                    fields[0] = entry.Name;
                    fields[1] = entry.Price.ToString();
                    fields[2] = SObject.inedible.ToString(); // edibility (removed in 1.6)
                    fields[3] = "Crafting -9";               // type + category (removed in 1.6)
                    fields[4] = StardewTokenParser.ParseText(entry.Description);
                    fields[5] = entry.CanBePlacedOutdoors.ToString().ToLowerInvariant();
                    fields[6] = entry.CanBePlacedIndoors.ToString().ToLowerInvariant();
                    fields[7] = entry.Fragility.ToString();
                    fields[8] = entry.IsLamp.ToString().ToLowerInvariant();
                    fields[9] = StardewTokenParser.ParseText(entry.DisplayName);

                    data[objectId] = string.Join('/', fields);
                }

                return data;
            }

            /// <summary>Merge pre-1.6 data into the new asset.</summary>
            /// <param name="asset">The asset data to update.</param>
            /// <param name="from">The pre-1.6 data to merge into the asset.</param>
            /// <param name="fromBackup">A copy of <paramref name="from"/> before edits were applied.</param>
            private void MergeIntoNewFormat(IDictionary<string, BigCraftableData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
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
                    if (!asset.TryGetValue(objectId, out BigCraftableData? entry))
                    {
                        isNew = true;
                        entry = new BigCraftableData();
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

                        entry.Name = ArgUtility.Get(fields, 0, entry.Name, allowBlank: false);
                        entry.Price = ArgUtility.GetInt(fields, 1, entry.Price);

                        string description = ArgUtility.Get(fields, 4);
                        if (!string.IsNullOrWhiteSpace(description) && description != ArgUtility.Get(backupFields, 4) && description != StardewTokenParser.ParseText(entry.Description))
                            entry.Description = description;

                        entry.CanBePlacedOutdoors = ArgUtility.GetBool(fields, 5, entry.CanBePlacedOutdoors);
                        entry.CanBePlacedIndoors = ArgUtility.GetBool(fields, 6, entry.CanBePlacedIndoors);
                        entry.Fragility = ArgUtility.GetInt(fields, 7, entry.Fragility);
                        entry.IsLamp = ArgUtility.GetBool(fields, 8, entry.IsLamp);

                        string displayName = ArgUtility.Get(fields, 9);
                        if (!string.IsNullOrWhiteSpace(displayName) && displayName != ArgUtility.Get(backupFields, 9) && displayName != StardewTokenParser.ParseText(entry.DisplayName))
                            entry.DisplayName = displayName;
                    }

                    // set value
                    if (isNew)
                        asset[objectId] = entry;
                }
            }
        }
    }
}
