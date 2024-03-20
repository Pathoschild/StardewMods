using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Enums;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;
using StardewValley.GameData.WildTrees;
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
        private readonly Cached<Stack<string>> ItemDrops;


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
            this.ItemDrops = new Cached<Stack<string>>(
                getCacheKey: () => $"{Game1.season},{Game1.dayOfMonth},{tree.hasSeed.Value}",
                fetchNew: () => new Stack<string>()
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
        public override ITrackedStack? GetOutput()
        {
            Tree tree = this.Machine;

            // recalculate drop queue
            var drops = this.ItemDrops.Value;
            if (!drops.Any())
            {
                // seed must be last item dropped, since collecting the seed will reset the stack
                string? seedId = TreeMachine.GetSeedForTree(tree, this.Location);
                if (seedId != null)
                    drops.Push(seedId);

                // get extra drops
                foreach (string itemId in this.GetRandomExtraDrops())
                    drops.Push(itemId);
            }

            // get next drop
            return drops.Any()
                ? new TrackedItem(ItemRegistry.Create(drops.Peek()), onReduced: this.OnOutputReduced)
                : null;
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
        /// <param name="location">The location containing the tree.</param>
        public static bool CanAutomate(Tree tree, GameLocation location)
        {
            return TreeMachine.GetSeedForTree(tree, location) != null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void OnOutputReduced(Item item)
        {
            Tree tree = this.Machine;

            if (ItemRegistry.HasItemId(item, TreeMachine.GetSeedForTree(tree, this.Location)))
                tree.hasSeed.Value = false;

            Stack<string> drops = this.ItemDrops.Value;
            if (drops.Any() && drops.Peek() == item.ItemId)
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
        private IEnumerable<string> GetRandomExtraDrops()
        {
            Tree tree = this.Machine;
            string type = tree.treeType.Value;

            // golden coconut
            if ((type == TreeType.Palm || type == TreeType.Palm2) && this.Location is IslandLocation && new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + this.TileArea.X * 13 + this.TileArea.Y * 54).NextDouble() < 0.1)
                yield return "791";

            // Qi bean
            if (Game1.random.NextDouble() <= 0.5 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
                yield return "890";

        }

        /// <summary>Get the seed ID dropped by a tree, regardless of whether it currently has a seed.</summary>
        /// <param name="tree">The tree instance.</param>
        /// <param name="location">The location containing the tree.</param>
        /// <remarks>Derived from <see cref="Tree.shake"/>.</remarks>
        private static string? GetSeedForTree(Tree tree, GameLocation location)
        {
            WildTreeData? data = tree.GetData();

            string? seed = data?.SeedItemId;
            if (Game1.GetSeasonForLocation(location) == Season.Fall && seed == "309"/*acorn*/ && Game1.dayOfMonth >= 14)
                seed = "408";/*hazelnut*/

            return seed;
        }
    }
}
