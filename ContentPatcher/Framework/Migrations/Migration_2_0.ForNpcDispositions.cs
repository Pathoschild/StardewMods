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
using StardewValley.GameData.Characters;
using StardewTokenParser = StardewValley.TokenizableStrings.TokenParser;

namespace ContentPatcher.Framework.Migrations;

internal partial class Migration_2_0 : BaseRuntimeMigration
{
    /// <summary>The migration logic to apply pre-1.6 <c>Data/NPCDispositions</c> patches to <c>Data/Characters</c>.</summary>
    private class NpcDispositionsMigrator : IEditAssetMigrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The pre-1.6 asset name.</summary>
        private const string OldAssetName = "Data/NPCDispositions";

        /// <summary>The 1.6 asset name.</summary>
        private const string NewAssetName = "Data/Characters";

        /// <summary>The NPC names added in Stardew Valley 1.6.</summary>
        private readonly HashSet<string> NpcNamesAddedIn16 = ["???", "Bear", "Birdie", "Bouncer", "Gil", "Governor", "Grandpa", "Gunther", "Henchman", "Mister Qi", "Morris", "Old Mariner", "Welwick"];

        /// <summary>The vanilla data without mod edits applied, used as the base when a pre-1.6 content pack loads the asset.</summary>
        private readonly VanillaAssetFactory<Dictionary<string, CharacterData>> OriginalData = new(DataLoader.Characters);


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public bool AppliesTo(IAssetName assetName)
        {
            return assetName?.IsEquivalentTo(NpcDispositionsMigrator.OldAssetName, useBaseName: true) is true;
        }

        /// <inheritdoc />
        public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
        {
            return new AssetName(NpcDispositionsMigrator.NewAssetName, null, null);
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
            var data = asset.GetData<Dictionary<string, CharacterData>>();
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
            return new AssetName(NpcDispositionsMigrator.OldAssetName, newName.LocaleCode, newName.LanguageCode);
        }

        /// <summary>Get the pre-1.6 equivalent for the new asset data.</summary>
        /// <param name="from">The data to convert.</param>
        private Dictionary<string, string> GetOldFormat(IDictionary<string, CharacterData> from)
        {
            var data = new Dictionary<string, string>();

            string[] fields = new string[12];
            foreach ((string npcName, CharacterData entry) in from)
            {
                fields[0] = entry.Age.ToString().ToLowerInvariant();
                fields[1] = entry.Manner.ToString().ToLowerInvariant();
                fields[2] = entry.SocialAnxiety.ToString().ToLowerInvariant();
                fields[3] = entry.Optimism.ToString().ToLowerInvariant();
                fields[4] = entry.Gender.ToString().ToLowerInvariant();
                fields[5] = this.GetOldDateableField(npcName, entry.CanBeRomanced);
                fields[6] = entry.LoveInterest ?? "null";
                fields[7] = entry.HomeRegion;
                fields[8] = this.GetOldBirthdayField(entry.BirthSeason, entry.BirthDay);
                fields[9] = this.GetOldFriendsAndFamilyField(entry.FriendsAndFamily);
                fields[10] = $"default_home_position 0 0"; // the home is dynamic in 1.6, so we just need to check if the content pack changes it
                fields[11] = StardewTokenParser.ParseText(entry.DisplayName);

                data[npcName] = string.Join('/', fields);
            }

            return data;
        }

        /// <summary>Merge pre-1.6 data into the new asset.</summary>
        /// <param name="asset">The asset data to update.</param>
        /// <param name="from">The pre-1.6 data to merge into the asset.</param>
        /// <param name="fromBackup">A copy of <paramref name="from"/> before edits were applied.</param>
        private void MergeIntoNewFormat(IDictionary<string, CharacterData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
        {
            // remove deleted entries
            foreach (string key in asset.Keys)
            {
                if (!from.ContainsKey(key))
                {
                    if (this.NpcNamesAddedIn16.Contains(key))
                        continue; // don't remove 1.6 content for a pre-1.6 content pack (usually a load patch)

                    asset.Remove(key);
                }
            }

            // apply entries
            foreach ((string npcName, string fromEntry) in from)
            {
                // get/add target record
                bool isNew = false;
                if (!asset.TryGetValue(npcName, out CharacterData? entry))
                {
                    isNew = true;
                    entry = new CharacterData();
                }

                // get backup
                string[]? backupFields = null;
                if (fromBackup is not null)
                {
                    if (fromBackup.TryGetValue(npcName, out string? prevRow) && prevRow == fromEntry)
                        continue; // no changes
                    backupFields = prevRow?.Split('/');
                }

                // merge fields into new asset
                {
                    string[] fields = fromEntry.Split('/');

                    entry.Age = ArgUtility.GetEnum(fields, 0, entry.Age);
                    entry.Manner = ArgUtility.GetEnum(fields, 1, entry.Manner);
                    entry.SocialAnxiety = ArgUtility.GetEnum(fields, 2, entry.SocialAnxiety);
                    entry.Optimism = ArgUtility.GetEnum(fields, 3, entry.Optimism);
                    entry.Gender = ArgUtility.GetEnum(fields, 4, entry.Gender);

                    this.MergeDateableFieldIntoNewFormat(entry, ArgUtility.Get(fields, 5));

                    entry.LoveInterest = ArgUtility.Get(fields, 6, entry.LoveInterest);
                    if (entry.LoveInterest == "null")
                        entry.LoveInterest = null;

                    entry.HomeRegion = ArgUtility.Get(fields, 7, entry.HomeRegion);

                    string rawBirthday = ArgUtility.Get(fields, 8);
                    if (rawBirthday != ArgUtility.Get(backupFields, 8))
                        this.MergeBirthdayIntoNewFormat(entry, rawBirthday);

                    string rawFriendsAndFamily = ArgUtility.Get(fields, 9);
                    if (rawFriendsAndFamily != ArgUtility.Get(backupFields, 9))
                        this.MergeFriendsAndFamilyIntoNewFormat(entry, rawFriendsAndFamily);

                    string rawHome = ArgUtility.Get(fields, 10);
                    if (rawHome != ArgUtility.Get(backupFields, 10))
                        this.MergeHomeIntoNewFormat(entry, rawHome);

                    entry.DisplayName = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(fields, 11), ArgUtility.Get(backupFields, 11), entry.DisplayName);
                }

                // set value
                if (isNew)
                    asset[npcName] = entry;
            }
        }

        /// <summary>Get the pre-1.6 dateable field from the new asset.</summary>
        /// <param name="npcName">The internal NPC name.</param>
        /// <param name="canRomance">Whether the NPC can be romanced.</param>
        private string GetOldDateableField(string npcName, bool canRomance)
        {
            if (canRomance)
                return "datable";

            return npcName == "Krobus"
                ? "secret"
                : "not-datable";
        }

        /// <summary>Merge a pre-1.6 'dateable' field into the new asset.</summary>
        /// <param name="entry">The NPC data to update.</param>
        /// <param name="dateable">The 'dateable' field value.</param>
        private void MergeDateableFieldIntoNewFormat(CharacterData entry, string dateable)
        {
            // can romance
            switch (dateable)
            {
                case "datable":
                    entry.CanBeRomanced = true;
                    break;

                default:
                    entry.CanBeRomanced = false;
                    break;
            }
        }

        /// <summary>Get the pre-1.6 birthday field from the new asset.</summary>
        /// <param name="season">The birthday season, or <c>null</c> if the NPC has no birthday.</param>
        /// <param name="day">The birthday day.</param>
        private string GetOldBirthdayField(Season? season, int day)
        {
            return season is not null
                ? $"{season.Value.ToString().ToLowerInvariant()} {day}"
                : "null";
        }

        /// <summary>Merge a pre-1.6 'birthday' field into the new asset.</summary>
        /// <param name="entry">The NPC data to update.</param>
        /// <param name="birthday">The 'birthday' field value.</param>
        private void MergeBirthdayIntoNewFormat(CharacterData entry, string birthday)
        {
            string[] fields = ArgUtility.SplitBySpace(birthday, 2);
            string rawSeason = ArgUtility.Get(fields, 0);
            int day = ArgUtility.GetInt(fields, 1);

            if (Utility.TryParseEnum(rawSeason, out Season season))
            {
                entry.BirthSeason = season;
                entry.BirthDay = day;
            }
            else
            {
                entry.BirthSeason = null;
                entry.BirthDay = 0;
            }
        }

        /// <summary>Get the pre-1.6 'friends and family' field from the new asset.</summary>
        /// <param name="data">The 1.6 field value.</param>
        private string GetOldFriendsAndFamilyField(Dictionary<string, string> data)
        {
            if (data?.Count is not > 0)
                return string.Empty;

            StringBuilder result = new();
            foreach (var pair in data)
            {
                string npcName = pair.Key;
                string? nickname = StardewTokenParser.ParseText(pair.Value)?.Replace("'", "``");

                result
                    .Append(npcName)
                    .Append(" '")
                    .Append(nickname)
                    .Append("' ");
            }

            return result.ToString(0, result.Length - 1);
        }

        /// <summary>Merge a pre-1.6 'friends and family' field into the new asset.</summary>
        /// <param name="entry">The NPC data to update.</param>
        /// <param name="rawFriendsAndFamily">The 'friends and family' field value.</param>
        /// <remarks>This is partly derived from the 1.5.6 <c>NPC.loadCurrentDialogue()</c> code, but with added support for spaces inside the quotes (which is allowed in 1.6) and edge cases like a trailing name.</remarks>
        private void MergeFriendsAndFamilyIntoNewFormat(CharacterData entry, string rawFriendsAndFamily)
        {
            // parse field
            Dictionary<string, string> newValues = [];
            {
                int startFrom = 0;
                while (startFrom < rawFriendsAndFamily.Length - 1)
                {
                    int startNicknameIndex = rawFriendsAndFamily.IndexOf('\'', startFrom);
                    if (startNicknameIndex == -1)
                        break; // 1.5.6 would ignore a trailing name without a nickname

                    int endNicknameIndex = rawFriendsAndFamily.IndexOf('\'', startNicknameIndex + 1);
                    if (endNicknameIndex == -1)
                        endNicknameIndex = rawFriendsAndFamily.Length;

                    string name = rawFriendsAndFamily.Substring(startFrom, startNicknameIndex - startFrom);
                    string nickname = rawFriendsAndFamily.Substring(startNicknameIndex + 1, endNicknameIndex - 1 - startNicknameIndex).Replace("``", "'").Replace('_', ' '); // unescape single quotes (from 1.6 data) and spaces (in pre-1.6 format)

                    newValues[name] = nickname;
                    startFrom = endNicknameIndex + 1;
                }
            }

            // remove or update existing entries
            if (entry.FriendsAndFamily?.Count > 0)
            {
                foreach ((string npcName, string oldNickname) in entry.FriendsAndFamily)
                {
                    // remove if deleted
                    if (!newValues.TryGetValue(npcName, out string? newNickname))
                    {
                        entry.FriendsAndFamily.Remove(npcName);
                        continue;
                    }

                    // else update
                    if (newNickname != oldNickname && newNickname != StardewTokenParser.ParseText(oldNickname))
                        entry.FriendsAndFamily[npcName] = newNickname;

                    newValues.Remove(npcName);
                }
            }

            // add any remaining entries
            if (newValues.Count > 0)
            {
                entry.FriendsAndFamily ??= new();

                foreach ((string npcName, string nickname) in newValues)
                    entry.FriendsAndFamily[npcName] = nickname;
            }
        }

        /// <summary>Merge a pre-1.6 'home' field into the new asset.</summary>
        /// <param name="entry">The NPC data to update.</param>
        /// <param name="rawHome">The 'home' field value.</param>
        private void MergeHomeIntoNewFormat(CharacterData entry, string rawHome)
        {
            string[] fields = ArgUtility.SplitBySpace(rawHome);

            string locationName = fields[0];
            if (!ArgUtility.TryGetPoint(fields, 1, out Point tile, out _))
                tile = Point.Zero;
            if (!ArgUtility.TryGetInt(fields, 3, out int direction, out _))
                direction = Game1.up;

            // update existing entry
            if (entry.Home?.Count > 0)
            {
                foreach (var home in entry.Home)
                {
                    if (home.Location == locationName && GameStateQuery.IsImmutablyTrue(home.Condition))
                    {
                        home.Tile = tile;
                        return;
                    }
                }
            }

            // else reset to match
            entry.Home ??= [];
            entry.Home.Clear();
            entry.Home.Add(new CharacterHomeData
            {
                Id = "Default",
                Location = locationName,
                Tile = tile,
                Direction = direction switch
                {
                    Game1.down => nameof(Game1.down),
                    Game1.left => nameof(Game1.left),
                    Game1.right => nameof(Game1.right),
                    _ => nameof(Game1.up)
                }
            });
        }
    }
}
