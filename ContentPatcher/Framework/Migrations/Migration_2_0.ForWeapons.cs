using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Weapons;
using StardewTokenParser = StardewValley.TokenizableStrings.TokenParser;

namespace ContentPatcher.Framework.Migrations;

internal partial class Migration_2_0 : BaseRuntimeMigration
{
    /// <summary>The migration logic to apply pre-1.6 <c>Data/Weapons</c> patches to the new format.</summary>
    private class WeaponsMigrator : IEditAssetMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The asset name.</summary>
        private const string AssetName = "Data/Weapons";

        /// <summary>The numeric object IDs added in Stardew Valley 1.6.</summary>
        private readonly HashSet<string> NumericIdsAddedIn16 = ["65", "66"];

        /// <summary>The vanilla data without mod edits applied, used as the base when a pre-1.6 content pack loads the asset.</summary>
        private readonly VanillaAssetFactory<Dictionary<string, WeaponData>> OriginalData = new(DataLoader.Weapons);


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool AppliesTo(IAssetName assetName)
        {
            return assetName?.IsEquivalentTo(WeaponsMigrator.AssetName, useBaseName: true) is true;
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
            var data = asset.GetData<Dictionary<string, WeaponData>>();
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
        private Dictionary<string, string> GetOldFormat(IDictionary<string, WeaponData> from)
        {
            var data = new Dictionary<string, string>();

            string[] fields = new string[15];
            foreach ((string objectId, WeaponData entry) in from)
            {
                fields[0] = entry.Name;
                fields[1] = StardewTokenParser.ParseText(entry.Description);
                fields[2] = entry.MinDamage.ToString();
                fields[3] = entry.MaxDamage.ToString();
                fields[4] = entry.Knockback.ToString();
                fields[5] = entry.Speed.ToString();
                fields[6] = entry.Precision.ToString();
                fields[7] = entry.Defense.ToString();
                fields[8] = entry.Type.ToString();
                fields[9] = entry.MineBaseLevel.ToString();
                fields[10] = entry.MineMinLevel.ToString();
                fields[11] = entry.AreaOfEffect.ToString();
                fields[12] = entry.CritChance.ToString();
                fields[13] = entry.CritMultiplier.ToString();
                fields[14] = StardewTokenParser.ParseText(entry.DisplayName);

                data[objectId] = string.Join('/', fields);
            }

            return data;
        }

        /// <summary>Merge pre-1.6 data into the new asset.</summary>
        /// <param name="asset">The asset data to update.</param>
        /// <param name="from">The pre-1.6 data to merge into the asset.</param>
        /// <param name="fromBackup">A copy of <paramref name="from"/> before edits were applied.</param>
        private void MergeIntoNewFormat(IDictionary<string, WeaponData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
        {
            // remove deleted entries
            foreach (string key in asset.Keys)
            {
                if (!from.ContainsKey(key))
                {
                    if (this.NumericIdsAddedIn16.Contains(key))
                        continue; // don't remove 1.6 content for a pre-1.6 content pack (usually a load patch)

                    asset.Remove(key);
                }
            }

            // apply entries
            foreach ((string objectId, string fromEntry) in from)
            {
                // skip if unchanged
                string[]? backupFields = null;
                if (fromBackup is not null)
                {
                    if (fromBackup.TryGetValue(objectId, out string? prevRow) && prevRow == fromEntry)
                        continue; // no changes
                    backupFields = prevRow?.Split('/');
                }

                // get/add target record
                bool isNew = false;
                if (!asset.TryGetValue(objectId, out WeaponData? entry))
                {
                    isNew = true;
                    entry = new WeaponData();
                }

                // merge fields into new asset
                {
                    string[] fields = fromEntry.Split('/');

                    entry.Name = ArgUtility.Get(fields, 0, entry.Name, allowBlank: false);
                    entry.Description = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(fields, 1), ArgUtility.Get(backupFields, 1), entry.Description);
                    entry.MinDamage = ArgUtility.GetInt(fields, 2, entry.MinDamage);
                    entry.MaxDamage = ArgUtility.GetInt(fields, 3, entry.MaxDamage);
                    entry.Knockback = ArgUtility.GetFloat(fields, 4, entry.Knockback);
                    entry.Speed = ArgUtility.GetInt(fields, 5, entry.Speed);
                    entry.Precision = ArgUtility.GetInt(fields, 6, entry.Precision);
                    entry.Defense = ArgUtility.GetInt(fields, 7, entry.Defense);
                    entry.Type = ArgUtility.GetInt(fields, 8, entry.Type);
                    entry.MineBaseLevel = ArgUtility.GetInt(fields, 9, entry.MineBaseLevel);
                    entry.MineMinLevel = ArgUtility.GetInt(fields, 10, entry.MineMinLevel);
                    entry.AreaOfEffect = ArgUtility.GetInt(fields, 11, entry.AreaOfEffect);
                    entry.CritChance = ArgUtility.GetFloat(fields, 12, entry.CritChance);
                    entry.CritMultiplier = ArgUtility.GetFloat(fields, 13, entry.CritMultiplier);
                    entry.DisplayName = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(fields, 14), ArgUtility.Get(backupFields, 14), entry.DisplayName);
                }

                // set value
                if (isNew)
                    asset[objectId] = entry;
            }
        }
    }
}
