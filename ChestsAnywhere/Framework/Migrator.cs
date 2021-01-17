using System.Text.RegularExpressions;
using Pathoschild.Stardew.Automate.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>Encapsulates migrating data from legacy versions of Chests Anywhere.</summary>
    internal static class Migrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>A regular expression which matches a group of tags in the chest name.</summary>
        private const string TagGroupPattern = @"\|([^\|]+)\|";


        /*********
        ** Public methods
        *********/
        /// <summary>Migrate legacy container data, if needed.</summary>
        /// <param name="chestFactory">Encapsulates logic for finding chests.</param>
        /// <param name="dataHelper">Handles reading and storing local mod data.</param>
        public static void MigrateLegacyData(ChestFactory chestFactory, IDataHelper dataHelper)
        {
            // Migrate container options stored in the chest name from ≤1.19.8
            foreach (ManagedChest chest in chestFactory.GetChests(RangeHandler.Unlimited()))
            {
                // get underlying item
                Item item = Migrator.GetStorageItem(chest.Container);
                if (item == null)
                    continue;

                // migrate legacy data
                if (Migrator.TryParseLegacyData(item, out string originalName, out ContainerData data))
                {
                    item.Name = originalName;
                    data.ToModData(item.modData);
                }
            }

            // Migrate shipping bin options stored in the save file from ≤1.19.8
            {
                ContainerData binData = dataHelper.ReadSaveData<ContainerData>("shipping-bin");
                if (binData != null)
                {
                    Farm farm = Game1.getFarm();
                    binData.ToModData(farm.modData, discriminator: ShippingBinContainer.ModDataDiscriminator);
                    dataHelper.WriteSaveData<ContainerData>("shipping-bin", null);
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the underlying storage item from a container, if applicable.</summary>
        /// <param name="container">The container instance.</param>
        /// <returns>Returns the storage item if found, else <c>null</c>.</returns>
        private static Item GetStorageItem(IContainer container)
        {
            return container switch
            {
                ChestContainer instance => instance.Chest,
                StorageFurnitureContainer instance => instance.Furniture,
                _ => null
            };
        }

        /// <summary>Parse a serialized name string.</summary>
        /// <param name="item">The container item.</param>
        /// <param name="originalName">The original name for the container.</param>
        /// <param name="data">The parsed container data.</param>
        /// <returns>Returns whether the container has legacy data.</returns>
        private static bool TryParseLegacyData(Item item, out string originalName, out ContainerData data)
        {
            // no serialized info
            if (string.IsNullOrWhiteSpace(item?.Name))
            {
                originalName = null;
                data = null;
                return false;
            }

            // get default name
            originalName = Migrator.GetLegacyDefaultName(item);
            if (originalName == null || originalName == item.Name)
            {
                data = null;
                return false;
            }

            // parse container data
            data = new ContainerData();
            foreach (Match match in Regex.Matches(item.Name, Migrator.TagGroupPattern))
            {
                string tag = match.Groups[1].Value;

                // ignore
                if (tag.ToLower() == "ignore")
                    data.IsIgnored = true;

                // category
                else if (tag.ToLower().StartsWith("cat:"))
                    data.Category = tag.Substring(4).Trim();

                // order
                else if (int.TryParse(tag, out int order))
                    data.Order = order;

                // Automate options
                else if (tag.ToLower() == "automate:no-store")
                    data.AutomateStoreItems = AutomateContainerPreference.Disable;
                else if (tag.ToLower() == "automate:prefer-store")
                    data.AutomateStoreItems = AutomateContainerPreference.Prefer;
                else if (tag.ToLower() == "automate:no-take")
                    data.AutomateTakeItems = AutomateContainerPreference.Disable;
                else if (tag.ToLower() == "automate:prefer-take")
                    data.AutomateTakeItems = AutomateContainerPreference.Prefer;
            }

            // read display name
            string customName = Regex.Replace(item.Name, Migrator.TagGroupPattern, "").Trim();
            data.Name = !string.IsNullOrWhiteSpace(customName) && customName != originalName
                ? customName
                : null;

            return true;
        }

        /// <summary>Get the default name for an item.</summary>
        /// <param name="item">The container whose default name to get.</param>
        private static string GetLegacyDefaultName(Item item)
        {
            // This gets the original item name for items which could be edited in previous Chests
            // Anywhere versions. This deliberately does *not* support new or custom containers
            // which didn't exist at the time.
            if (item is Object obj && obj.bigCraftable.Value)
            {
                return item.ParentSheetIndex switch
                {
                    130 => "Chest",
                    216 => "Mini-Fridge",
                    _ => null
                };
            }

            if (item is Furniture furniture)
            {
                return furniture.ParentSheetIndex switch
                {
                    704 => "Oak Dresser",
                    709 => "Walnut Dresser",
                    714 => "Birch Dresser",
                    719 => "Mahogany Dresser",
                    _ => null
                };
            }

            return null;
        }
    }
}
