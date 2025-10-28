using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures;

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

    /// <inheritdoc />
    public override MachineState GetState()
    {
        if (this.Machine.growthStage.Value < FruitTree.treeStage)
            return MachineState.Disabled;

        return this.Machine.fruit.Count > 0
            ? MachineState.Done
            : MachineState.Processing;
    }

    /// <inheritdoc />
    public override ITrackedStack GetOutput()
    {
        FruitTree tree = this.Machine;

        // if struck by lightning => coal
        if (tree.struckByLightningCountdown.Value > 0)
            return new TrackedItem(ItemRegistry.Create(SObject.coalQID, tree.fruit.Count)).OnReduced((_, _) => tree.fruit.Clear());

        // else => fruit
        return new TrackedItem(tree.fruit[^1]).OnReduced((_, item) => tree.fruit.Remove(item));
    }

    /// <inheritdoc />
    public override bool SetInput(IStorage input)
    {
        return false; // no input
    }
}
