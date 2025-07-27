//
// Note: this file is shared between the Automate and Chests Anywhere projects.
//

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Provide constants for Automate logic.</summary>
    internal static class AutomateConstants
    {
        /// <summary>The context tag which indicates an item that can be treated as a storage container.</summary>
        public const string StorageTag = "automate_storage";

        /// <summary>The context tag which indicates an item can be treated as a storage container which only allows retrieving items. This requires the <see cref="StorageTag"/> tag too.</summary>
        public const string StorageTakeOnlyTag = "automate_storage_take";

        /// <summary>Get the unqualified item IDs for big craftable items to treat as a chest by default.</summary>
        public static string[] GetDefaultChestItemIds()
        {
            return
            [
                "130", // chest
                "216", // mini-fridge
                "232", // stone chest
                "256", // Junimo chest
                "BigChest",
                "BigStoneChest"
            ];
        }

        /// <summary>Get the unqualified item IDs for big craftable items to treat as a chest which only allows retrieving items by default.</summary>
        public static string[] GetTakeOnlyChestItemIds()
        {
            return
            [
                "275" // hopper
            ];
        }
    }
}
