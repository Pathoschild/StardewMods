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

        /// <summary>Get the unqualified item IDs for big craftable items to treat as a chest by default.</summary>
        public static string[] GetDefaultChestItemIds()
        {
            return
            [
                "130", // chest,
                "216", // mini-fridge
                "232", // stone chest
                "256", // Junimo chest
                "BigChest",
                "BigStoneChest"
            ];
        }
    }
}
