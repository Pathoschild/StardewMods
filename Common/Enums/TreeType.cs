using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Enums
{
    /// <summary>Indicates a tree type.</summary>
    internal static class TreeType
    {
        /// <summary>The tree type ID for a big mushroom tree.</summary>
        public static string BigMushroom { get; } = Tree.mushroomTree.ToString();

        /// <summary>The tree type ID for a mahogany tree.</summary>
        public static string Mahogany { get; } = Tree.mahoganyTree.ToString();

        /// <summary>The tree type ID for a maple tree.</summary>
        public static string Maple { get; } = Tree.leafyTree.ToString();

        /// <summary>The tree type ID for an oak tree.</summary>
        public static string Oak { get; } = Tree.bushyTree.ToString();

        /// <summary>The tree type ID for a palm tree.</summary>
        public static string Palm { get; } = Tree.palmTree.ToString();

        /// <summary>The tree type ID for a palm2 tree.</summary>
        public static string Palm2 { get; } = Tree.palmTree2.ToString();

        /// <summary>The tree type ID for a pine tree.</summary>
        public static string Pine { get; } = Tree.pineTree.ToString();
    }
}
