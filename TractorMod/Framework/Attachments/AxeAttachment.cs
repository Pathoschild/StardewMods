using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the axe.</summary>
    internal class AxeAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>Whether to destroy twig debris.</summary>
        private readonly bool CutTwigs;

        /// <summary>Whether to cut down non-fruit trees.</summary>
        private readonly bool CutTrees;

        /// <summary>Whether to cut down fruit trees.</summary>
        private readonly bool CutFruitTrees;

        /// <summary>The axe upgrade levels needed to break supported resource clumps.</summary>
        /// <remarks>Derived from <see cref="ResourceClump.performToolAction"/>.</remarks>
        private readonly IDictionary<int, int> ResourceUpgradeLevelsNeeded = new Dictionary<int, int>
        {
            [ResourceClump.stumpIndex] = Tool.copper,
            [ResourceClump.hollowLogIndex] = Tool.steel
        };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public AxeAttachment(ModConfig config)
        {
            this.CutTwigs = config.AxeCutsTwigs;
            this.CutTrees = config.AxeCutsTrees;
            this.CutFruitTrees = config.AxeCutsFruitTrees;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location)
        {
            return tool is Axe;
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, SFarmer player, Tool tool, Item item, GameLocation location)
        {
            // clear twigs
            if (tileObj?.Name == "Twig")
                return this.CutTwigs && this.UseToolOnTile(tool, tile);

            // cut non-fruit tree
            if (tileFeature is Tree)
                return this.CutTrees && this.UseToolOnTile(tool, tile);

            // cut fruit tree
            if (tileFeature is FruitTree)
                return this.CutFruitTrees && this.UseToolOnTile(tool, tile);

            // clear dead crops
            if (tileFeature is HoeDirt dirt && dirt.crop?.dead == true)
                return this.UseToolOnTile(tool, tile);

            // clear stumps
            // This needs to check if the axe upgrade level is high enough first, to avoid spamming
            // 'need to upgrade your tool' messages. Based on ResourceClump.performToolAction.
            {
                Rectangle tileArea = this.GetAbsoluteTileArea(tile);
                ResourceClump stump =
                    (
                        from clump in this.GetResourceClumps(location)
                        where
                            clump.getBoundingBox(clump.tile).Intersects(tileArea)
                            && this.ResourceUpgradeLevelsNeeded.ContainsKey(clump.parentSheetIndex)
                            && tool.upgradeLevel >= this.ResourceUpgradeLevelsNeeded[clump.parentSheetIndex]
                        select clump
                    )
                    .FirstOrDefault();
                if (stump != null)
                    this.UseToolOnTile(tool, tile);
            }

            return false;
        }
    }
}
