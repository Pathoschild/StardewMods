using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Enums;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures
{
    /// <summary>A tree machine that provides output.</summary>
    /// <remarks>Derived from <see cref="Tree.shake"/>.</remarks>
    internal class TreeMachine : BaseMachine<Tree>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The items to drop.</summary>
        private readonly Cached<Stack<int>> ItemDrops;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tree">The underlying tree.</param>
        /// <param name="location">The machine's in-game location.</param>
        /// <param name="tile">The tree's tile position.</param>
        public TreeMachine(Tree tree, GameLocation location, Vector2 tile)
            : base(tree, location, BaseMachine.GetTileAreaFor(tile))
        {
            this.ItemDrops = new Cached<Stack<int>>(
                getCacheKey: () => $"{Game1.currentSeason},{Game1.dayOfMonth},{tree.hasSeed.Value}",
                fetchNew: () => new Stack<int>()
            );
        }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.growthStage.Value < Tree.treeStage || this.Machine.stump.Value)
                return MachineState.Disabled;

            return this.HasSeed()
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            Tree tree = this.Machine;

            // recalculate drop queue
            var drops = this.ItemDrops.Value;
            if (!drops.Any())
            {
                drops.Push(TreeMachine.GetSeedForTree(tree)!.Value); // must be last item dropped, since collecting the seed will reset the stack
                foreach (int itemId in this.GetRandomExtraDrops())
                    drops.Push(itemId);
            }

            // get next drop
            return new TrackedItem(new SObject(drops.Peek(), 1), onReduced: this.OnOutputReduced);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input
        }

        /// <summary>Get whether a tree can be automated.</summary>
        /// <param name="tree">The tree to automate.</param>
        public static bool CanAutomate(Tree tree)
        {
            return TreeMachine.GetSeedForTree(tree) != null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void OnOutputReduced(Item item)
        {
            var tree = this.Machine;

            if (item.ParentSheetIndex == TreeMachine.GetSeedForTree(tree))
                tree.hasSeed.Value = false;

            var drops = this.ItemDrops.Value;
            if (drops.Any() && drops.Peek() == item.ParentSheetIndex)
                drops.Pop();
        }

        /// <summary>Get whether the tree has a seed to drop.</summary>
        private bool HasSeed()
        {
            return
                this.Machine.hasSeed.Value
                && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1);
        }

        /// <summary>Get the random items that should also drop when this tree has a seed.</summary>
        private IEnumerable<int> GetRandomExtraDrops()
        {
            Tree tree = this.Machine;
            TreeType type = (TreeType)tree.treeType.Value;

            // golden coconut
            if ((type == TreeType.Palm || type == TreeType.Palm2) && this.Location is IslandLocation && new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + this.TileArea.X * 13 + this.TileArea.Y * 54).NextDouble() < 0.1)
                yield return 791;

            // Qi bean
            if (Game1.random.NextDouble() <= 0.5 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                yield return 890;

        }

        /// <summary>Get the seed ID dropped by a tree, regardless of whether it currently has a seed.</summary>
        /// <param name="tree">The tree instance.</param>
        private static int? GetSeedForTree(Tree tree)
        {
            TreeType type = (TreeType)tree.treeType.Value;
            return type switch
            {
                TreeType.Oak => 309, // acorn
                TreeType.Maple => Game1.GetSeasonForLocation(tree.currentLocation) == "fall" && Game1.dayOfMonth >= 14
                    ? 408 // hazelnut
                    : 310, // maple seed
                TreeType.Pine => 311, // pine code
                TreeType.Palm => 88, // coconut
                TreeType.Palm2 => 88, // coconut
                TreeType.Mahogany => 292, // mahogany seed
                _ => null
            };
        }
    }
}
