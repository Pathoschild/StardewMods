using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Content;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewTokenParser = StardewValley.TokenizableStrings.TokenParser;

namespace ContentPatcher.Framework.Migrations;

internal partial class Migration_2_0 : BaseRuntimeMigration
{
    /// <summary>The migration logic to apply pre-1.6 <c>Data/Blueprints</c> patches to <c>Data/Buildings</c>.</summary>
    private class BlueprintsMigrator : IEditAssetMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The pre-1.6 asset name.</summary>
        private const string OldAssetName = "Data/Blueprints";

        /// <summary>The 1.6 asset name.</summary>
        private const string NewAssetName = "Data/Buildings";

        /// <summary>The building IDs added in Stardew Valley 1.6.</summary>
        private readonly HashSet<string> BuildingIdsAddedIn16 = ["Cabin", "Pet Bowl", "Farmhouse"];

        /// <summary>The vanilla data without mod edits applied, used as the base when a pre-1.6 content pack loads the asset.</summary>
        private readonly VanillaAssetFactory<Dictionary<string, BuildingData>> OriginalData = new(DataLoader.Buildings);


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool AppliesTo(IAssetName assetName)
        {
            return assetName?.IsEquivalentTo(BlueprintsMigrator.OldAssetName, useBaseName: true) is true;
        }

        /// <inheritdoc />
        public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            return new AssetName(BlueprintsMigrator.NewAssetName, null, null);
        }

        /// <inheritdoc />
        public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
        {
            var data = this.OriginalData.GetFreshCopy();
            var dataBackup = this.GetOldFormat(data);

            var legacyLoad = patch.Load<Dictionary<string, string>>(this.GetOldAssetName(assetName));
            this.MergeIntoNewFormat(data, legacyLoad, dataBackup);

            asset = (T)(object)data;

            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
        {
            var data = asset.GetData<Dictionary<string, BuildingData>>();
            Dictionary<string, string> tempData = this.GetOldFormat(data);
            Dictionary<string, string> tempDataBackup = new(tempData);
            patch.Edit<Dictionary<string, string>>(new FakeAssetData(asset, this.GetOldAssetName(asset.Name), tempData), onWarning);
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
            return new AssetName(BlueprintsMigrator.OldAssetName, newName.LocaleCode, newName.LanguageCode);
        }

        /// <summary>Get the pre-1.6 equivalent for the new asset data.</summary>
        /// <param name="from">The data to convert.</param>
        private Dictionary<string, string> GetOldFormat(IDictionary<string, BuildingData> from)
        {
            var data = new Dictionary<string, string>();

            string[] fields = new string[19];
            foreach ((string key, BuildingData entry) in from)
            {
                fields[0] = this.GetOldItemsRequiredField(entry);
                fields[1] = entry.Size.X.ToString();
                fields[2] = entry.Size.Y.ToString();
                fields[3] = entry.HumanDoor.X.ToString();
                fields[4] = entry.HumanDoor.Y.ToString();
                fields[5] = entry.AnimalDoor.X.ToString();
                fields[6] = entry.AnimalDoor.Y.ToString();
                fields[7] = entry.IndoorMap;
                fields[8] = StardewTokenParser.ParseText(entry.Name);
                fields[9] = StardewTokenParser.ParseText(entry.Description);
                fields[10] = "Buildings"; // unused (blueprintType)
                fields[11] = "none";      // unused (nameOfBuildingToUpgrade)
                fields[12] = "0";         // unused (sourceRectForMenuView.X)
                fields[13] = "0";         // unused (sourceRectForMenuView.Y)
                fields[14] = entry.MaxOccupants.ToString();
                fields[15] = "null";      // unused (actionBehavior)
                fields[16] = "Farm";      // unused (locations)
                fields[17] = entry.BuildCost.ToString();
                fields[18] = entry.MagicalConstruction.ToString().ToLowerInvariant();

                data[key] = string.Join('/', fields);
            }

            return data;
        }

        /// <summary>Merge pre-1.6 data into the new asset.</summary>
        /// <param name="asset">The asset data to update.</param>
        /// <param name="from">The pre-1.6 data to merge into the asset.</param>
        /// <param name="fromBackup">A copy of <paramref name="from"/> before edits were applied.</param>
        private void MergeIntoNewFormat(IDictionary<string, BuildingData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
        {
            // remove deleted entries
            foreach (string key in asset.Keys)
            {
                if (!from.ContainsKey(key))
                {
                    if (this.BuildingIdsAddedIn16.Contains(key))
                        continue; // don't remove 1.6 content for a pre-1.6 content pack (usually a load patch)

                    asset.Remove(key);
                }
            }

            // apply entries
            foreach ((string key, string fromEntry) in from)
            {
                // get/add target record
                bool isNew = false;
                if (!asset.TryGetValue(key, out BuildingData? entry))
                {
                    isNew = true;
                    entry = new BuildingData
                    {
                        Name = key,
                        Description = "...",
                        Texture = $"Buildings\\{key}"
                    };
                }

                // get backup
                string[]? backupFields = null;
                if (fromBackup is not null)
                {
                    if (fromBackup.TryGetValue(key, out string? prevRow) && prevRow == fromEntry)
                        continue; // no changes
                    backupFields = prevRow?.Split('/');
                }

                // merge fields into new asset
                {
                    string[] fields = fromEntry.Split('/');

                    string rawItemsRequired = ArgUtility.Get(fields, 0);
                    if (rawItemsRequired != ArgUtility.Get(backupFields, 0))
                        this.MergeItemsRequiredFieldIntoNewFormat(entry, rawItemsRequired);

                    entry.Size = new Point(
                        ArgUtility.GetInt(fields, 1, entry.Size.X),
                        ArgUtility.GetInt(fields, 2, entry.Size.Y)
                    );
                    entry.HumanDoor = new Point(
                        ArgUtility.GetInt(fields, 3, entry.HumanDoor.X),
                        ArgUtility.GetInt(fields, 4, entry.HumanDoor.Y)
                    );
                    entry.AnimalDoor = new Rectangle(
                        ArgUtility.GetInt(fields, 5, entry.AnimalDoor.X),
                        ArgUtility.GetInt(fields, 6, entry.AnimalDoor.Y),
                        1,
                        1
                    );

                    entry.IndoorMap = ArgUtility.Get(fields, 7, entry.IndoorMap, allowBlank: false);
                    if (string.IsNullOrWhiteSpace(entry.IndoorMap) || entry.IndoorMap == "null")
                        entry.IndoorMap = null;

                    entry.Name = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(fields, 8), ArgUtility.Get(backupFields, 8), entry.Name);
                    entry.Description = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(fields, 9), ArgUtility.Get(backupFields, 9), entry.Description);
                    entry.MaxOccupants = ArgUtility.GetInt(fields, 14, entry.MaxOccupants);
                    entry.BuildCost = ArgUtility.GetInt(fields, 17, entry.BuildCost);
                    entry.MagicalConstruction = ArgUtility.GetBool(fields, 18, entry.MagicalConstruction);
                }

                // set value
                if (isNew)
                    asset[key] = entry;
            }
        }

        /// <summary>Get the pre-1.6 'items required' field for the new asset data.</summary>
        /// <param name="data">The building data.</param>
        private string GetOldItemsRequiredField(BuildingData data)
        {
            if (data.BuildMaterials?.Count is not > 0)
                return string.Empty;

            StringBuilder result = new();

            foreach (BuildingMaterial material in data.BuildMaterials)
            {
                result
                    .Append(RuntimeMigrationHelper.ParseObjectId(material.ItemId) ?? material.ItemId)
                    .Append(' ')
                    .Append(material.Amount)
                    .Append(' ');
            }

            return result.ToString(0, result.Length - 1);
        }

        /// <summary>Merge a pre-1.6 'items required' field into the new asset data.</summary>
        /// <param name="data">The asset entry.</param>
        /// <param name="field">The field value.</param>
        private void MergeItemsRequiredFieldIntoNewFormat(BuildingData data, string field)
        {
            string[] fields = field.Split(' ');

            // build list
            Dictionary<string, int> materials = [];
            for (int i = 0; i < fields.Length - 1; i += 2)
            {
                string itemId = ArgUtility.Get(fields, i, allowBlank: false);
                int count = ArgUtility.GetInt(fields, i + 1, 1);

                if (itemId != null)
                    materials[itemId] = count;
            }

            // step 1: remove or update existing entries
            if (data.BuildMaterials?.Count > 0)
            {
                for (int i = 0; i < data.BuildMaterials.Count; i++)
                {
                    var material = data.BuildMaterials[i];
                    string itemId = RuntimeMigrationHelper.ParseObjectId(material.ItemId) ?? ItemRegistry.QualifyItemId(material.ItemId);

                    // remove if deleted
                    if (!materials.TryGetValue(itemId, out int count))
                    {
                        data.BuildMaterials.RemoveAt(i);
                        i--;
                    }

                    // else update
                    else
                    {
                        material.Amount = count;
                        materials.Remove(itemId);
                    }
                }
            }

            // step 2: add any remaining as new entries
            if (materials.Count > 0)
            {
                data.BuildMaterials ??= new();

                foreach ((string itemId, int amount) in materials)
                {
                    string qualifiedItemId = ItemRegistry.ManuallyQualifyItemId(itemId, ItemRegistry.type_object);

                    data.BuildMaterials.Add(new()
                    {
                        ItemId = qualifiedItemId,
                        Amount = amount
                    });
                }
            }
        }
    }
}
