using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures
{
    /// <summary>A fruit tree machine that accepts input and provides output.</summary>
    /// <remarks>Derived from <see cref="FruitTree.shake"/>.</remarks>
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
                return new TrackedItem(new SObject(382, tree.fruitsOnTree), onReduced: this.OnOutputReduced);

            // else => fruit
            int quality = SObject.lowQuality;
            if (tree.daysUntilMature <= -112)
                quality = SObject.medQuality;
            if (tree.daysUntilMature <= -224)
                quality = SObject.highQuality;
            if (tree.daysUntilMature <= -336)
                quality = SObject.bestQuality;
            return new TrackedItem(new SObject(tree.indexOfFruit, tree.fruitsOnTree, quality: quality), onReduced: this.OnOutputReduced);
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
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
