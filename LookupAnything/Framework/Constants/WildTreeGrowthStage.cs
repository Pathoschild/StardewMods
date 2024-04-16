using STree = StardewValley.TerrainFeatures.Tree;

namespace Pathoschild.Stardew.LookupAnything.Framework.Constants
{
    /// <summary>Indicates a wild tree's growth stage.</summary>
    internal enum WildTreeGrowthStage
    {
        Seed = STree.seedStage,
        Sprout = STree.sproutStage,
        Sapling = STree.saplingStage,
        Bush = STree.bushStage,
        SmallTree = STree.treeStage - 1, // there is no smallTreeStage constant, so we compute this
        Tree = STree.treeStage
    }
}
