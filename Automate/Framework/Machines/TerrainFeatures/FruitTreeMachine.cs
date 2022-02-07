using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures
{
    /// <summary>A fruit tree machine that provides output.</summary>
    /// <remarks>Derived from <see cref="FruitTree.shake"/>.</remarks>
    internal class FruitTreeMachine : BaseMachine<FruitTree>
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tree">The underlying fruit tree.</param>
        /// <param name="location">The machine's in-game location.</param>
        /// <param name="tile">The tree's tile position.</param>
        public FruitTreeMachine(FruitTree tree, GameLocation location, Vector2 tile)
            : base(tree, location, BaseMachine.GetTileAreaFor(tile)) { }

        /// <summary>Get the machine's processing state.</summary>
        public override MachineState GetState()
        {
            if (this.Machine.growthStage.Value < FruitTree.treeStage)
                return MachineState.Disabled;

            return this.Machine.fruit.Count > 0
                ? MachineState.Done
                : MachineState.Processing;
        }

        /// <summary>Get the output item.</summary>
        public override ITrackedStack GetOutput()
        {
            FruitTree tree = this.Machine;

            // if struck by lightning => coal
            if (tree.struckByLightningCountdown.Value > 0)
                return new TrackedItem(ItemRegistry.Create(SObject.coalQID, tree.fruit.Count), onReduced: _ => tree.fruit.Clear());

            // else => fruit
            return new TrackedItem(tree.fruit[^1], onReduced: item => tree.fruit.Remove(item));
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public override bool SetInput(IStorage input)
        {
            return false; // no input
        }
    }
}
