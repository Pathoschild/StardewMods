using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Common
{
    /// <summary>Provides utility methods for working with machine data in <c>Data/Buildings</c> and <c>Data/Machines</c>.</summary>
    public static class MachineDataHelper
    {
        /*********
        ** Building data
        *********/
        /// <summary>Get the building chest names referenced by a building's item conversion rules.</summary>
        /// <param name="data">The building data.</param>
        /// <param name="inputChests">The input chest names found in the conversion rules.</param>
        /// <param name="outputChests">The output chest names found in the conversion rules.</param>
        public static void GetBuildingChestNames(BuildingData? data, ISet<string> inputChests, ISet<string> outputChests)
        {
            if (data?.ItemConversions?.Count is not > 0)
                return;

            foreach (BuildingItemConversion? rule in data.ItemConversions)
            {
                if (rule?.SourceChest is not null)
                    inputChests.Add(rule.SourceChest);

                if (rule?.DestinationChest is not null)
                    outputChests.Add(rule.DestinationChest);
            }
        }

        /// <summary>Get the building chest names referenced by a building's item conversion rules.</summary>
        /// <param name="data">The building data.</param>
        /// <param name="inputChests">The input chest names found in the conversion rules.</param>
        /// <param name="outputChests">The output chest names found in the conversion rules.</param>
        /// <returns>Returns whether any input or output chests were found.</returns>
        public static bool TryGetBuildingChestNames(BuildingData? data, out ISet<string> inputChests, out ISet<string> outputChests)
        {
            inputChests = new HashSet<string>();
            outputChests = new HashSet<string>();

            MachineDataHelper.GetBuildingChestNames(data, inputChests, outputChests);

            return inputChests.Count > 0 || outputChests.Count > 0;
        }

        /// <summary>Get the building chests which match a set of chest names.</summary>
        /// <param name="building">The building whose chests to get.</param>
        /// <param name="chestNames">The chest names to match.</param>
        public static IEnumerable<Chest> GetBuildingChests(Building building, ISet<string> chestNames)
        {
            foreach (Chest chest in building.buildingChests)
            {
                if (chestNames.Contains(chest.Name))
                    yield return chest;
            }
        }


        /*********
        ** Context tags
        *********/
        /// <summary>Get the item data matching a context tag, if the tag uniquely identifies one.</summary>
        /// <param name="contextTag">The context tag to match.</param>
        /// <param name="data">The item data, if found.</param>
        /// <returns>Returns whether the <paramref name="data"/> was set to a unique item match.</returns>
        public static bool TryGetUniqueItemFromContextTag(string contextTag, [NotNullWhen(true)] out ParsedItemData? data)
        {
            if (contextTag.StartsWith("id_"))
            {
                string rawIdentifier = contextTag[3..];
                string? qualifiedId = null;

                // extract qualified item ID
                if (rawIdentifier.StartsWith('('))
                    qualifiedId = rawIdentifier;
                else
                {
                    string[] parts = rawIdentifier.Split('_', 2);

                    foreach (IItemDataDefinition type in ItemRegistry.ItemTypes)
                    {
                        if (string.Equals(parts[0], type.StandardDescriptor, StringComparison.InvariantCultureIgnoreCase))
                        {
                            qualifiedId = type.Identifier + parts[1];
                            break;
                        }
                    }
                }

                // get data if valid
                data = ItemRegistry.GetData(qualifiedId);
                return data != null;
            }

            data = null;
            return false;
        }


        /*********
        ** Game state queries
        *********/
        /// <summary>Get the item data matching a game state query, if the query uniquely identifies one.</summary>
        /// <param name="query">The context tag to match.</param>
        /// <param name="data">The item data, if found.</param>
        /// <returns>Returns whether the <paramref name="data"/> was set to a unique item match.</returns>
        public static bool TryGetUniqueItemFromGameStateQuery(string query, [NotNullWhen(true)] out ParsedItemData? data)
        {
            data = null;

            foreach (GameStateQuery.ParsedGameStateQuery condition in GameStateQuery.Parse(query))
            {
                if (condition.Error is not null)
                    continue;

                string queryName = ArgUtility.Get(condition.Query, 0);

                // handle ITEM_ID
                if (queryName.EqualsIgnoreCase(nameof(GameStateQuery.DefaultResolvers.ITEM_ID)))
                {
                    if (condition.Query.Length == 3 && condition.Query[1].EqualsIgnoreCase("Input"))
                    {
                        string itemId = ItemRegistry.QualifyItemId(condition.Query[2]);

                        if (data is null)
                            data = ItemRegistry.GetData(itemId);
                        else if (data.QualifiedItemId != itemId)
                        {
                            // conflicting ID queries
                            data = null;
                            return false;
                        }
                    }
                }

                // handle ITEM_CONTEXT_TAG
                else if (queryName.EqualsIgnoreCase(nameof(GameStateQuery.DefaultResolvers.ITEM_CONTEXT_TAG)))
                {
                    if (condition.Query.Length == 3 && condition.Query[1].EqualsIgnoreCase("Input"))
                    {
                        if (MachineDataHelper.TryGetUniqueItemFromContextTag(condition.Query[2], out ParsedItemData? itemData))
                        {
                            if (data is null)
                                data = itemData;

                            else if (data.QualifiedItemId != itemData.QualifiedItemId)
                            {
                                // conflicting context tag queries
                                data = null;
                                return false;
                            }
                        }
                    }
                }
            }

            return data != null;
        }
    }
}
