namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Provide constants for Automate logic.</summary>
    internal static class ModConstants
    {
        /// <summary>The context tag which indicates an item that can be treated as a storage container.</summary>
        public const string StorageTag = "automate_storage";

        /// <summary>Get the unqualified item IDs for big craftable items to treat as a chest by default.</summary>
        public static string[] GetDefaultChestItemIds()
        {
            return
            [
                "130", // chest,
                "232", // stone chest
                "256", // Junimo chest,
                "BigChest",
                "BigStoneChest"
            ];
        }
    }
}
