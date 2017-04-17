using Pathoschild.Stardew.Automate.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Automate.Machines.TerrainFeatures
{
    /// <summary>A fruit tree machine that accepts input and provides output.</summary>
    internal class FruitTreeMachine : IMachine
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying fruit tree.</summary>
        private readonly FruitTree Tree;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tree">The underlying fruit tree.</param>
        public FruitTreeMachine(FruitTree tree)
        {
            this.Tree = tree;
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            if (this.Tree.growthStage < FruitTree.treeStage)
                return MachineState.Disabled;

            return this.Tree.fruitsOnTree > 0
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            FruitTree tree = this.Tree;

            // if struck by lightning => coal
            if (tree.struckByLightningCountdown > 0)
                return new TrackedItem(new Object(382, tree.fruitsOnTree), onReduced: this.OnOutputReduced);

            // else => fruit
            int quality = 0;
            if (tree.daysUntilMature <= -112)
                quality = 1;
            else if (tree.daysUntilMature <= -224)
                quality = 2;
            else if (tree.daysUntilMature <= -336)
                quality = 4;
            return new TrackedItem(new Object(tree.indexOfFruit, tree.fruitsOnTree, quality: quality), onReduced: this.OnOutputReduced);
        }

        /// <summary>Pull items from the connected pipes.</summary>
        /// <param name="pipes">The connected IO pipes.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool Pull(IPipe[] pipes)
        {
            return false; // no input
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the machine so it's ready to accept a new input.</summary>
        /// <param name="item">The output item that was taken.</param>
        private void OnOutputReduced(Item item)
        {
            this.Tree.fruitsOnTree = item.Stack;
        }
    }
}
