using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Content;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;

namespace ContentPatcher.Framework.Migrations;

internal partial class Migration_2_0 : BaseRuntimeMigration
{
    //
    // Known limitation: since we're combining two different assets, it's possible some mods added the context tags
    // in Data/ObjectContextTags before adding the objects in Data/ObjectInformation. Unfortunately we can't add
    // context tags to an object which doesn't exist yet, so those context tags will be ignored.
    //

    /// <summary>The migration logic to apply pre-1.6 <c>Data/ObjectContextTags</c> patches to <c>Data/Objects</c>.</summary>
    private class ObjectContextTagsMigrator : IEditAssetMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The pre-1.6 asset name.</summary>
        private const string OldAssetName = "Data/ObjectContextTags";

        /// <summary>The 1.6 asset name.</summary>
        private const string NewAssetName = "Data/Objects";


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool AppliesTo(IAssetName assetName)
        {
            return assetName?.IsEquivalentTo(ObjectContextTagsMigrator.OldAssetName, useBaseName: true) is true;
        }

        /// <inheritdoc />
        public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            return new AssetName(ObjectContextTagsMigrator.NewAssetName, null, null);
        }

        /// <inheritdoc />
        public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
        {
            // we can't migrate Action: Load patches because the patch won't actually contain any object data
            // besides the context tags.
            error = $"can't migrate load patches for '{ObjectContextTagsMigrator.OldAssetName}' to Stardew Valley 1.6";
            return false;
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
            return new AssetName(ObjectContextTagsMigrator.OldAssetName, newName.LocaleCode, newName.LanguageCode);
        }

        /// <summary>Get the pre-1.6 equivalent for the new asset data.</summary>
        /// <param name="asset">The data to convert.</param>
        private Dictionary<string, string> GetOldFormat(IDictionary<string, ObjectData> asset)
        {
            var data = new Dictionary<string, string>();

            foreach ((string objectId, ObjectData entry) in asset)
            {
                if (entry.Name is null)
                    continue;

                string key = this.GetOldEntryKey(objectId, entry);
                data[key] = entry.ContextTags?.Count > 0
                    ? string.Join(", ", entry.ContextTags)
                    : string.Empty;
            }

            return data;
        }

        /// <summary>Merge pre-1.6 data into the new asset.</summary>
        /// <param name="asset">The asset data to update.</param>
        /// <param name="contextTags">The pre-1.6 data to merge into the asset.</param>
        /// <param name="contextTagsBackup">A copy of <paramref name="contextTags"/> before edits were applied.</param>
        /// <param name="modId">The unique ID for the mod, used in auto-generated entry IDs.</param>
        private void MergeIntoNewFormat(IDictionary<string, ObjectData> asset, IDictionary<string, string> contextTags, IDictionary<string, string>? contextTagsBackup, string modId)
        {
            // skip if no entries changed
            // (We can't remove unchanged entries though, since we need to combine context tags by both ID and name)
            if (contextTagsBackup is not null)
            {
                bool anyChanged = false;

                foreach ((string oldKey, string rawTags) in contextTags)
                {
                    if (!contextTagsBackup.TryGetValue(oldKey, out string? prevRawTags) || prevRawTags != rawTags)
                    {
                        anyChanged = true;
                        break;
                    }
                }

                if (!anyChanged)
                    return;
            }

            // get context tags by item ID
            var contextTagsById = new Dictionary<string, HashSet<string>>();
            {
                ILookup<string, string> itemIdsByName = asset.ToLookup(p => p.Value.Name, p => p.Key);

                foreach ((string oldKey, string rawTags) in contextTags)
                {
                    string[] tags = rawTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    // add by ID
                    if (oldKey.StartsWith("id_"))
                    {
                        if (oldKey.StartsWith("id_o_"))
                        {
                            string objectId = oldKey.Substring("id_o_".Length);
                            this.TrackRawContextTagsById(contextTagsById, objectId, tags);
                        }
                    }

                    // else by name
                    else
                    {
                        foreach (string objectId in itemIdsByName[oldKey])
                            this.TrackRawContextTagsById(contextTagsById, objectId, tags);
                    }
                }
            }

            // merge into Data/Objects
            foreach ((string oldKey, HashSet<string> tags) in contextTagsById)
            {
                // get or add matching object record
                if (!asset.TryGetValue(oldKey, out ObjectData? entry))
                    continue;

                // update context tags
                if (tags.Count == 0)
                    entry.ContextTags?.Clear();
                else
                {
                    entry.ContextTags ??= [];
                    entry.ContextTags.Clear();
                    entry.ContextTags.AddRange(tags);
                }
            }
        }

        /// <summary>Add context tags to a lookup by object ID.</summary>
        /// <param name="contextTagsById">The lookup to update.</param>
        /// <param name="objectId">The object ID whose context tags to track.</param>
        /// <param name="tags">The context tags to track, in addition to any already tracked for the same object ID.</param>
        private void TrackRawContextTagsById(Dictionary<string, HashSet<string>> contextTagsById, string objectId, string[] tags)
        {
            // merge into previous
            if (contextTagsById.TryGetValue(objectId, out HashSet<string>? prevTags))
                prevTags.AddRange(tags);

            // else add new
            else
                contextTagsById[objectId] = new HashSet<string>(tags);
        }

        /// <summary>Get the entry key in <c>Data/ObjectContextTags</c> for an entry.</summary>
        /// <param name="objectId">The unique object ID.</param>
        /// <param name="entry">The object data.</param>
        private string GetOldEntryKey(string objectId, ObjectData entry)
        {
            switch (objectId)
            {
                case "113": // Chicken Statue
                case "126": // Strange Doll #1
                case "127": // Strange Doll #2
                case "340": // Honey
                case "342": // Pickles
                case "344": // Jelly
                case "348": // Wine
                case "350": // Juice
                case "447": // Aged Roe
                case "812": // Roe
                    return "id_0_" + objectId; // match pre-1.6 key

                default:
                    return entry.Name ?? "id_0_" + objectId;
            }
        }
    }
}
