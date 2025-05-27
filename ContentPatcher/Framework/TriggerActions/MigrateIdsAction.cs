using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Triggers;
using SObject = StardewValley.Object;

namespace ContentPatcher.Framework.TriggerActions;

/// <summary>Implements the <c>Pathoschild.ContentPatcher_MigrateIds</c> trigger action.</summary>
internal class MigrateIdsAction
{
    /*********
    ** Fields
    *********/
    /// <summary>The Json Assets mapped ID types, with their corresponding <see cref="ItemRegistry"/> data types.</summary>
    private readonly Dictionary<string, string[]> JsonAssetsTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["big-craftables"] = [ItemRegistry.type_bigCraftable],
        ["clothing"] = [ItemRegistry.type_pants, ItemRegistry.type_shirt],
        ["hats"] = [ItemRegistry.type_hat],
        ["objects"] = [ItemRegistry.type_object],
        ["weapons"] = [ItemRegistry.type_weapon]
    };


    /*********
    ** Public methods
    *********/
    /// <summary>Handle the action when it's called by the game.</summary>
    /// <inheritdoc cref="TriggerActionDelegate" />
    public bool Handle(string[] args, TriggerActionContext context, [NotNullWhen(false)] out string? error)
    {
        // validate context
        // We need to migrate IDs everywhere, including in non-synced locations and on farmhand fields that can't
        // be edited remotely. That's only possible when run on the host before any other players have connected.
        if (context.Data is null)
        {
            error = "this action must be run via Data/TriggerActions";
            return false;
        }
        if (!context.Data.HostOnly || !string.Equals(context.Data.Trigger?.Trim(), TriggerActionManager.trigger_dayStarted))
        {
            error = $"this action must be run with `\"{nameof(context.Data.HostOnly)}\": true` and `\"{nameof(context.Data.Trigger)}: \"{TriggerActionManager.trigger_dayStarted}\"`";
            return false;
        }

        // get ID type
        if (!ArgUtility.TryGetEnum(args, 1, out MigrateIdType type, out error))
            return false;

        // get old => new IDs
        var mapIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 2; i < args.Length; i += 2)
        {
            if (!ArgUtility.TryGet(args, i, out string oldId, out error, allowBlank: false))
                return false;
            if (!ArgUtility.TryGet(args, i + 1, out string newId, out error, allowBlank: false))
            {
                if (!ArgUtility.HasIndex(args, i + 1))
                    error = $"index {i} with old ID \"{oldId}\" doesn't have a corresponding new ID at index {i + 1}";
                return false;
            }

            mapIds[oldId] = newId;
        }

        // apply
        Farmer[] players = Game1.getAllFarmers().ToArray();
        switch (type)
        {
            case MigrateIdType.CookingRecipes:
                return this.TryMigrateCookingRecipeIds(players, mapIds, out error);

            case MigrateIdType.CraftingRecipes:
                return this.TryMigrateCraftingRecipeIds(players, mapIds, out error);

            case MigrateIdType.Events:
                return this.TryMigrateEventIds(players, mapIds, out error);

            case MigrateIdType.Items:
                return this.TryMigrateItemIds(mapIds, out error);

            case MigrateIdType.Mail:
                return this.TryMigrateMailIds(players, mapIds, out error);

            case MigrateIdType.Songs:
                return this.TryMigrateSongIds(players, mapIds, out error);

            default:
                error = $"required index 1 has unknown ID type '{type}'";
                return false;
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Try to migrate cooking recipe IDs.</summary>
    /// <param name="players">The players to edit.</param>
    /// <param name="mapIds">The old and new IDs to map.</param>
    /// <param name="error">An error indicating why the migration failed.</param>
    private bool TryMigrateCookingRecipeIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
    {
        foreach (Farmer player in players)
        {
            // note: we iterate deliberately so keys are matched case-insensitively

            foreach ((string oldKey, int oldValue) in player.cookingRecipes.Pairs.ToArray())
            {
                if (mapIds.TryGetValue(oldKey, out string? newKey))
                {
                    player.cookingRecipes.Remove(oldKey);
                    player.cookingRecipes.TryAdd(newKey, oldValue);
                }
            }

            foreach ((string oldKey, int oldValue) in player.craftingRecipes.Pairs.ToArray())
            {
                if (mapIds.TryGetValue(oldKey, out string? newKey))
                {
                    player.craftingRecipes.Remove(oldKey);
                    player.craftingRecipes.TryAdd(newKey, oldValue);
                }
            }
        }

        error = null;
        return true;
    }

    /// <summary>Try to migrate crafting recipe IDs.</summary>
    /// <param name="players">The players to edit.</param>
    /// <param name="mapIds">The old and new IDs to map.</param>
    /// <param name="error">An error indicating why the migration failed.</param>
    private bool TryMigrateCraftingRecipeIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
    {
        foreach (Farmer player in players)
        {
            foreach ((string oldKey, int oldValue) in player.craftingRecipes.Pairs.ToArray())
            {
                if (mapIds.TryGetValue(oldKey, out string? newKey))
                {
                    player.craftingRecipes.Remove(oldKey);
                    player.craftingRecipes.TryAdd(newKey, oldValue);
                }
            }
        }

        error = null;
        return true;
    }

    /// <summary>Try to migrate event IDs.</summary>
    /// <param name="players">The players to edit.</param>
    /// <param name="mapIds">The old and new IDs to map.</param>
    /// <param name="error">An error indicating why the migration failed.</param>
    private bool TryMigrateEventIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
    {
        foreach (Farmer player in players)
        {
            foreach (string oldId in player.eventsSeen.ToArray())
            {
                if (mapIds.TryGetValue(oldId, out string? newId))
                {
                    player.eventsSeen.Remove(oldId);
                    player.eventsSeen.Add(newId);
                }
            }
        }

        error = null;
        return true;
    }

    /// <summary>Try to migrate item IDs.</summary>
    /// <param name="mapRawIds">The old and new IDs to map.</param>
    /// <param name="error">An error indicating why the migration failed.</param>
    private bool TryMigrateItemIds(IDictionary<string, string> mapRawIds, [NotNullWhen(false)] out string? error)
    {
        // validate & index item IDs
        var mapQualifiedIds = new Dictionary<string, ItemMetadata>();
        var mapLocalObjectIds = new Dictionary<string, ItemMetadata>();
        var jsonAssetMap = new Lazy<Dictionary<string, Dictionary<string, string>>>(this.LoadJsonAssetsMap);
        foreach ((string oldId, string newId) in mapRawIds)
        {
            // get new data
            ItemMetadata newData = ItemRegistry.ResolveMetadata(newId);
            if (newData is null)
            {
                error = $"the new item ID \"{newId}\" doesn't match an existing item";
                return false;
            }

            // map Json Assets ID
            if (this.TryReadJsonAssetsId(oldId, jsonAssetMap, out string[] oldIds, out error))
            {
                foreach (string id in oldIds)
                {
                    mapQualifiedIds[id] = newData;

                    if (newData.TypeIdentifier == ItemRegistry.type_object)
                    {
                        string localId = ItemRegistry.ManuallyQualifyItemId(id, "", true);
                        mapLocalObjectIds[localId] = newData;
                    }
                }
                continue;
            }
            if (error != null)
                return false;

            // else map qualified ID
            {
                if (!ItemRegistry.IsQualifiedItemId(oldId))
                {
                    error = $"the old item ID \"{oldId}\" must be a qualified item ID (like {ItemRegistry.type_object}{oldId}) or a Json Assets ID in the form \"JsonAssets:<type>:<name>\"";
                    return false;
                }

                mapQualifiedIds[oldId] = newData;

                if (newData.TypeIdentifier == ItemRegistry.type_object)
                {
                    string oldLocalId = ItemRegistry.ManuallyQualifyItemId(oldId, "", true);
                    mapLocalObjectIds[oldLocalId] = newData;
                }
            }
        }

        // migrate items
        Utility.ForEachItem(item =>
        {
            if (mapQualifiedIds.TryGetValue(item.QualifiedItemId, out ItemMetadata? data))
            {
                if (item.ParentSheetIndex == 0 || (int.TryParse(item.ItemId, out int oldIndex) && item.ParentSheetIndex == oldIndex))
                    item.ParentSheetIndex = data.GetParsedData()?.SpriteIndex ?? item.ParentSheetIndex;

                item.ItemId = data.LocalItemId;

                if (item is SObject obj)
                    obj.reloadSprite();
            }

            return true;
        });

        // migrate indirect references
        foreach (Farmer player in Game1.getAllFarmers())
        {
            // artifacts (unqualified IDs)
            foreach ((string oldId, int[] oldValue) in player.archaeologyFound.Pairs.ToArray())
            {
                if (mapLocalObjectIds.TryGetValue(oldId, out ItemMetadata? data))
                {
                    player.archaeologyFound.Remove(oldId);
                    player.archaeologyFound.TryAdd(data.LocalItemId, oldValue);
                }
            }

            // fish caught (qualified IDs)
            foreach ((string oldId, int[] oldValue) in player.fishCaught.Pairs.ToArray())
            {
                if (mapQualifiedIds.TryGetValue(oldId, out ItemMetadata? data))
                {
                    player.fishCaught.Remove(oldId);
                    player.fishCaught.TryAdd(data.QualifiedItemId, oldValue);
                }
            }

            // gifted items (unqualified IDs)
            foreach (SerializableDictionary<string, int> giftedItems in player.giftedItems.Values)
            {
                foreach ((string oldId, int oldValue) in giftedItems.ToArray())
                {
                    if (mapLocalObjectIds.TryGetValue(oldId, out ItemMetadata? data))
                    {
                        giftedItems.Remove(oldId);
                        giftedItems.TryAdd(data.LocalItemId, oldValue);
                    }
                }
            }

            // minerals (unqualified IDs)
            foreach ((string oldId, int oldValue) in player.mineralsFound.Pairs.ToArray())
            {
                if (mapLocalObjectIds.TryGetValue(oldId, out ItemMetadata? data))
                {
                    player.mineralsFound.Remove(oldId);
                    player.mineralsFound.TryAdd(data.LocalItemId, oldValue);
                }
            }

            // shipped (unqualified IDs)
            foreach ((string oldId, int oldValue) in player.basicShipped.Pairs.ToArray())
            {
                if (mapLocalObjectIds.TryGetValue(oldId, out ItemMetadata? data))
                {
                    player.basicShipped.Remove(oldId);
                    player.basicShipped.TryAdd(data.LocalItemId, oldValue);
                }
            }

            // tailored (IDs in legacy 'standard description' format)
            foreach ((string oldTailoredId, int oldValue) in player.tailoredItems.Pairs.ToArray())
            {
#pragma warning disable CS0618 // deliberately using obsolete methods used by tailoredItems

                Item oldItem = Utility.getItemFromStandardTextDescription(oldTailoredId, Game1.player);

                if (oldItem != null && mapQualifiedIds.TryGetValue(oldItem.QualifiedItemId, out ItemMetadata? data))
                {
                    string newTailoredId = Utility.getStandardDescriptionFromItem(data.TypeIdentifier, data.LocalItemId, false, false, 1);

                    player.tailoredItems.Remove(oldTailoredId);
                    player.tailoredItems.TryAdd(newTailoredId, oldValue);
                }
#pragma warning restore CS0618
            }
        }

        error = null;
        return true;
    }

    /// <summary>Try to migrate mail IDs.</summary>
    /// <param name="players">The players to edit.</param>
    /// <param name="mapIds">The old and new IDs to map.</param>
    /// <param name="error">An error indicating why the migration failed.</param>
    private bool TryMigrateMailIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
    {
        foreach (Farmer player in players)
        {
            // received
            foreach (string oldId in player.mailReceived.ToArray())
            {
                if (mapIds.TryGetValue(oldId, out string? newId))
                {
                    player.mailReceived.Remove(oldId);
                    player.mailReceived.Add(newId);
                }
            }

            // in mailbox
            for (int i = 0; i < player.mailbox.Count; i++)
            {
                if (mapIds.TryGetValue(player.mailbox[i], out string? newId))
                {
                    player.mailbox.RemoveAt(i);
                    player.mailbox.Insert(i, newId);
                }
            }

            // queued for tomorrow
            foreach (string oldId in player.mailForTomorrow.ToArray())
            {
                if (mapIds.TryGetValue(oldId, out string? newId))
                {
                    player.mailForTomorrow.Remove(oldId);
                    player.mailForTomorrow.Add(newId);
                }
            }
        }

        error = null;
        return true;
    }

    /// <summary>Try to migrate song IDs.</summary>
    /// <param name="players">The players to edit.</param>
    /// <param name="mapIds">The old and new IDs to map.</param>
    /// <param name="error">An error indicating why the migration failed.</param>
    private bool TryMigrateSongIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
    {
        foreach (Farmer player in players)
        {
            foreach (string oldId in player.songsHeard.ToArray())
            {
                if (mapIds.TryGetValue(oldId, out string? newId))
                {
                    player.songsHeard.Remove(oldId);
                    player.songsHeard.Add(newId);
                }
            }
        }

        error = null;
        return true;
    }

    /// <summary>Load the Json Assets ID map for the current save, indexed by entity type.</summary>
    private Dictionary<string, Dictionary<string, string>> LoadJsonAssetsMap()
    {
        var data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (string type in this.JsonAssetsTypes.Keys)
        {
            Dictionary<string, string>? map = null;

            try
            {
                string path = Path.Combine(StardewModdingAPI.Constants.CurrentSavePath!, "JsonAssets", $"ids-{type}.json");

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
            }
            catch
            {
                // if the ID map is invalid, just ignore it
            }

            data[type] = map.ToNonNullCaseInsensitive();
        }

        return data;
    }

    /// <summary>Try to map a raw ID as a Json Assets item specifier.</summary>
    /// <param name="rawId">The raw ID to parse.</param>
    /// <param name="jsonAssetsMap">The Json Assets ID map loaded via <see cref="LoadJsonAssetsMap"/>.</param>
    /// <param name="oldIds">The old IDs in the save file to match, if found.</param>
    /// <param name="error">The error message indicating why the format is invalid, if applicable.</param>
    /// <returns>Returns whether the ID was successfully parsed as a Json Assets item specifier, regardless of whether it was found in the Json Assets ID map.</returns>
    private bool TryReadJsonAssetsId(string rawId, Lazy<Dictionary<string, Dictionary<string, string>>> jsonAssetsMap, out string[] oldIds, out string? error)
    {
        // not a Json Assets ID
        if (rawId?.StartsWith("JsonAssets", StringComparison.OrdinalIgnoreCase) is not true)
        {
            oldIds = [];
            error = null;
            return false;
        }

        // extract parts
        string type, name;
        {
            string[] parts = rawId.Split(':', 3, StringSplitOptions.TrimEntries);
            if (parts.Length != 3)
            {
                oldIds = [];
                error = $"the old item ID \"{rawId}\" is not a valid Json Assets item specifier. It must have the form \"JsonAssets:<type>:<name>\", where the type is one of [{string.Join(", ", this.JsonAssetsTypes.Keys)}].";
                return false;
            }

            type = parts[1];
            name = parts[2];
        }

        // get mapped item ID types
        if (!this.JsonAssetsTypes.TryGetValue(type, out string[]? typeIds))
        {
            oldIds = [];
            error = $"the old item ID \"{rawId}\" has invalid Json Assets type '{type}'. This must be one of [{string.Join(", ", this.JsonAssetsTypes.Keys)}].";
            return false;
        }

        // get real qualified item IDs (if any)
        oldIds = jsonAssetsMap.Value.TryGetValue(type, out Dictionary<string, string>? map) && map.TryGetValue(name, out string? newId)
            ? typeIds.Select(prefix => prefix + newId).ToArray()
            : [];
        error = null;
        return true;
    }
}
